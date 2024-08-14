using System.Runtime.InteropServices;
using System.Windows;

namespace WinSleepWell
{
    public static class SystemThemeHelper
    {
        public enum Theme
        {
            Light,
            Dark,
            System
        }

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

        public static void SetLightMode(Window targetWindow)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(targetWindow).Handle;
            int lightMode = 0;  // 0 to enable llight mode
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref lightMode, sizeof(int));

            //ApplyTheme(Theme.System);
        }

        public static void ApplyTheme(Theme theme)
        {
            // Reset Resource Dictionary
            Application.Current.Resources.MergedDictionaries.Clear();

            // Applying Themes
            switch (theme)
            {
                case Theme.Light:
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative)
                    });
                    break;
                case Theme.Dark:
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("/DarkTheme.xaml", UriKind.Relative)
                    });
                    break;
                case Theme.System:
                    ApplySystemTheme();
                    break;
            }
        }

        private static void ApplySystemTheme()
        {
            var isDarkMode = IsDarkMode();
            if (isDarkMode)
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
                });
            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative)
                });
            }
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
