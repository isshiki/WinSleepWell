param (
    [switch]$install,
    [switch]$uninstall
)

# Change directory to the script's location (wherever the script is located)
Set-Location -Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

# Determine if we are running from within the bin\Release directory
$isInReleaseDir = (Test-Path -Path "App\WinSleepWell.exe") -and (Test-Path -Path "Service\WinSleepWellService.exe")

# If running from bin\Release, adjust paths accordingly
if ($isInReleaseDir) {
    $appExePath = "App\WinSleepWell.exe"
    $appWorkingDir = "App"
    $svcExePath = "Service\WinSleepWellService.exe"
} else {
    $binPath = "bin"
    $appExePath = Join-Path $binPath "Release\App\WinSleepWell.exe"
    $appWorkingDir = Join-Path $binPath "Release\App"
    $svcExePath = Join-Path $binPath "Release\Service\WinSleepWellService.exe"
}

# Convert to full paths
$fullAppExePath = Resolve-Path $appExePath | Select-Object -ExpandProperty Path
$fullAppWorkingDir = Resolve-Path $appWorkingDir | Select-Object -ExpandProperty Path
$fullSvcExePath = Resolve-Path $svcExePath | Select-Object -ExpandProperty Path

# Service and TaskScheduler settings
$serviceName = "WinSleepWellService"
$taskName = "WinSleepWell"

# Descriptions for the service
$serviceDescriptionEn = "Service to appropriately re-sleep Windows."
$serviceDescriptionJa = "Windowsを適切に再スリープさせるためのサービスです。"

# Determine the system's display language
$language = (Get-Culture).Name

# Choose the service description based on the system language
if ($language -eq "ja-JP") {
    $serviceDescription = $serviceDescriptionJa
} else {
    $serviceDescription = $serviceDescriptionEn
}

function Install-ServiceAndTask() {
    # Check if the service already exists
    if (Get-Service -Name $serviceName -ErrorAction SilentlyContinue) {
        Write-Host "Service '$serviceName' is already installed."
    } else {
        # Register the service using sc command
        $createService = sc.exe create $serviceName binPath= "$fullSvcExePath" DisplayName= "WinSleepWell Service" start= auto obj= LocalSystem
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Service '$serviceName' installed successfully."
        } else {
            Write-Host "Failed to install service '$serviceName'. Exit code: $LASTEXITCODE"
            Write-Host $createService
            return  # If an error occurs, no further processing is performed
        }

        # Add description to the service
        $addDescription = sc.exe description $serviceName "$serviceDescription"
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Service description added."
        } else {
            Write-Host "Failed to add description to service '$serviceName'. Exit code: $LASTEXITCODE"
            Write-Host $addDescription
            return  # If an error occurs, no further processing is performed
        }

        # Start the service automatically
        $startService = sc.exe start $serviceName
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Service '$serviceName' started successfully."
        } else {
            Write-Host "Failed to start service '$serviceName'. Exit code: $LASTEXITCODE"
            Write-Host $startService
        }
    }

    # Check if the TaskScheduler task already exists
    if (schtasks /Query /TN $taskName /FO LIST /V 2>$null) {
        Write-Host "Task '$taskName' is already installed."
    } else {
        # XML content for the Task Scheduler
        $taskXml = @"
<?xml version="1.0" encoding="UTF-16"?>
<Task version="1.4" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
  <RegistrationInfo>
    <Date>$(Get-Date -Format s)</Date>
    <Author>$env:COMPUTERNAME\$env:USERNAME</Author>
    <Description>Start WinSleepWell at logon with admin rights</Description>
    <URI>\$taskName</URI>
  </RegistrationInfo>
  <Triggers>
    <LogonTrigger>
      <Enabled>true</Enabled>
    </LogonTrigger>
  </Triggers>
  <Principals>
    <Principal id="Author">
      <UserId>$env:USERDOMAIN\$env:USERNAME</UserId>
      <LogonType>InteractiveToken</LogonType>
      <RunLevel>HighestAvailable</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
    <AllowHardTerminate>true</AllowHardTerminate>
    <StartWhenAvailable>false</StartWhenAvailable>
    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>
    <IdleSettings>
      <StopOnIdleEnd>true</StopOnIdleEnd>
      <RestartOnIdle>false</RestartOnIdle>
    </IdleSettings>
    <AllowStartOnDemand>true</AllowStartOnDemand>
    <Enabled>true</Enabled>
    <Hidden>false</Hidden>
    <RunOnlyIfIdle>false</RunOnlyIfIdle>
    <DisallowStartOnRemoteAppSession>false</DisallowStartOnRemoteAppSession>
    <UseUnifiedSchedulingEngine>true</UseUnifiedSchedulingEngine>
    <WakeToRun>false</WakeToRun>
    <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
    <Priority>7</Priority>
  </Settings>
  <Actions Context="Author">
    <Exec>
      <Command>$fullAppExePath</Command>
      <WorkingDirectory>$fullAppWorkingDir</WorkingDirectory>
    </Exec>
  </Actions>
</Task>
"@

        # Register the task with Task Scheduler
        $taskPath = Join-Path $env:TEMP "$taskName.xml"
        $taskXml | Out-File -FilePath $taskPath -Encoding Unicode

        $registerTask = schtasks /Create /F /XML $taskPath /TN $taskName
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Task '$taskName' installed successfully."
        } else {
            Write-Host "Failed to install task '$taskName'. Exit code: $LASTEXITCODE"
            Write-Host $registerTask
        }
        Remove-Item $taskPath

        # Run the application immediately after task registration
        $process = Start-Process -FilePath $fullAppExePath -WorkingDirectory $fullAppWorkingDir -Verb RunAs -ArgumentList "--show-settings" -PassThru
        if ($process -ne $null) {
            Write-Host "Application '$fullAppExePath' started successfully."
        }else {
            Write-Host "Failed to start application '$fullAppExePath'."
        }
    }
}

