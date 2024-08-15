using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace WinSleepWell
{
    public static class ThemeHelper
    {
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static bool SetDarkMode(Window targetWindow)
        {
            try
            {
                var interopHelper = new System.Windows.Interop.WindowInteropHelper(targetWindow);
                interopHelper.EnsureHandle();  // Ensure that the window handle is created
                IntPtr hwnd = interopHelper.Handle;
                int darkMode = 1;  // 1 to enable dark mode
                int result = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));


                // Check if the function call was successful
                if (result != 0)  // 0x00000000 (=S_OK: Operation succeeded)
                {
                    // Retrieve and handle the last Win32 error
                    int errorCode = Marshal.GetLastWin32Error();
                    EventLogger.LogEvent($"[SetDarkMode] Failed to set dark mode. Win32 Error Code: {errorCode}", EventLogEntryType.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions
                EventLogger.LogEvent($"[SetDarkMode] An error occurred while setting dark mode: {ex.Message}", EventLogEntryType.Error);
                return false;
            }

            return true;
        }

        public static bool IsDarkMode()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key == null)
                    {
                        // Handle case where registry key is not found
                        EventLogger.LogEvent("[IsDarkMode] Registry key not found. Returning default value: Light Mode.", EventLogEntryType.Error);
                        return false; // Default to Light Mode
                    }

                    var value = key.GetValue("AppsUseLightTheme");
                    if (value == null)
                    {
                        // Handle case where registry value is not found
                        EventLogger.LogEvent("[IsDarkMode] Registry value not found. Returning default value: Light Mode.", EventLogEntryType.Error);
                        return false; // Default to Light Mode
                    }

                    return (int)value == 0; // 0 = Dark Mode, 1 = Light Mode
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions
                EventLogger.LogEvent($"[IsDarkMode] An error occurred while reading Registry value: {ex.Message}. Returning default value: Light Mode.", EventLogEntryType.Error);
                return false; // Default to Light Mode in case of an error
            }
        }
    }

}
