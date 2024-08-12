using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
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
        private PowerMonitor _powerMonitor = null!;
        private bool _isInitialized = false;
        private bool _isLoadingSettings = false;
        private bool _mouseAutoToggle;
        private bool _biometricAutoToggle;
        private string _selectedMouseDevice = "None";
        private string _selectedBiometricDevice = "None";

        public MainWindow()
        {
            InitializeComponent();
            SetWindowTitleWithVersion();
            try
            {
                _deviceManager = new DeviceManager();
                _settingsManager = new SettingsManager(false);
                _devices = _deviceManager.GetDevices();
                LoadDevicesInfo();
                LoadSettings();
                InitializeNotifyIcon();
                _powerMonitor = new PowerMonitor();
                _powerMonitor.Suspend += OnSuspend;
                _powerMonitor.Resume += OnResume;
                _isInitialized = true;
                Hide();
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent("Failed to initialize MainWindow: " + ex.Message, EventLogEntryType.Error);
                MessageBox.Show("Initialization failed. Please check the logs for more details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void SetWindowTitleWithVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Title = $"WinSleepWell v.{version}";
        }

        private void InitializeNotifyIcon()
        {
            var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(iconPath),
                Visible = true,
                Text = "WinSleepWell"
            };
            //EventLogger.LogEvent("Added notify-icon", EventLogEntryType.Information);

            _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, (s, e) => ShowMainWindow());
            contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        public void ShowMainWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void ExitApplication()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _powerMonitor.Dispose();
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

        private void OnSuspend(object? sender, PowerEventArgs e)
        {
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
            if (_mouseAutoToggle)
            {
                ChangeDeviceStatus(true, true, false, e.Message);
            }

            if (_biometricAutoToggle)
            {
                ChangeDeviceStatus(true, false, false, e.Message);
            }
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

            if (selectedItem == "None" || string.IsNullOrEmpty(selectedItem))
            {
                MessageBox.Show("Please select a device.");
                return String.Empty;
            }

            var deviceId = selectedItem?.Split(' ')[0] ?? "";
            if (string.IsNullOrEmpty(deviceId))
            {
                MessageBox.Show("Please select a device.");
                return String.Empty;
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
            _devices = _deviceManager.GetDevices();
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
    }
}
