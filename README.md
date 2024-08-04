
# WinSleepWell
This project includes a service that detects when Windows wakes from sleep and puts it back to sleep, and a taskbar-resident application that minimizes wake-ups from sleep caused by mouse movement as much as possible.

## Folder Structure

```
/MyProject
„ 
„¥„Ÿ„Ÿ /src
„    „¥„Ÿ„Ÿ /WinSleepWell          # Project for the WPF tray application
„    „¥„Ÿ„Ÿ /WinSleepWellService   # Project for the Windows service (to be added in the future)
„    „¥„Ÿ„Ÿ /WinSleepWellLib       # Project for the shared library (to be added in the future)
„    „¥„Ÿ„Ÿ /WinSleepWellSetup     # Project for the setup (to be added in the future)
„    „¤„Ÿ„Ÿ WinSleepWell.sln       # Solution file
„ 
„¥„Ÿ„Ÿ /bin
„    „¤„Ÿ„Ÿ /Release               # Released program contents
„ 
„¥„Ÿ„Ÿ /images                     # Images used in the project
„ 
„¥„Ÿ„Ÿ .gitignore
„¥„Ÿ„Ÿ build.ps1                   # Build script
„¥„Ÿ„Ÿ README.md                   # English README file
„¥„Ÿ„Ÿ README-ja.md                # Japanese README file
„¥„Ÿ„Ÿ LICENSE                     # License file
„¤„Ÿ„Ÿ CONTRIBUTING.md             # Contribution guidelines
```

## How to develop
Run Visual Studio with administrator privileges to debug. This program requires administrator privileges.

## How to build
To build the project, you can use the `build.ps1` script. This script will compile all projects and place the artifacts in the appropriate `bin` directories.

### Using the build script

1. Open a PowerShell window with administrator privileges.
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