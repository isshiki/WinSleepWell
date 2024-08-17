using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinSleepWell
{
    public class LidEventArgs : EventArgs
    {
        public bool IsLidClosed { get; }

        public LidEventArgs(bool isLidClosed)
        {
            IsLidClosed = isLidClosed;
        }
    }

    public class LidMonitor : IDisposable
    {
        public event EventHandler<LidEventArgs>? LidStateChanged;

        private const int SERVICE_CONTROL_POWEREVENT = 0x0000000D;
        // https://learn.microsoft.com/ja-jp/windows/win32/api/winsvc/nc-winsvc-lphandler_function_ex
        private const int PBT_POWERSETTINGCHANGE = 0x8013;
        // https://learn.microsoft.com/en-us/windows/win32/power/pbt-powersettingchange

        private static Guid GUID_LIDSWITCH_STATE_CHANGE = new Guid(0xBA3E0F4D, 0xB817, 0x4094, 0xA2, 0xD1, 0xD5, 0x63, 0x79, 0xE6, 0xA0, 0xF3);
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

        //private delegate IntPtr WndProcCallback(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        //private WndProcCallback _callback;

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

        private IntPtr _recipientHandle;
        private IntPtr _registrationHandle;

        private bool _isService;
        private string _programName;

        public bool IsLidClosed { get; private set; }

        public LidMonitor(bool isService, string serviceName= "WinSleepWellService")
        {
            IsLidClosed = false;
            _isService = isService;
            _programName = _isService ? "Service" : "application";

            _callback = NotificationCallback;

            if (_isService)
            {
                try
                {
                    _recipientHandle = RegisterServiceCtrlHandlerEx(serviceName, _callback, IntPtr.Zero);  // Unregister is not required

                    if (_recipientHandle != IntPtr.Zero)
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
                        throw new System.ComponentModel.Win32Exception(errCode, $" [{errCode}(0x{errCode:X8})] Failed to register service control handler.");
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

            try
            {
                var handleFlag = _isService ? DEVICE_NOTIFY_SERVICE_HANDLE : DEVICE_NOTIFY_WINDOW_HANDLE;
                _registrationHandle = RegisterPowerSettingNotification(_recipientHandle, ref GUID_LIDSWITCH_STATE_CHANGE, handleFlag);

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
                    throw new System.ComponentModel.Win32Exception(errCode, $" [{errCode}(0x{errCode:X8})] Failed to register lid switch state change notification.");
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent($"[{_programName}] Failed to initialize LidMonitor: {ex.Message}", EventLogEntryType.Error);
                Dispose();
                throw new Exception("Failed to initialize LidMonitor.", ex);
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
    }
}
