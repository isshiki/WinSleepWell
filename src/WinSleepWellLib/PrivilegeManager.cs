using System;
using System.Diagnostics;
using System.Security.Principal;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace WinSleepWell
{
    public static class PrivilegeManager
    {
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool EnsureAdminPrivileges(bool isService, string prgramName)
        {
            if (IsAdministrator())
            {
                return true;
            }

            string message = $"This {prgramName} must be run as an administrator.";
            EventLogger.LogEvent($"[Insufficient Privileges] {message}", EventLogEntryType.Error);
            if ((isService == false) && (Environment.UserInteractive))
            {
                MessageBox.Show(message, "Insufficient Privileges", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }
    }
}