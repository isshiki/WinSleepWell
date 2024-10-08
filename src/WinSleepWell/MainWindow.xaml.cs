﻿using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using WinSleepWellLib;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace WinSleepWell
{
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon = null!;
        private DeviceManager _deviceManager = null!;
        private List<DeviceManager.DeviceInfo> _devices = null!;
        private SettingsManager _settingsManager = null!;
        private System.Timers.Timer _retryTimer = null!;
        private bool _isInitialized = false;
        private bool _isFirstTimeShown = true;
        private bool _isLoadingSettings = false;
        private bool _mouseAutoToggle;
        private bool _biometricAutoToggle;
        private string _selectedMouseDevice = "None";
        private string _selectedBiometricDevice = "None";

        public MainWindow()
        {
            InitializeComponent();
            ApplyTheme();
            SetWindowTitleWithVersion();
            try
            {
                _deviceManager = new DeviceManager(false);
                _settingsManager = new SettingsManager(false);
                _devices = _deviceManager.GetDevicesForDesktopOnly();
                LoadDevicesInfo();
                LoadSettings();
                InitializeNotifyIcon();
                _isInitialized = true;
                Hide();
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent($"[application] Failed to initialize MainWindow: {ex.Message}", EventLogEntryType.Error);
                MessageBox.Show("Initialization failed. Please check the logs for more details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void ApplyTheme()
        {
            Resources.MergedDictionaries.Clear();

            if (ThemeHelper.IsDarkMode())
            {
                Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
                });
            }
        }

        private void SetWindowTitleWithVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Title = $"WinSleepWell v.{version}";
        }

        private void InitializeNotifyIcon()
        {
            try
            {
                var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                _notifyIcon = new NotifyIcon
                {
                    Icon = new Icon(iconPath),
                    Text = "WinSleepWell"
                };

                _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();
                _notifyIcon.ContextMenuStrip = CreateContextMenu();

                // Set the NotifyIcon visibility to register it in the task tray.
                _notifyIcon.Visible = true;

                // Check if the NotifyIcon registration was successful
                if (_notifyIcon.Visible)
                {
                    //EventLogger.LogEvent("[application] Successfully registered NotifyIcon.", EventLogEntryType.Information);
                }
                else
                {
                    // Log the failure to the event log
                    EventLogger.LogEvent("[application] Failed to register NotifyIcon on first attempt.", EventLogEntryType.Error);

                    // Schedule a retry after 1 minute
                    _retryTimer = new System.Timers.Timer(60000); // 60,000 ms = 1 minute
                    _retryTimer.Elapsed += (sender, args) =>
                    {
                        _retryTimer.Stop(); // Stop the timer to prevent repeated execution
                        _retryTimer.Dispose();

                        // Check if the application is still running before attempting retry
                        if (Application.Current != null && !Application.Current.Dispatcher.HasShutdownStarted)
                        {
                            // Retry NotifyIcon registration
                            _notifyIcon.Visible = true;

                            // Log the result of the retry
                            if (_notifyIcon.Visible)
                            {
                                //EventLogger.LogEvent("[application] Successfully registered NotifyIcon on retry.", EventLogEntryType.Information);
                            }
                            else
                            {
                                EventLogger.LogEvent("[application] Failed to register NotifyIcon on retry.", EventLogEntryType.Error);
                                MessageBox.Show("Failed to register NotifyIcon on retry.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                Application.Current.Shutdown();
                            }
                        }
                    };
                    _retryTimer.Start(); // Start the timer for the retry
                }
            }
            catch (Exception ex)
            {
                // Log any unexpected exceptions to the event log
                EventLogger.LogEvent($"[application] Failed to initialize NotifyIcon: {ex.Message}", EventLogEntryType.Error);
            }
        }

        private ContextMenuStrip? CreateContextMenu()
        {
            bool isDarkMode = ThemeHelper.IsDarkMode();

            var contextMenu = new ContextMenuStrip
            {
                Renderer = new ContextMenuRenderer(isDarkMode)
            };

            contextMenu.Items.Add(new ToolStripMenuItem("Show", null, (s, e) => ShowMainWindow())
            {
                Padding = new Padding(0, 10, 0, 10)

            });

            contextMenu.Items.Add(new ToolStripMenuItem("Exit", null, (s, e) => ExitApplication())
            {
                Padding = new Padding(0, 10, 0, 10)
            });

            return contextMenu;
        }

        public void ShowMainWindow()
        {
            Show();
            if (_isFirstTimeShown)
            {
                _isFirstTimeShown = false;

                if (ThemeHelper.IsDarkMode())
                {
                    ThemeHelper.SetDarkMode(this);
                    Dispatcher.InvokeAsync(() =>
                    {
                        Hide();
                        Show();
                        Activate();
                    });
                }
                else
                {
                    Activate();
                }
            }
            else
            {
                Activate();
            }
        }

        public void ExitApplication()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _settingsManager.Dispose();
            _deviceManager.Dispose();
            Application.Current.Shutdown();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            Hide();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                _notifyIcon.ShowBalloonTip(1000, "WinSleepWell", "Application minimized to tray.", ToolTipIcon.Info);
            }
            base.OnStateChanged(e);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            SaveSettings();
            Hide();
        }

        private void LoadDevicesInfo()
        {
            MouseInfoComboBox.Items.Clear();
            BiometricInfoComboBox.Items.Clear();

            MouseInfoComboBox.Items.Add("None");
            BiometricInfoComboBox.Items.Add("None");

            foreach (var device in _devices)
            {
                var statusText = device.Status == "OK" ? "[Enabled]" : "[Disabled]";
                var displayText = $"{device.DeviceId} ({device.DeviceName}) {statusText}";

                if (device.PnpClass == "Mouse")
                {
                    MouseInfoComboBox.Items.Add(displayText);
                }
                else if (device.PnpClass == "Biometric" && device.DeviceName.Contains("Fingerprint"))
                {
                    BiometricInfoComboBox.Items.Add(displayText);
                }
            }
        }

        private void MouseInfoComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedMouseDevice = MouseInfoComboBox.SelectedItem?.ToString() ?? "None";
            UpdateMouseButtonStates();
            if (_isInitialized && !_isLoadingSettings)
            {
                SaveSettings();
            }
        }

        private void BiometricInfoComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedBiometricDevice = BiometricInfoComboBox.SelectedItem?.ToString() ?? "None";
            UpdateBiometricButtonStates();
            if (_isInitialized && !_isLoadingSettings)
            {
                SaveSettings();
            }
        }

        private void UpdateMouseButtonStates()
        {
            var mouseSelectedItem = MouseInfoComboBox.SelectedItem?.ToString();

            if (mouseSelectedItem != null && mouseSelectedItem != "None" && mouseSelectedItem.Contains("[Enabled]"))
            {
                EnableMouseButton.IsEnabled = false;
                DisableMouseButton.IsEnabled = true;
            }
            else if (mouseSelectedItem != null && mouseSelectedItem != "None" && mouseSelectedItem.Contains("[Disabled]"))
            {
                EnableMouseButton.IsEnabled = true;
                DisableMouseButton.IsEnabled = false;
            }
            else
            {
                EnableMouseButton.IsEnabled = false;
                DisableMouseButton.IsEnabled = false;
            }
        }

        private void UpdateBiometricButtonStates()
        {
            var biometricSelectedItem = BiometricInfoComboBox.SelectedItem?.ToString();

            if (biometricSelectedItem != null && biometricSelectedItem != "None" && biometricSelectedItem.Contains("[Enabled]"))
            {
                EnableBiometricButton.IsEnabled = false;
                DisableBiometricButton.IsEnabled = true;
            }
            else if (biometricSelectedItem != null && biometricSelectedItem != "None" && biometricSelectedItem.Contains("[Disabled]"))
            {
                EnableBiometricButton.IsEnabled = true;
                DisableBiometricButton.IsEnabled = false;
            }
            else
            {
                EnableBiometricButton.IsEnabled = false;
                DisableBiometricButton.IsEnabled = false;
            }
        }

        private void EnableMouseButton_Click(object sender, RoutedEventArgs e)
        {
            var result = ChangeDeviceStatus(true, true, true, "BUTTON CLICKED");
            UpdateMouseButtonStates();
            MessageBox.Show(result);
        }

        private void DisableMouseButton_Click(object sender, RoutedEventArgs e)
        {
            var result = ChangeDeviceStatus(false, true, true, "BUTTON CLICKED");
            UpdateMouseButtonStates();
            MessageBox.Show(result);
        }

        private void EnableBiometricButton_Click(object sender, RoutedEventArgs e)
        {
            var result = ChangeDeviceStatus(true, false, true, "BUTTON CLICKED");
            UpdateBiometricButtonStates();
            MessageBox.Show(result);
        }

        private void DisableBiometricButton_Click(object sender, RoutedEventArgs e)
        {
            var result = ChangeDeviceStatus(false, false, true, "BUTTON CLICKED");
            UpdateBiometricButtonStates();
            MessageBox.Show(result);
        }

        private string ChangeDeviceStatus(bool enable, bool isMouse, bool canUseGUI, string message)
        {
            var selectedItem = isMouse ? _selectedMouseDevice : _selectedBiometricDevice;

            if (String.IsNullOrEmpty(selectedItem) || selectedItem == "None")
            {
                return "Please select a device.";
            }

            var deviceId = selectedItem?.Split(' ')[0] ?? "";
            if (String.IsNullOrEmpty(deviceId))
            {
                return "Please select a device.";
            }

            var result = _deviceManager.ChangeDeviceStatus(deviceId, enable, canUseGUI, message);

            if (canUseGUI)
            {
                ReloadDevicesInfo();
            }

            return result;
        }

        private void ReloadDevicesInfo_Click(object sender, RoutedEventArgs e)
        {
            ReloadDevicesInfo();
        }

        private void ReloadDevicesInfo()
        {
            _isLoadingSettings = true;
            _devices = _deviceManager.GetDevicesForDesktopOnly();
            LoadDevicesInfo();
            LoadSettings(); // Load settings after refreshing the device list
            _isLoadingSettings = false;
        }

        private void AutoToggleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == MouseAutoToggleCheckBox)
            {
                _mouseAutoToggle = true;
            }
            else if (sender == BiometricAutoToggleCheckBox)
            {
                _biometricAutoToggle = true;
            }

            if (_isInitialized && !_isLoadingSettings)
            {
                SaveSettings();
            }
        }

        private void AutoToggleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender == MouseAutoToggleCheckBox)
            {
                _mouseAutoToggle = false;
            }
            else if (sender == BiometricAutoToggleCheckBox)
            {
                _biometricAutoToggle = false;
            }

            if (_isInitialized && !_isLoadingSettings)
            {
                SaveSettings();
            }
        }

        private void SaveSettings()
        {
            var settings = new Settings
            {
                MouseDeviceId = MouseInfoComboBox.SelectedItem?.ToString() ?? "None",
                BiometricDeviceId = BiometricInfoComboBox.SelectedItem?.ToString() ?? "None",
                MouseAutoToggle = MouseAutoToggleCheckBox.IsChecked ?? true,
                BiometricAutoToggle = BiometricAutoToggleCheckBox.IsChecked ?? true
            };

            _settingsManager.SaveSettings(settings);
        }

        private void LoadSettings()
        {
            _isLoadingSettings = true;
            var settings = _settingsManager.LoadSettings();

            _selectedMouseDevice = settings?.MouseDeviceId ?? "None";
            _selectedBiometricDevice = settings?.BiometricDeviceId ?? "None";
            SelectComboBoxItem(MouseInfoComboBox, _selectedMouseDevice);
            SelectComboBoxItem(BiometricInfoComboBox, _selectedBiometricDevice);

            _mouseAutoToggle = settings?.MouseAutoToggle ?? false;
            _biometricAutoToggle = settings?.BiometricAutoToggle ?? false;
            MouseAutoToggleCheckBox.IsChecked = _mouseAutoToggle;
            BiometricAutoToggleCheckBox.IsChecked = _biometricAutoToggle;
            _isLoadingSettings = false;
        }

        private void SelectComboBoxItem(System.Windows.Controls.ComboBox comboBox, string item)
        {
            if (comboBox.Items.Count > 1)
            {
                foreach (var comboBoxItem in comboBox.Items)
                {
                    if (comboBoxItem.ToString() == item)
                    {
                        comboBox.SelectedItem = comboBoxItem;
                        if (comboBox == MouseInfoComboBox)
                        {
                            _selectedMouseDevice = item;
                        }
                        else if (comboBox == BiometricInfoComboBox)
                        {
                            _selectedBiometricDevice = item;
                        }
                        return;
                    }
                }

                comboBox.SelectedIndex = 1; // Select the first item if the item is not found
                if (comboBox == MouseInfoComboBox)
                {
                    _selectedMouseDevice = comboBox.Items[1].ToString() ?? "None";
                }
                else if (comboBox == BiometricInfoComboBox)
                {
                    _selectedBiometricDevice = comboBox.Items[1].ToString() ?? "None";
                }
            }
            else
            {
                comboBox.SelectedIndex = 0; // Select "None" if the item is not found
                if (comboBox == MouseInfoComboBox)
                {
                    _selectedMouseDevice = "None";
                }
                else if (comboBox == BiometricInfoComboBox)
                {
                    _selectedBiometricDevice = "None";
                }
            }
        }

        private void RestartServiceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var sc = new ServiceController(Identifiers.ServiceName))
                {
                    var currentStatus = sc.Status;

                    if (currentStatus == ServiceControllerStatus.StartPending || currentStatus == ServiceControllerStatus.StopPending)
                    {
                        MessageBox.Show($"The service is currently in the '{currentStatus}' state. Please wait until the service is fully started or stopped before attempting a restart.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if ((currentStatus == ServiceControllerStatus.Running || currentStatus == ServiceControllerStatus.Paused) && sc.CanStop)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    }

                    if (sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running);
                    }

#if DEBUG || TEST
                    MessageBox.Show($"Status: {sc.Status}, Can Pause and Continue: {sc.CanPauseAndContinue}, Can Shutdown: {sc.CanShutdown}");
#endif
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to restart the {Identifiers.ServiceName}. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility && (Visibility)value == Visibility.Visible)
                return true;
            else
                return false;
        }
    }
}
