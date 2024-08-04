using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinSleepWell
{
    public class PowerEventArgs : EventArgs
    {
        public string Message { get; }

        public PowerEventArgs(string message)
        {
            Message = message;
        }
    }

    public class PowerMonitor
    {
        public event EventHandler<PowerEventArgs> Suspend;
        public event EventHandler<PowerEventArgs> Resume;

        private const int DEVICE_NOTIFY_CALLBACK = 2;
        
        private const int PBT_APMSUSPEND = 4; // (0x4) - System is suspending operation.
        private const int PBT_APMRESUMEAUTOMATIC = 18; // (0x12) - Operation is resuming automatically from a low-power state.This message is sent every time the system resumes.
        private const int PBT_APMRESUMESUSPEND = 7; // (0x7) - Operation is resuming from a low-power state.This message is sent after PBT_APMRESUMEAUTOMATIC if the resume is triggered by user input, such as pressing a key.
        private const int PBT_APMRESUMECRITICAL = 6; // (0x6) - Operation is resuming after a critical suspension.
        // Other power management messages
        private const int PBT_APMBATTERYLOW = 9; // (0x9) - The system's battery power is low.
        private const int PBT_APMOEMEVENT = 11; // (0xB) - OEM-defined event occurred.
        private const int PBT_APMPOWERSTATUSCHANGE = 10; // (0xA) - Power status has changed.
        private const int PBT_APMQUERYSUSPEND = 0; // (0x0) - The system is requesting permission to suspend.
        private const int PBT_APMQUERYSUSPENDFAILED = 2; // (0x2) - Permission to suspend was denied.
        private const int PBT_POWERSETTINGCHANGE = 32787; // (0x8013) - A power setting change event occurred.

        private delegate uint DeviceNotifyCallbackRoutine(IntPtr context, int type, IntPtr setting);

        [StructLayout(LayoutKind.Sequential)]
        struct DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS
        {
            public DeviceNotifyCallbackRoutine Callback;
            public IntPtr Context;
        }

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerRegisterSuspendResumeNotification(int flags, IntPtr recipient, out IntPtr registrationHandle);

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerUnregisterSuspendResumeNotification(IntPtr registrationHandle);

        private IntPtr _registrationHandle;
        private GCHandle _gcHandle;

        public bool IsSuspended { get; private set; }

        public PowerMonitor()
        {
            var callback = new DeviceNotifyCallbackRoutine(NotificationCallback);
            _gcHandle = GCHandle.Alloc(callback);
            var parameters = new DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS
            {
                Callback = new DeviceNotifyCallbackRoutine(NotificationCallback),
                Context = IntPtr.Zero
            };

            var recipient = Marshal.AllocHGlobal(Marshal.SizeOf(parameters));
            Marshal.StructureToPtr(parameters, recipient, false);

            var result = PowerRegisterSuspendResumeNotification(DEVICE_NOTIFY_CALLBACK, recipient, out _registrationHandle);

            Marshal.FreeHGlobal(recipient);

            if (result != 0)
            {
                _gcHandle.Free();
                EventLogger.LogEvent("Something went wrong: DEVICE_NOTIFY_CALLBACK", EventLogEntryType.Error);
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private uint NotificationCallback(IntPtr context, int type, IntPtr setting)
        {
            switch (type)
            {
                case PBT_APMSUSPEND:
#if DEBUG
                    EventLogger.LogEvent("PBT_APMSUSPEND", EventLogEntryType.Information);
#endif
                    // System is suspending operation
                    Suspend?.Invoke(this, new PowerEventArgs(" on PBT_APMSUSPEND"));
                    IsSuspended = true;
                    return 0;
                case PBT_APMRESUMEAUTOMATIC:
#if DEBUG
                    EventLogger.LogEvent("PBT_APMRESUMEAUTOMATIC", EventLogEntryType.Information);
#endif                    // System is resuming automatically from a low-power state
                    Resume?.Invoke(this, new PowerEventArgs(" on PBT_APMRESUMEAUTOMATIC"));
                    IsSuspended = false;
                    return 0;
                case PBT_APMRESUMECRITICAL:
#if DEBUG
                    EventLogger.LogEvent("PBT_APMRESUMECRITICAL", EventLogEntryType.Information);
#endif
                    // System is resuming after a critical suspension
                    Resume?.Invoke(this, new PowerEventArgs(" on PBT_APMRESUMECRITICAL"));
                    IsSuspended = false;
                    return 0;
                case PBT_APMRESUMESUSPEND:
                    // System is resuming from a low-power state triggered by user input
#if DEBUG
                    EventLogger.LogEvent("PBT_APMRESUMESUSPEND", EventLogEntryType.Information);
#endif
                    return 0;
                case PBT_APMBATTERYLOW:
                    // System's battery power is low
#if DEBUG
                    EventLogger.LogEvent("PBT_APMBATTERYLOW", EventLogEntryType.Information);
#endif
                    break;
                case PBT_APMOEMEVENT:
                    // OEM-defined event occurred
#if DEBUG
                    EventLogger.LogEvent("PBT_APMOEMEVENT", EventLogEntryType.Information);
#endif
                    break;
                case PBT_APMPOWERSTATUSCHANGE:
                    // Power status has changed
#if DEBUG
                    EventLogger.LogEvent("PBT_APMPOWERSTATUSCHANGE", EventLogEntryType.Information);
#endif
                    break;
                case PBT_APMQUERYSUSPEND:
                    // System is requesting permission to suspend
#if DEBUG
                    EventLogger.LogEvent("PBT_APMQUERYSUSPEND", EventLogEntryType.Information);
#endif
                    break;
                case PBT_APMQUERYSUSPENDFAILED:
                    // Permission to suspend was denied
#if DEBUG
                    EventLogger.LogEvent("PBT_APMQUERYSUSPENDFAILED", EventLogEntryType.Information);
#endif
                    break;
                case PBT_POWERSETTINGCHANGE:
                    // A power setting change event occurred
#if DEBUG
                    EventLogger.LogEvent("PBT_POWERSETTINGCHANGE", EventLogEntryType.Information);
#endif
                    break;
                default:
                    // Other power management messages
#if DEBUG
                    EventLogger.LogEvent("PBT_TypeID_" + type.ToString(), EventLogEntryType.Information);
#endif
                    break;
            }
            return 0;
        }

        public void Dispose()
        {
            if (_registrationHandle != IntPtr.Zero)
            {
                PowerUnregisterSuspendResumeNotification(_registrationHandle);
                _registrationHandle = IntPtr.Zero;
            }

            if (_gcHandle.IsAllocated)
            {
                _gcHandle.Free();
            }
        }
    }
}
