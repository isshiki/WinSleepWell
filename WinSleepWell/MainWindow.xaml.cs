using Microsoft.Win32;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace WinSleepWell
{
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;
        private DeviceManager _deviceManager;
        private List<DeviceManager.DeviceInfo> _devices;
        private SettingsManager _settingsManager;
        private bool _isInitialized = false;
        private bool _isLoading = false;

        public MainWindow()
        {
            InitializeComponent();
            _deviceManager = new DeviceManager();
            _settingsManager = new SettingsManager();
            _devices = _deviceManager.GetDevices();
            InitializeNotifyIcon();
            LoadDevicesInfo();
            LoadSettings();
            SystemEvents.PowerModeChanged += OnPowerChange;
            _isInitialized = true;
            Hide();
        }

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("app.ico"), // アイコンファイルをプロジェクトに追加しておく
                Visible = true,
                Text = "WinSleepWell"
            };

            _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, (s, e) => ShowMainWindow());
            contextMenu.Items.Add("Exit", null, ExitApplication);

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void ShowMainWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
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
            UpdateMouseButtonStates();
            if (_isInitialized && !_isLoading)
            {
                SaveSettings();
            }
        }

        private void BiometricInfoComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateBiometricButtonStates();
            if (_isInitialized && !_isLoading)
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
            ChangeDeviceStatus(true, true);
            UpdateMouseButtonStates();
        }

        private void DisableMouseButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDeviceStatus(false, true);
            UpdateMouseButtonStates();
        }

        private void EnableBiometricButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDeviceStatus(true, false);
            UpdateBiometricButtonStates();
        }

        private void DisableBiometricButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDeviceStatus(false, false);
            UpdateBiometricButtonStates();
        }

        private void ChangeDeviceStatus(bool enable, bool isMouse)
        {
            var selectedItem = isMouse ? MouseInfoComboBox.SelectedItem?.ToString() : BiometricInfoComboBox.SelectedItem?.ToString();

            if (selectedItem == "None" || string.IsNullOrEmpty(selectedItem))
            {
                MessageBox.Show("Please select a device.");
                return;
            }

            var deviceId = selectedItem?.Split(' ')[0] ?? "";
            if (string.IsNullOrEmpty(deviceId))
            {
                MessageBox.Show("Please select a device.");
                return;
            }

            var result = _deviceManager.ChangeDeviceStatus(deviceId, enable);
            MessageBox.Show(result);

            ReloadDevicesInfo();
        }

        private void ReloadDevicesInfo_Click(object sender, RoutedEventArgs e)
        {
            ReloadDevicesInfo();
        }

        private void ReloadDevicesInfo()
        {
            _isLoading = true;
            _devices = _deviceManager.GetDevices();
            LoadDevicesInfo();
            LoadSettings(); // Load settings after refreshing the device list
            _isLoading = false;
        }

        private void AutoToggleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitialized && !_isLoading)
            {
                SaveSettings();
            }
        }

        private void AutoToggleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isInitialized && !_isLoading)
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
            _isLoading = true;
            var settings = _settingsManager.LoadSettings();

            SelectComboBoxItem(MouseInfoComboBox, settings?.MouseDeviceId ?? String.Empty);
            SelectComboBoxItem(BiometricInfoComboBox, settings?.BiometricDeviceId ?? String.Empty);
            MouseAutoToggleCheckBox.IsChecked = settings?.MouseAutoToggle ?? true;
            BiometricAutoToggleCheckBox.IsChecked = settings?.BiometricAutoToggle ?? true;
            _isLoading = false;
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
                        return;
                    }
                }

                comboBox.SelectedIndex = 1; // Select the first item if the item is not found
            }
            else
            {
                comboBox.SelectedIndex = 0; // Select "None" if the item is not found
            }
        }

        private void OnPowerChange(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                if (MouseAutoToggleCheckBox.IsChecked == true)
                {
                    ChangeDeviceStatus(false, true);
                }

                if (BiometricAutoToggleCheckBox.IsChecked == true)
                {
                    ChangeDeviceStatus(false, false);
                }
            }
            else if (e.Mode == PowerModes.Resume)
            {
                if (MouseAutoToggleCheckBox.IsChecked == true)
                {
                    ChangeDeviceStatus(true, true);
                }

                if (BiometricAutoToggleCheckBox.IsChecked == true)
                {
                    ChangeDeviceStatus(true, false);
                }
            }
        }
    }
}