function Uninstall-ServiceAndTask() {
    # Check if WinSleepWell applicaiton is running, and if so, terminate it.
    $processName = "WinSleepWell"
    $runningProcess = Get-Process -Name $processName -ErrorAction SilentlyContinue
    if ($runningProcess) {
        Write-Host "Process '$processName' is running. Attempting to stop it."
        $runningProcess | ForEach-Object {
            try {
                $_.Kill()
                Write-Host "Process '$processName' stopped successfully."
            } catch {
                Write-Host "Failed to stop process '$processName'."
                Write-Host $_.Exception.Message
            }
        }
    } else {
        Write-Host "Process '$processName' is not running."
    }

    # Uninstall the service if it exists
    $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if ($service) {
        if ($service.Status -eq 'Running') {
            $stopService = sc.exe stop $serviceName
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Service '$serviceName' stopped successfully."
            } else {
                Write-Host "Failed to stop service '$serviceName'. Exit code: $LASTEXITCODE"
                Write-Host $stopService
            }
        }
        $deleteService = sc.exe delete $serviceName
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Service '$serviceName' uninstalled successfully."
        } else {
            Write-Host "Failed to delete service '$serviceName'. Exit code: $LASTEXITCODE"
            Write-Host $deleteService
        }
    } else {
        Write-Host "Service '$serviceName' is not installed."
    }

    # Uninstall the TaskScheduler task if it exists
    if (schtasks /Query /TN $taskName /FO LIST /V 2>$null) {
        $deleteTask = schtasks /Delete /TN $taskName /F
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Task '$taskName' uninstalled successfully."
        } else {
            Write-Host "Failed to delete task '$taskName'. Exit code: $LASTEXITCODE"
            Write-Host $deleteTask
        }
    } else {
        Write-Host "Task '$taskName' is not installed."
    }
}

if ($install) {
    Install-ServiceAndTask
} elseif ($uninstall) {
    Uninstall-ServiceAndTask
} else {
    $Mode = Read-Host "Would you like to install or uninstall the service and task? (Type 'i' for install or 'u' for uninstall)"
    if ($Mode -eq "i") {
        Install-ServiceAndTask
    } elseif ($Mode -eq "u") {
        Uninstall-ServiceAndTask
    } else {
        Write-Host "Invalid selection. Please type 'i' for install or 'u' for uninstall."
    }
}
