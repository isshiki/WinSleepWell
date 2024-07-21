using System;
using System.Security.Principal;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace WinSleepWell
{
    public partial class App : Application
    {
        public MainWindow mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!IsAdministrator())
            {
                MessageBox.Show("This application must be run as an administrator.", "Insufficient Privileges", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            mainWindow = new MainWindow();
        }

        private bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
