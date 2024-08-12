using System.Diagnostics;
using WinSleepWell;

namespace WinSleepWellService
{
    public sealed class WindowsBackgroundService(SleepKeeperService sleepKeeper) : BackgroundService
    {
        // Triggered when the application host is ready to start the service.
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            EventLogger.LogEvent("WinSleepWell Service is starting at: " + DateTimeOffset.Now, EventLogEntryType.Information);

            await base.StartAsync(cancellationToken);  // Calling StartAsync in the base class to start the service.
        }

        // Triggered when the application host is performing a graceful shutdown.
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            EventLogger.LogEvent("WinSleepWell Service is stopping at: " + DateTimeOffset.Now, EventLogEntryType.Information);

            await base.StopAsync(cancellationToken);  // Calling StopAsync in the base class to stop the service.
        }

        // This method is called when the IHostedService starts.
        // The implementation should return a task that represents the lifetime of the long running operation(s) being performed.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            EventLogger.LogEvent("WinSleepWell Service is executing at: " + DateTimeOffset.Now, EventLogEntryType.Information);

            // Wait indefinitely until the task is canceled.
            await Task.CompletedTask;
            // Since the service is still running,
            // it becomes dependent on other code (if any) that is executed after ExecuteAsync completes and on other lifecycle methods of the service.

            /*
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    string joke = jokeService.GetJoke();
                    logger.LogWarning("{Joke}", joke);

                    // Use EventLogger
                    //EventLogger.LogEvent("Worker2 running at: " + DateTimeOffset.Now, EventLogEntryType.Information);

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // When the stopping token is canceled, for example, a call made from services.msc,
                // we shouldn't exit with a non-zero exit code. In other words, this is expected...
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{Message}", ex.Message);

                // Terminates this process and returns an exit code to the operating system.
                // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
                // performs one of two scenarios:
                // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
                // 2. When set to "StopHost": will cleanly stop the host, and log errors.
                //
                // In order for the Windows Service Management system to leverage configured
                // recovery options, we need to terminate the process with a non-zero exit code.
                Environment.Exit(1);
            }
            */
        }
    }
}
