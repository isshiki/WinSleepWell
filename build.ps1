# Define paths
$srcPath = "src"
$binPath = "bin"

# Clean the bin directory
if (Test-Path $binPath) {
    Remove-Item -Recurse -Force $binPath
}
New-Item -ItemType Directory -Path $binPath

# Create Debug and Release directories
$debugPath = "$binPath/Debug"
$releasePath = "$binPath/Release"

New-Item -ItemType Directory -Path $debugPath
New-Item -ItemType Directory -Path $releasePath

# Build Debug and Release configurations
Write-Host "Building Debug configuration..."
dotnet build "$srcPath/WinSleepWellLib" -c Debug -o $debugPath
dotnet build "$srcPath/WinSleepWellApp" -c Debug -o $debugPath
dotnet build "$srcPath/WinSleepWellService" -c Debug -o $debugPath

Write-Host "Building Release configuration..."
dotnet build "$srcPath/WinSleepWellLib" -c Release -o $releasePath
dotnet build "$srcPath/WinSleepWellApp" -c Release -o $releasePath
dotnet build "$srcPath/WinSleepWellService" -c Release -o $releasePath

Write-Host "Build completed successfully."
