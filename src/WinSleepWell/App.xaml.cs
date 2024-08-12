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
        public MainWindow? mainWindow;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (PrivilegeManager.EnsureAdminPrivileges(false, "WinSleepWell application") == false)
            {
                Application.Current.Shutdown();
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            mainWindow = new MainWindow();

            if (e.Args.Contains("--show-settings"))
            {
                mainWindow.ShowMainWindow();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            string logMessage = $"Unhandled Exception in AppDomain: {ex?.Message ?? "No Exception Message"}\nStack Trace: {ex?.StackTrace ?? "No Stack Trace"}";
            EventLogger.LogEvent(logMessage, EventLogEntryType.Error);

            // Optional: Show a user-friendly message box
            MessageBox.Show("An unexpected error occurred. The application will close.", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Optional: Force application to close
            Environment.Exit(1);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            string logMessage = $"Unhandled Dispatcher Exception: {ex.Message}\nStack Trace: {ex.StackTrace}";
            EventLogger.LogEvent(logMessage, EventLogEntryType.Error);

            // Optional: Show a user-friendly message box
            MessageBox.Show("An error occurred in the application. Please save your work and restart the application.", "Application Error", MessageBoxButton.OK, MessageBoxImage.Warning);

            // Prevent the application from crashing
            e.Handled = true;
        }
    }
}
