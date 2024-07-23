using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

namespace WinSleepWell
{
    public class PowerMonitor
    {
        public event EventHandler Suspend;
        public event EventHandler Resume;

        private const int DEVICE_NOTIFY_CALLBACK = 2;
        
        private const int PBT_APMSUSPEND = 4; // (0x4) - System is suspending operation.
        private const int PBT_APMRESUMEAUTOMATIC = 18; // (0x12) - Operation is resuming automatically from a low-power state.This message is sent every time the system resumes.
        private const int PBT_APMRESUMESUSPEND = 7; // (0x7) - Operation is resuming from a low-power state.This message is sent after PBT_APMRESUMEAUTOMATIC if the resume is triggered by user input, such as pressing a key.

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
                    //EventLogger.LogEvent("PBT_APMSUSPEND", EventLogEntryType.Information);
                    Suspend?.Invoke(this, EventArgs.Empty);
                    return 0;
                case PBT_APMRESUMEAUTOMATIC:
                    //EventLogger.LogEvent("PBT_APMRESUMEAUTOMATIC", EventLogEntryType.Information);
                    Resume?.Invoke(this, EventArgs.Empty);
                    return 0;
                case PBT_APMRESUMESUSPEND:
                    //EventLogger.LogEvent("PBT_APMRESUMESUSPEND", EventLogEntryType.Information);
                    break;
                default:
                    //EventLogger.LogEvent("PBT_TypeID_" + type.ToString(), EventLogEntryType.Information);
                    break;
            }
            return 1;
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
