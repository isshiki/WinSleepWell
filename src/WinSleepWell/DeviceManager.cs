using System.Diagnostics;
using System.Management;

namespace WinSleepWell
{
    public class DeviceManager
    {
        public class DeviceInfo
        {
            public string DeviceId { get; set; }
            public string DeviceName { get; set; }
            public string Status { get; set; }
            public string PnpClass { get; set; }
        }

        public List<DeviceInfo> GetDevices()
        {
            var devices = new List<DeviceInfo>();
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");

                foreach (ManagementObject obj in searcher.Get())
                {
                    var deviceId = obj["DeviceID"]?.ToString() ?? "Unknown Device ID";
                    var deviceName = obj["Name"]?.ToString() ?? "Unknown Device Name";
                    var status = obj["Status"]?.ToString() ?? "Unknown Status";
                    var pnpClass = obj["PNPClass"]?.ToString() ?? "Unknown PNPClass";

                    devices.Add(new DeviceInfo
                    {
                        DeviceId = deviceId,
                        DeviceName = deviceName,
                        Status = status,
                        PnpClass = pnpClass
                    });
                }
            }
            catch (ManagementException ex)
            {
                EventLogger.LogEvent("An error occurred while querying for WMI data: " + ex.Message, EventLogEntryType.Error);
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent("Something went wrong: " + ex.Message, EventLogEntryType.Error);
            }

            return devices;
        }

        public string ChangeDeviceStatus(string deviceId, bool enable, bool canUseGUI, string message)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_PnPEntity WHERE DeviceID='{deviceId.Replace("\\", "\\\\")}'"))
                {
                    foreach (ManagementObject mobj in searcher.Get())
                    {
                        object[] methodArgs = { string.Empty };
                        mobj.InvokeMethod(enable ? "Enable" : "Disable", methodArgs);
                        var prefix = canUseGUI ? $"[{message}] " : (enable ? $"[RESUME{message}] " : $"[SUSPEND{message}] ");
                        var deviceName = mobj["Name"].ToString() ?? "Unknown device";
                        var resultMessage = $"{deviceName} is " + (enable ? "Enabled." : "Disabled.");
                        EventLogger.LogEvent(prefix + resultMessage, EventLogEntryType.Information);
                        return resultMessage;
                    }
                }
            }
            catch (ManagementException ex)
            {
                var errorMessage = "An error occurred while changing the device status: " + ex.Message;
                EventLogger.LogEvent(errorMessage, EventLogEntryType.Error);
                return errorMessage;
            }
            catch (Exception ex)
            {
                var errorMessage = "Something went wrong: " + ex.Message;
                EventLogger.LogEvent(errorMessage, EventLogEntryType.Error);
                return errorMessage;
            }

            return "Device not found.";
        }
    }
}
