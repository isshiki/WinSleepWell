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

            if (PrivilegeManager.EnsureAdminPrivileges(false, "application") == false)
            {
                Application.Current.Shutdown();
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            mainWindow = new MainWindow();

            Application.Current.SessionEnding += new SessionEndingCancelEventHandler(Current_SessionEnding);
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            if (e.Args.Contains("--show-settings"))
            {
                mainWindow.ShowMainWindow();
            }
        }

        private void Current_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            // Occurs when the user ends the Windows session by logging off or shutting down the operating system.
            mainWindow?.ExitApplication();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            string logMessage = $"[application] Unhandled Exception in AppDomain: {ex?.Message ?? "No Exception Message"}\nStack Trace: {ex?.StackTrace ?? "No Stack Trace"}";
            EventLogger.LogEvent(logMessage, EventLogEntryType.Error);

            // Optional: Show a user-friendly message box
            if (Environment.UserInteractive)
            {
                MessageBox.Show("An unexpected error occurred. The application will close.", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Optional: Force application to close
            mainWindow?.ExitApplication();
            //Environment.Exit(1);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            string logMessage = $"[application] Unhandled Dispatcher Exception: {ex.Message}\nStack Trace: {ex.StackTrace}";
            EventLogger.LogEvent(logMessage, EventLogEntryType.Error);

            // Optional: Show a user-friendly message box
            if (Environment.UserInteractive)
            {
                MessageBox.Show("An error occurred in the application. Please save your work and restart the application.", "Application Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Prevent the application from crashing
            e.Handled = true;
        }
    }
}
