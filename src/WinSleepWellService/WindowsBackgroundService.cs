using System.Diagnostics;
using System.Windows;
using WinSleepWell;

namespace WinSleepWellService
{
    public sealed class WindowsBackgroundService : BackgroundService
    {
        private DeviceManager _deviceManager = null!;
        private PowerMonitor _powerMonitor = null!;
        private SettingsManager _settingsManager = null!;
        private string _selectedMouseDevice = "None";
        private string _selectedBiometricDevice = "None";
        private bool _mouseAutoToggle = false;
        private bool _biometricAutoToggle = false;

        public WindowsBackgroundService()
        {
            try
            {
                _deviceManager = new DeviceManager();
                _settingsManager = new SettingsManager(false);
                _powerMonitor = new PowerMonitor();
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent("Failed to initialize WindowsBackgroundService: " + ex.Message, EventLogEntryType.Error);
                Environment.Exit(1);
            }
        }
        public override void Dispose()
        {
            base.Dispose();
            _powerMonitor.Dispose();
            _settingsManager.Dispose();
            _deviceManager.Dispose();
        }

        private void LoadSettings()
        {
            var settings = _settingsManager.LoadSettings();

            _selectedMouseDevice = settings?.MouseDeviceId ?? "None";
            _selectedBiometricDevice = settings?.BiometricDeviceId ?? "None";

            _mouseAutoToggle = settings?.MouseAutoToggle ?? false;
            _biometricAutoToggle = settings?.BiometricAutoToggle ?? false;
        }

        private string ChangeDeviceStatus(bool enable, bool isMouse, bool canUseGUI, string message)
        {
            var selectedItem = isMouse ? _selectedMouseDevice : _selectedBiometricDevice;

            if (selectedItem == "None" || string.IsNullOrEmpty(selectedItem))
            {
                return "No device selected.";
            }

            var deviceId = selectedItem?.Split(' ')[0] ?? "";
            if (string.IsNullOrEmpty(deviceId))
            {
                return "Invalid device ID.";
            }

            var result = _deviceManager.ChangeDeviceStatus(deviceId, enable, canUseGUI, message);
#if DEBUG
            EventLogger.LogEvent($"Device status changed: {selectedItem}, Enabled: {enable}", EventLogEntryType.Information);
#endif
            return result;
        }


        private void OnSuspend(object? sender, PowerEventArgs e)
        {
#if DEBUG
            EventLogger.LogEvent("System is suspending: " + DateTimeOffset.Now, EventLogEntryType.Information);
#endif
            LoadSettings();

            if (_mouseAutoToggle)
            {
                ChangeDeviceStatus(false, true, false, e.Message);
            }

            if (_biometricAutoToggle)
            {
                ChangeDeviceStatus(false, false, false, e.Message);
            }
        }

        private void OnResume(object? sender, PowerEventArgs e)
        {
#if DEBUG
            EventLogger.LogEvent("System has resumed: " + DateTimeOffset.Now, EventLogEntryType.Information);
#endif
            LoadSettings();

            if (_mouseAutoToggle)
            {
                ChangeDeviceStatus(true, true, false, e.Message);
            }

            if (_biometricAutoToggle)
            {
                ChangeDeviceStatus(true, false, false, e.Message);
            }
        }

        // Triggered when the application host is ready to start the service.
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
#if DEBUG
            EventLogger.LogEvent("WinSleepWell Service is starting at: " + DateTimeOffset.Now, EventLogEntryType.Information);
#endif
            _powerMonitor.Suspend += OnSuspend;
            _powerMonitor.Resume += OnResume;

            await base.StartAsync(cancellationToken);  // Calling StartAsync in the base class to start the service.
        }

        // Triggered when the application host is performing a graceful shutdown.
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
#if DEBUG
            EventLogger.LogEvent("WinSleepWell Service is stopping at: " + DateTimeOffset.Now, EventLogEntryType.Information);
#endif
            _powerMonitor.Suspend -= OnSuspend;
            _powerMonitor.Resume -= OnResume;

            await base.StopAsync(cancellationToken);  // Calling StopAsync in the base class to stop the service.
        }

        // This method is called when the IHostedService starts.
        // The implementation should return a task that represents the lifetime of the long running operation(s) being performed.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
#if DEBUG
            EventLogger.LogEvent("WinSleepWell Service is executing at: " + DateTimeOffset.Now, EventLogEntryType.Information);
#endif
            // Wait indefinitely until the task is canceled.
            await Task.CompletedTask;
            // Since the service is still running,
            // it becomes dependent on other code (if any) that is executed after ExecuteAsync completes and on other lifecycle methods of the service.
        }
    }
}
