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
        sc.exe create $serviceName binPath= "$fullSvcExePath" DisplayName= "WinSleepWell Service" start= auto obj= LocalSystem
        Write-Host "Service '$serviceName' installed successfully."

        # Add description to the service
        sc.exe description $serviceName "$serviceDescription"
        Write-Host "Service description added."

        # Start the service automatically
        sc.exe start $serviceName
        Write-Host "Service '$serviceName' started successfully."
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

        schtasks /Create /F /XML $taskPath /TN $taskName
        Remove-Item $taskPath
        Write-Host "Task '$taskName' installed successfully."

        # Run the application immediately after task registration
        Start-Process -FilePath $fullAppExePath -WorkingDirectory $fullAppWorkingDir -Verb RunAs -ArgumentList "--show-settings"
        Write-Host "Application '$fullAppExePath' started successfully."
    }
}

function Uninstall-ServiceAndTask() {
    # Uninstall the service if it exists
    if (Get-Service -Name $serviceName -ErrorAction SilentlyContinue) {
        sc.exe stop $serviceName
        sc.exe delete $serviceName
        Write-Host "Service '$serviceName' uninstalled successfully."
    } else {
        Write-Host "Service '$serviceName' is not installed."
    }

    # Uninstall the TaskScheduler task if it exists
    if (schtasks /Query /TN $taskName /FO LIST /V 2>$null) {
        schtasks /Delete /TN $taskName /F
        Write-Host "Task '$taskName' uninstalled successfully."
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
