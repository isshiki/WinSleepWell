# WinSleepWell - User Guide

Thank you for using WinSleepWell. This project includes a service that detects when Windows wakes from sleep and puts it back to sleep, and a taskbar-resident application that minimizes wake-ups from sleep caused by mouse movement as much as possible.

## Setup Instructions

1. **Download**:
    - Download the `WinSleepWell` ZIP file from the [releases](https://github.com/isshiki/WinSleepWell/releases) page.
    - **Check Security Permissions**:
        - Right-click the downloaded ZIP file and select "Properties."
        - In the Properties window, go to the "General" tab and check for a "Security" section at the bottom.
        - If there is an option to "Unblock," check the box next to it and click "OK."

2. **Place**:
    - Extract the ZIP file and place the `WinSleepWell` folder in any directory of your choice.

3. **Configure PowerShell Execution Policy** (if needed):
    - Open **PowerShell** with administrator privileges.
    - Check and save the current execution policy by running:
      ```powershell
      $OriginalPolicy = Get-ExecutionPolicy
      Write-Host "Current Execution Policy: $OriginalPolicy"
      ```
    - If the policy is set to anything other than `Unrestricted`, change it by running:
      ```powershell
      Set-ExecutionPolicy Unrestricted -Scope CurrentUser
      ```
      **Note:** Setting the execution policy to `Unrestricted` allows all scripts to run, but you may receive warnings when running scripts downloaded from the internet. Changing the execution policy requires administrator privileges. The author of this project assumes no liability for any issues that may arise from modifying the execution policy.

4. **Run Setup**:
    - In the same PowerShell window, navigate to the `WinSleepWell` folder.
    - Run `setup.ps1` by typing:
      ```powershell
      .\setup.ps1 -i
      ```

5. **Uninstall**:
    - To uninstall the service and task, run:
      ```powershell
      .\setup.ps1 -u
      ```

6. **Revert Execution Policy (if changed)**:
    - After completing the setup or uninstallation, if you changed the execution policy earlier, it's recommended to revert it to the original setting by running:
      ```powershell
      Set-ExecutionPolicy $OriginalPolicy -Scope CurrentUser
      ```

## Security Notice
Modifying the PowerShell execution policy can affect the security of your system. Ensure that you understand the implications of changing the execution policy, and revert it back to its original setting after completing the setup. The author of this project assumes no liability for any issues or security risks that may arise from changing the execution policy.

## How to use  
After the setup is complete, the settings window will automatically appear. If it does not, double-click the icon that resides in the taskbar. Once the settings are completed, the service will ensure that Windows continues to sleep properly based on those settings.

**Note:** This application and service are specifically designed for the [ONEXPLAYER X1 AMD Edition](https://onexplayerstore.com/collections/x1/products/onexplayer-x1-amd-ryzen%E2%84%A2-7-8840u-10-95-3-in-1-gaming-handheld-pre-sale). While it may work on other devices, proper functionality is not guaranteed. Please be aware that we accept no liability for any damage or accidents that may occur from using this application.

## Additional Information

- **Issues**: [https://github.com/isshiki/WinSleepWell/issues](https://github.com/isshiki/WinSleepWell/issues)
- **Source Code**: [https://github.com/isshiki/WinSleepWell](https://github.com/isshiki/WinSleepWell)

If you encounter any bugs or have feature requests, you can report them through the Issues link above. However, please note that support for special use cases outside of the primary functionality is not planned. This application was created for personal use and is shared for those who might find it useful. Thank you for your understanding.
