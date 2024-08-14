using System.Runtime.InteropServices;
using System.Windows;

namespace WinSleepWell
{
    public static class SystemThemeHelper
    {
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static void SetDarkMode(Window targetWindow)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(targetWindow).Handle;
            int darkMode = 1;  // 1 to enable dark mode
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));

            //ApplyTheme(Theme.Dark);
        }

        public static bool IsDarkMode()
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key != null)
            {
                var value = key.GetValue("AppsUseLightTheme");
                if (value != null)
                {
                    return (int)value == 0; // 0 = Dark Mode, 1 = Light Mode
                }
            }
            return false;
        }
    }

}
