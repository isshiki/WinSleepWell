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
        public static bool IsAdministrator(string programName)
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                if (identity == null)
                {
                    EventLogger.LogEvent($"[{programName}] Failed to get the current Windows identity.", EventLogEntryType.Error);
                    return false;
                }

                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent($"[{programName}] An error occurred while checking for administrator privileges: {ex.Message}", EventLogEntryType.Error);
                return false;
            }
        }

        public static bool EnsureAdminPrivileges(bool isService, string programName)
        {
            if (IsAdministrator(programName))
            {
                return true;
            }

            string message = $"This WinSleepWell {programName} must be run as an administrator.";
            EventLogger.LogEvent($"[Insufficient Privileges] {message}", EventLogEntryType.Error);
            if ((isService == false) && (Environment.UserInteractive))
            {
                MessageBox.Show(message, "Insufficient Privileges", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }
    }
}