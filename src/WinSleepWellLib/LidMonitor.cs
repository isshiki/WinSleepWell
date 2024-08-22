/*
using Microsoft.Win32.TaskScheduler;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.ServiceProcess;
*/
using System;
using System.Diagnostics;
using System.Management;


namespace WinSleepWell
{
    /*
    public class LidEventArgs : EventArgs
    {
        public bool IsLidClosed { get; }

        public LidEventArgs(bool isLidClosed)
        {
            IsLidClosed = isLidClosed;
        }
    }
    */

    public class LidMonitor : IDisposable
    {
        private bool _isService;
        private string _programName;

        public LidMonitor(bool isService)
        {
            _isService = isService;
            _programName = _isService ? "Service" : "application";
        }

        public void Dispose()
        {
            // Do nothing
        }

        public enum VIDEO_OUTPUT_TECHNOLOGY : uint
        {
            UNINITIALIZED = 0xFFFFFFFE, // -2 in unsigned int
            OTHER = 0xFFFFFFFF,        // -1 in unsigned int
            HD15 = 0,
            SVIDEO = 1,
            COMPOSITE_VIDEO = 2,
            COMPONENT_VIDEO = 3,
            DVI = 4,
            HDMI = 5,
            LVDS = 6,
            D_JPN = 8,
            SDI = 9,
            DISPLAYPORT_EXTERNAL = 10,
            DISPLAYPORT_EMBEDDED = 11,
            UDI_EXTERNAL = 12,
            UDI_EMBEDDED = 13,
            SDTVDONGLE = 14,
            MIRACAST = 15,
            INDIRECT_WIRED = 16,
            INTERNAL = 0x80000000,
            //SVIDEO_4PIN = SVIDEO,
            //SVIDEO_7PIN = SVIDEO,
            //RF = COMPOSITE_VIDEO,
            //RCA_3COMPONENT = COMPOSITE_VIDEO,
            //BNC = COMPOSITE_VIDEO
        }   // https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/d3dkmdt/ne-d3dkmdt-_d3dkmdt_video_output_technology

        public bool IsLidOpen()
        {
            try
            {
                var foundActiveMonitor = false;

                var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM WmiMonitorConnectionParams");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string instanceName = "";
                    string videoOutputTech = "";
                    bool isActive = true;

                    foreach (var prop in obj.Properties)
                    {
                        switch (prop.Name)
                        {
                            case "InstanceName":
                                instanceName = (string)prop.Value;
                                break;
                            case "VideoOutputTechnology":
                                var tech = (VIDEO_OUTPUT_TECHNOLOGY)prop.Value;
                                videoOutputTech = Enum.GetName(typeof(VIDEO_OUTPUT_TECHNOLOGY), tech) ?? "unknown";
                                break;
                            case "Active":
                                isActive = (bool)prop.Value;
                                if (isActive)
                                {
                                    foundActiveMonitor = true;
                                }                                
                                break;
                        }

                    }

                    EventLogger.LogEvent($"[Monitor] Instance Name: {instanceName}, Video Output Technology: {videoOutputTech}, Is Active: {isActive}", EventLogEntryType.Information);

                    if (foundActiveMonitor) break;
                }

                return foundActiveMonitor;  // An active display is considered the same as an open lid.
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent($"[{_programName}] Failed to check lid status in IsLidOpen: {ex.Message}", EventLogEntryType.Error);
            }
            return true;
        }


