
## WinSleepWell  
This project includes a service that detects when Windows wakes from sleep and puts it back to sleep, and a taskbar-resident application that minimizes wake-ups from sleep caused by mouse movement as much as possible.

## Folder Structure

```
/MyProject
│
├── /src
│   ├── /WinSleepWell          # Project for the Windows (WPF) tray application
│   ├── /WinSleepWellService   # Project for the Windows service
│   ├── /WinSleepWellLib       # Project for the shared library
│   └── WinSleepWell.sln       # Solution file
│
├── /bin
│   └── /Release               # Released program contents
│       ├── /App               # Windows application
│       ├── /Service           # Windows service
│       ├── settings.json      # Shared configuration file
│       ├── setup.ps1          # Setup script for WinSleepWell (Same as the script in the project root)
│       ├── UserGuide.txt      # English user guide
│       └── UserGuide-ja.txt   # Japanese user guide
│
├── /images                     # Images used in the project
│
├── .gitignore
├── build.ps1                   # Build script
├── setup.ps1                   # Setup script for installing and uninstalling the service and task
├── README.md                   # English README file
├── README-ja.md                # Japanese README file
├── LICENSE                     # License file
└── CONTRIBUTING.md             # Contribution guidelines
```

## How to setup  
This section will be added in the future.

## How to use  
After the setup is complete, the settings window will automatically appear. If it does not, double-click the icon that resides in the taskbar. Once the settings are completed, the service will ensure that Windows continues to sleep properly based on those settings.

## Prerequisites for running PowerShell scripts
Before running the `build.ps1` and `setup.ps1` scripts, ensure that your execution policy is configured correctly:

1. Open **Developer PowerShell for VS 2022** with administrator privileges. You can find this in the Start Menu under **All Programs > Visual Studio 2022 > Developer PowerShell for VS 2022**.
2. Check the current execution policy:
   ```powershell
   $OriginalPolicy = Get-ExecutionPolicy
   Write-Host "Current Execution Policy: $OriginalPolicy"
   ```
3. If the policy is not set to `Unrestricted`, set it using:
   ```powershell
   Set-ExecutionPolicy Unrestricted -Scope CurrentUser
   ```
   **Note:** Setting the execution policy to `Unrestricted` allows all scripts to run, but you may receive warnings when running scripts downloaded from the internet. Changing the execution policy requires administrator privileges. The author of this project assumes no liability for any issues that may arise from modifying the execution policy.
4. After completing the setup or build process, if you do not plan to run these scripts frequently, you can revert to the original policy using:
   ```powershell
   Set-ExecutionPolicy $OriginalPolicy -Scope CurrentUser
   ```

## How to setup for developers  
To set up the project for development, you can follow these steps:

1. Open **Developer PowerShell for VS 2022** with administrator privileges.
2. Navigate to the root directory of the project.
3. Run the build script to compile all projects:
   ```powershell
   ./build.ps1
   ```
4. After the build is complete, run the setup script to install the service and task:
   ```powershell
   ./setup.ps1 -i
   ```
5. The built artifacts will be placed in the `bin\Debug` and `bin\Release` directories, and the service will be installed and started automatically.
6. To uninstall the service and task, run:
   ```powershell
   ./setup.ps1 -u
   ```

## How to develop  
Run Visual Studio with administrator privileges to debug. This program requires administrator privileges.

## How to build  
To build the project, you can use the `build.ps1` script. This script will compile all projects and place the artifacts in the appropriate `bin` directories.

### Using the build script  

1. Open **Developer PowerShell for VS 2022** with administrator privileges.
2. Navigate to the root directory of the project.
3. Run the build script:
   ```powershell
   ./build.ps1
   ```
4. The built artifacts will be placed in the `bin\Debug` and `bin\Release` directories.

## Contributing  
Contributions are welcome! Please see the [CONTRIBUTING.md](CONTRIBUTING.md) file for guidelines on how to contribute to this project.

## License  
This project is licensed under the terms of the Apache 2.0 license. See the [LICENSE](LICENSE) file for details.
