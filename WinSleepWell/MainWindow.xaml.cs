using System;
using System.Windows;
using System.Management;

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
            GetMouseInfo();
        }

        private void GetMouseInfo()
        {
            MouseInfoComboBox.Items.Clear();
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PointingDevice");

                foreach (ManagementObject obj in searcher.Get())
                {
                    var deviceId = obj["DeviceID"]?.ToString() ?? "Unknown Device ID";
                    var description = obj["Description"]?.ToString() ?? "Unknown Description";
                    var status = obj["Status"]?.ToString();

                    // Convert status to [Enabled] or [Disabled]
                    var statusText = status == "OK" ? "[Enabled]" : "[Disabled]";
                    var displayText = $"{deviceId} ({description}) {statusText}";

                    MouseInfoComboBox.Items.Add(displayText);
                }

                if (MouseInfoComboBox.Items.Count > 0)
                {
                    MouseInfoComboBox.SelectedIndex = 0;
                }
            }
            catch (ManagementException ex)
            {
                MessageBox.Show("An error occurred while querying for WMI data: " + ex.Message);
            }
        }

        private void MouseInfoComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (MouseInfoComboBox.SelectedItem != null)
            {
                var selectedItem = MouseInfoComboBox.SelectedItem.ToString() ?? "not-selected";
                if (selectedItem.Contains("[Enabled]"))
                {
                    EnableButton.IsEnabled = false;
                    DisableButton.IsEnabled = true;
                }
                else if (selectedItem.Contains("[Disabled]"))
                {
                    EnableButton.IsEnabled = true;
                    DisableButton.IsEnabled = false;
                }
                else
                {
                    EnableButton.IsEnabled = false;
                    DisableButton.IsEnabled = false;
                }
            }
            else
            {
                EnableButton.IsEnabled = false;
                DisableButton.IsEnabled = false;
            }
        }

        private void EnableButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDeviceStatus(true);
        }

        private void DisableButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDeviceStatus(false);
        }

        private void ChangeDeviceStatus(bool enable)
        {
            if (MouseInfoComboBox.SelectedItem != null)
            {
                var selectedItem = MouseInfoComboBox.SelectedItem.ToString();
                var deviceId = selectedItem?.Split(' ')[0] ?? "";
                if (string.IsNullOrEmpty(deviceId))
                {
                    MessageBox.Show("Choose device.");
                    return;
                }

                // Code to enable or disable the device using WMI
                try
                {
                    using (var searcher = new ManagementObjectSearcher(
                        $"SELECT * FROM Win32_PNPEntity WHERE DeviceID='{deviceId.Replace("\\", "\\\\")}'"))
                    {
                        foreach (ManagementObject mobj in searcher.Get())
                        {

                            mobj.InvokeMethod(enable ? "Enable" : "Disable", null);
                            var deviceName = mobj["Name"].ToString() ?? "Unknown device";
                            MessageBox.Show($"{deviceName} is " + (enable ? "Enabled" : "Disabled"));
                        }
                    }

                    GetMouseInfo(); // Refresh the list
                }
                catch (ManagementException ex)
                {
                    MessageBox.Show("An error occurred while changing the device status: " + ex.Message);
                }
            }
        }
    }
}
