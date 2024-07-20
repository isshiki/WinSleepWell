using System;
using System.Windows;
using System.Management;
using System.Diagnostics;

namespace WinSleepWell
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GetDevicesInfo();
        }

        private void GetDevicesInfo()
        {
            MouseInfoComboBox.Items.Clear();
            BiometricInfoComboBox.Items.Clear();

            MouseInfoComboBox.Items.Add("None");
            BiometricInfoComboBox.Items.Add("None");

            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");

                foreach (ManagementObject obj in searcher.Get())
                {
                    var deviceId = obj["DeviceID"]?.ToString() ?? "Unknown Device ID";
                    var deviceName = obj["Name"]?.ToString() ?? "Unknown Device Name";
                    //var description = obj["Description"]?.ToString() ?? "Unknown Description";
                    var status = obj["Status"]?.ToString() ?? "Unknown Status";
                    var pnpClass = obj["PNPClass"]?.ToString() ?? "Unknown PNPClass";

                    // Convert status to [Enabled] or [Disabled]
                    var statusText = status == "OK" ? "[Enabled]" : "[Disabled]";
                    var displayText = $"{deviceId} ({deviceName}) {statusText}";

                    // PNPClass for Mouse and Touchpad
                    if (pnpClass == "Mouse")
                    {
                        MouseInfoComboBox.Items.Add(displayText);
                    }
                    // PNPClass for Biometric devices
                    else if ((pnpClass == "Biometric") && (deviceName.Contains("Fingerprint")))
                    {
                        BiometricInfoComboBox.Items.Add(displayText);
                    }
                }

                if (MouseInfoComboBox.Items.Count > 1)
                {
                    MouseInfoComboBox.SelectedIndex = 1;
                }
                else
                {
                    MouseInfoComboBox.SelectedIndex = 0;
                }

                if (BiometricInfoComboBox.Items.Count > 1)
                {
                    BiometricInfoComboBox.SelectedIndex = 1;
                }
                else
                {
                    BiometricInfoComboBox.SelectedIndex = 0;
                }
            }
            catch (ManagementException ex)
            {
                MessageBox.Show("An error occurred while querying for WMI data: " + ex.Message);
            }
        }

        private void MouseInfoComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateMouseButtonStates();
        }

        private void BiometricInfoComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateBiometricButtonStates();
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
        }

        private void DisableMouseButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDeviceStatus(false, true);
        }

        private void EnableBiometricButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDeviceStatus(true, false);
        }

        private void DisableBiometricButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDeviceStatus(false, false);
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

            // Code to enable or disable the device using WMI
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_PnPEntity WHERE DeviceID='{deviceId.Replace("\\", "\\\\")}'"))
                {
                    foreach (ManagementObject mobj in searcher.Get())
                    {
                        // Enable or Disable method
                        object[] methodArgs = { String.Empty };
                        Debug.WriteLine($"Invoking method: {(enable ? "Enable" : "Disable")} on device: {deviceId}");
                        mobj.InvokeMethod(enable ? "Enable" : "Disable", methodArgs);
                        var deviceName = mobj["Name"].ToString() ?? "Unknown device";
                        MessageBox.Show($"{deviceName} is " + (enable ? "Enabled." : "Disabled."));
                    }
                }

                GetDevicesInfo(); // Refresh the list
            }
            catch (ManagementException ex)
            {
                MessageBox.Show("An error occurred while changing the device status: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something wrong: " + ex.Message);
            }
        }
    }
}
