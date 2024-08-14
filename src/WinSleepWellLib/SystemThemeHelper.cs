namespace WinSleepWell
{
    public static class SystemThemeHelper
    {
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
