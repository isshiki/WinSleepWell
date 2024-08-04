using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using System.Windows.Threading;
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
                if (Environment.UserInteractive)
                {
                    MessageBox.Show("This application must be run as an administrator.", "Insufficient Privileges", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    EventLogger.LogEvent("[Insufficient Privileges] This application must be run as an administrator.", EventLogEntryType.Error);
                }
                Application.Current.Shutdown();
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

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

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            string logMessage = $"Unhandled Exception in AppDoma: {ex?.Message ?? ""}\nStack Trace: {ex?.StackTrace ?? ""}";
            EventLogger.LogEvent(logMessage, EventLogEntryType.Error);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            string logMessage = $"Unhandled Dispatcher Exception:  {ex.Message}\nStack Trace: {ex.StackTrace}";
            EventLogger.LogEvent(logMessage, EventLogEntryType.Error);
            e.Handled = true;
        }
    }
}
