# Change directory to the script's location (project root)
Set-Location -Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

# Define paths
$srcPath = "src"
$binPath = "bin"

# If the bin/Release directory exists, delete its subdirectories but keep the root files.
if (Test-Path "$binPath/Release") {
    Get-ChildItem "$binPath/Release" -Recurse -Directory | Remove-Item -Recurse -Force
}

# Create Release output directories for App and Service within the bin/Release folder.
$binReleaseAppPath = "$binPath/Release/App"
$binReleaseServicePath = "$binPath/Release/Service"

New-Item -ItemType Directory -Path $binReleaseAppPath
New-Item -ItemType Directory -Path $binReleaseServicePath

# Build the class library, WPF application, and service in Debug configuration.
Write-Host "Building Debug configuration for class library, WPF application, and service..."
dotnet build "$srcPath/WinSleepWellLib" -c Debug
dotnet build "$srcPath/WinSleepWell" -c Debug
dotnet build "$srcPath/WinSleepWellService" -c Debug

# Build the class library, WPF application, and service in Release configuration.
Write-Host "Building Release configuration for class library, WPF application, and service..."
dotnet build "$srcPath/WinSleepWellLib" -c Release
dotnet build "$srcPath/WinSleepWell" -c Release -o $binReleaseAppPath
dotnet build "$srcPath/WinSleepWellService" -c Release

# Build and publish the Windows service in Release configuration to the Service folder, using framework-dependent deployment
Write-Host "Publishing WinSleepWellService in Release configuration..."
dotnet publish "$srcPath/WinSleepWellService" -c Release --output $binReleaseServicePath --no-self-contained

# Display a completion message.
Write-Host "Build and publish completed successfully."
