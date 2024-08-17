using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using WinSleepWell;

namespace WinSleepWellService
{
    public sealed class WindowsBackgroundService : BackgroundService
    {
        private DeviceManager _deviceManager = null!;
        private List<DeviceManager.DeviceInfo> _devices = null!;
        private PowerMonitor _powerMonitor = null!;
        private LidMonitor _lidMonitor = null!;
        private SettingsManager _settingsManager = null!;
        private string _selectedMouseDeviceID = "None";
        private string _selectedBiometricDeviceID = "None";
        private bool _mouseAutoToggle = false;
        private bool _biometricAutoToggle = false;

        public WindowsBackgroundService()
        {
            try
            {
                _deviceManager = new DeviceManager(true);
                _settingsManager = new SettingsManager(true);
                _powerMonitor = new PowerMonitor(true);
                _lidMonitor = new LidMonitor(true);
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent("[Service] Failed to initialize WindowsBackgroundService: " + ex.Message, EventLogEntryType.Error);
                Environment.Exit(1);
            }
        }
        public override void Dispose()
        {
            base.Dispose();
            _lidMonitor.Dispose();
            _powerMonitor.Dispose();
            _settingsManager.Dispose();
            _deviceManager.Dispose();
        }

        private string ExtractDeviceID(string deviceString)
        {
            if (String.IsNullOrEmpty(deviceString) || deviceString == "None")
            {
                return "None";
            }

            // Find the index of " ("
            int index = deviceString.IndexOf(" (");

            // If " (" is found, return the substring before it; otherwise, return "Invalid"
            if (index >= 0)
            {
                return deviceString.Substring(0, index);
            }

            return "Invalid";
        }

        private void LoadSettings()
        {
            var settings = _settingsManager.LoadSettings();

            var selectedMouseDevice = settings?.MouseDeviceId ?? "None";
            _selectedMouseDeviceID = ExtractDeviceID(selectedMouseDevice);

            var selectedBiometricDevice = settings?.BiometricDeviceId ?? "None";
            _selectedBiometricDeviceID = ExtractDeviceID(selectedBiometricDevice);

            _mouseAutoToggle = settings?.MouseAutoToggle ?? false;
            _biometricAutoToggle = settings?.BiometricAutoToggle ?? false;
        }

        private string ChangeDeviceStatus(bool enable, bool isMouse, bool canUseGUI, string message)
        {
            var selectedDeviceID = isMouse ? _selectedMouseDeviceID : _selectedBiometricDeviceID;
            if (String.IsNullOrEmpty(selectedDeviceID) || selectedDeviceID == "None")
            {
                return "No device selected.";
            }
            if (selectedDeviceID == "Invalid")
            {
                return "Invalid device ID.";
            }

            var result = _deviceManager.ChangeDeviceStatus(selectedDeviceID, enable, canUseGUI, message);
#if DEBUG
            EventLogger.LogEvent($"[Service] Device status changed: {selectedDeviceID}, Enabled: {enable}", EventLogEntryType.Information);
#endif
            return result;
        }


        private void OnSuspend(object? sender, PowerEventArgs e)
        {
#if DEBUG || TEST
            EventLogger.LogEvent($"[Service] System is suspending: {DateTimeOffset.Now}", EventLogEntryType.Information);
#endif
            LoadSettings();

            if (_mouseAutoToggle)
            {
                ChangeDeviceStatus(false, true, false, " EVENT");
            }

            if (_biometricAutoToggle)
            {
                ChangeDeviceStatus(false, false, false, " EVENT");
            }
        }

        private void OnResume(object? sender, PowerEventArgs e)
        {
#if DEBUG || TEST
            EventLogger.LogEvent($"[Service] System has resumed: {DateTimeOffset.Now}", EventLogEntryType.Information);
#endif
            LoadSettings();

            if (_mouseAutoToggle)
            {
                ChangeDeviceStatus(true, true, false, " EVENT");
            }

            if (_biometricAutoToggle)
            {
                ChangeDeviceStatus(true, false, false, " EVENT");
            }
        }
        private static void OnLidStateChanged(object? sender, LidEventArgs e)
        {
            if (e.IsLidClosed)
            {
                EventLogger.LogEvent($"[Service] Lid is closed at {DateTimeOffset.Now}", EventLogEntryType.Information);
            }
            else
            {
                EventLogger.LogEvent($"[Service] Lid is open at {DateTimeOffset.Now}", EventLogEntryType.Information);
            }
        }

        // Triggered when the application host is ready to start the service.
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
#if DEBUG || TEST
            EventLogger.LogEvent($"[Service] WinSleepWell Service is starting at: {DateTimeOffset.Now}", EventLogEntryType.Information);
#endif
            // Load settings and get all device information
            LoadSettings();
            if (_mouseAutoToggle || _biometricAutoToggle)
            {
                _devices = _deviceManager.GetDevices();
            }

            if (_mouseAutoToggle)
            {
                // Find the device with the specified DeviceId (_selectedMouseDevice)
                var mouseDevice = _devices.FirstOrDefault(device => device.DeviceId == _selectedMouseDeviceID);

                if (mouseDevice != null && mouseDevice.Status != "OK")
                {
                    // The device is found and is not enabled, so we enable it
                    ChangeDeviceStatus(true, true, false, " at Service startup");
                }
            }

            if (_biometricAutoToggle)
            {
                // Find the device with the specified DeviceId (_selectedBiometricDevice)
                var biometricDevice = _devices.FirstOrDefault(device => device.DeviceId == _selectedBiometricDeviceID);

                if (biometricDevice != null && biometricDevice.Status != "OK")
                {
                    // The device is found and is not enabled, so we enable it
                    ChangeDeviceStatus(true, false, false, " at Service startup");
                }
            }

            _lidMonitor.LidStateChanged += OnLidStateChanged;

            _powerMonitor.Suspend += OnSuspend;
            _powerMonitor.Resume += OnResume;

            await base.StartAsync(cancellationToken);  // Calling StartAsync in the base class to start the service.
        }

        // Triggered when the application host is performing a graceful shutdown.
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
#if DEBUG || TEST
            EventLogger.LogEvent($"[Service] WinSleepWell Service is stopping at: {DateTimeOffset.Now}", EventLogEntryType.Information);
#endif
            _powerMonitor.Suspend -= OnSuspend;
            _powerMonitor.Resume -= OnResume;

            await base.StopAsync(cancellationToken);  // Calling StopAsync in the base class to stop the service.
        }

        // This method is called when the IHostedService starts.
        // The implementation should return a task that represents the lifetime of the long running operation(s) being performed.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
#if DEBUG || TEST
            EventLogger.LogEvent($"[Service] WinSleepWell Service is executing at: {DateTimeOffset.Now}", EventLogEntryType.Information);
#endif
            // Wait indefinitely until the task is canceled.
            await Task.CompletedTask;
            // Since the service is still running,
            // it becomes dependent on other code (if any) that is executed after ExecuteAsync completes and on other lifecycle methods of the service.
        }
    }
}