        /*
        public event EventHandler<LidEventArgs>? LidStateChanged;

        private const int SERVICE_CONTROL_POWEREVENT = 0x0000000D;
        // https://learn.microsoft.com/ja-jp/windows/win32/api/winsvc/nc-winsvc-lphandler_function_ex
        private const int PBT_POWERSETTINGCHANGE = 0x8013;
        // https://learn.microsoft.com/en-us/windows/win32/power/pbt-powersettingchange

        private static Guid GUID_LIDSWITCH_STATE_CHANGE;// = new Guid(0xBA3E0F4D, 0xB817, 0x4094, 0xA2, 0xD1, 0xD5, 0x63, 0x79, 0xE6, 0xA0, 0xF3);
        // https://learn.microsoft.com/en-us/windows/win32/power/power-setting-guids#GUID_LIDSWITCH_STATE_CHANGE
        // GUID_LIDSWITCH_STATE_CHANGE(BA3E0F4D-B817-4094-A2D1-D56379E6A0F3)
        // The state of the lid has changed(open vs. closed). The callback won't be called until a lid device is found and its current state is known.
        // The Data member is a DWORD that indicates the current lid state:
        // 0x0 - The lid is closed.
        // 0x1 - The lid is opened.

        [StructLayout(LayoutKind.Sequential)]
        private struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }

        private delegate IntPtr LidNotificationHandler(int control, int eventType, IntPtr eventData, IntPtr context);
        private LidNotificationHandler _callback;
        // https://learn.microsoft.com/en-us/windows/win32/api/winsvc/nc-winsvc-lphandler_function_ex

        // [Service Control Handler Function](https://learn.microsoft.com/en-us/windows/win32/services/service-control-handler-function)
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern IntPtr RegisterServiceCtrlHandlerEx(string serviceName, LidNotificationHandler handler, IntPtr context);
        // https://learn.microsoft.com/en-us/windows/win32/api/winsvc/nf-winsvc-registerservicectrlhandlerexa

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, uint Flags);
        // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerpowersettingnotification
        private const uint DEVICE_NOTIFY_WINDOW_HANDLE = 0;
        private const uint DEVICE_NOTIFY_SERVICE_HANDLE = 1;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterPowerSettingNotification(IntPtr hRecipient);

        private IntPtr _registrationHandle;
        private IntPtr _serviceStatusHandle;

        private bool _isService;
        private string _programName;

        public bool IsLidClosed { get; private set; }

        public LidMonitor(bool isService, string serviceName, IntPtr recipientHandle)
        {
            IsLidClosed = false;
            _isService = isService;
            _programName = _isService ? "Service" : "application";

            _callback = NotificationCallback;

            try
            {
                GUID_LIDSWITCH_STATE_CHANGE = new Guid(0xBA3E0F4D, 0xB817, 0x4094, 0xA2, 0xD1, 0xD5, 0x63, 0x79, 0xE6, 0xA0, 0xF3);                
                var handleFlag = _isService ? DEVICE_NOTIFY_SERVICE_HANDLE : DEVICE_NOTIFY_WINDOW_HANDLE;
                _registrationHandle = RegisterPowerSettingNotification(recipientHandle, ref GUID_LIDSWITCH_STATE_CHANGE, handleFlag);

                if (_registrationHandle != IntPtr.Zero)
                {
#if DEBUG || TEST
                    EventLogger.LogEvent("Registered Power Setting Notification", EventLogEntryType.Information);
#endif
                }
                else
                {
                    var errCode = Marshal.GetLastWin32Error();
                    Debug.Assert(errCode != 1083 || false, "ERROR_SERVICE_NOT_IN_EXE: The executable program that this service is configured to run in does not implement the service.");
                    // https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes--1000-1299-
                    throw new System.ComponentModel.Win32Exception(errCode, $" [{errCode} (0x{errCode:X8})] Failed to call RegisterPowerSettingNotification.");
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent($"[{_programName}] Failed to initialize LidMonitor: {ex.Message}", EventLogEntryType.Error);
                Dispose();
                throw new Exception("Failed to initialize LidMonitor.", ex);
            }

            if (_isService)
            {
                try
                {
                    _serviceStatusHandle = RegisterServiceCtrlHandlerEx(serviceName, _callback, IntPtr.Zero);  // Unregister is not required
                    if (_serviceStatusHandle != IntPtr.Zero)
                    {
#if DEBUG || TEST
                        EventLogger.LogEvent("Registered Service Control Handler", EventLogEntryType.Information);
#endif
                    }
                    else
                    {
                        var errCode = Marshal.GetLastWin32Error();
                        Debug.Assert(errCode != -1073741819 || false, "Access Violation: Run as a service.");
                        // https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes--1000-1299-
                        throw new System.ComponentModel.Win32Exception(errCode, $" [{errCode} (0x{errCode:X8})] Failed to call RegisterServiceCtrlHandlerEx.");
                    }
                }
                catch (Exception ex)
                {
                    EventLogger.LogEvent($"[{_programName}] Failed to initialize LidMonitor - service control handle: {ex.Message}", EventLogEntryType.Error);
                    throw new Exception("Failed to initialize LidMonitor - service control handle.", ex);
                }
            }
            else
            {
                // This functionality is not implemented because there is no plan to use it within the WPF application.
                // Please specify the window handle that will receive notifications in `_recipientHandle`.
                // If necessary, use the CreateWindowEx function to create a hidden window for receiving messages, and specify its handle.
                // Notifications will not be sent to `NotificationCallback`, but instead, they will be sent to the `WndProc` of that window.
                Debug.Assert(false, "Not Implemented!!!");
                throw new Exception("Failed to initialize LidMonitor - window handle.");
            }
        }

        private IntPtr NotificationCallback(int control, int eventType, IntPtr eventData, IntPtr context)
        {
            if (control == SERVICE_CONTROL_POWEREVENT && eventType == PBT_POWERSETTINGCHANGE)
            {
                // If control is SERVICE_CONTROL_POWEREVENT and eventType is PBT_POWERSETTINGCHANGE,
                // eventData is a pointer to a POWERBROADCAST_SETTING structure.
                var ps = Marshal.PtrToStructure<POWERBROADCAST_SETTING>(eventData);
                if (ps.PowerSetting == GUID_LIDSWITCH_STATE_CHANGE)
                {
                    bool lidClosed = (ps.Data == 0);
                    IsLidClosed = lidClosed;
                    LidStateChanged?.Invoke(this, new LidEventArgs(lidClosed));
                }
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            try
            {
                if (_registrationHandle != IntPtr.Zero)
                {
                    UnregisterPowerSettingNotification(_registrationHandle);
                    _registrationHandle = IntPtr.Zero;
#if DEBUG || TEST
                    EventLogger.LogEvent("Unregistered Power Setting Notification", EventLogEntryType.Information);
#endif
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent($"[{_programName}] Failed to unregister LidMonitor : {ex.Message}", EventLogEntryType.Error);
            }
        }
        */
    }
}
