using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinSleepWell
{
    public class PowerEventArgs : EventArgs
    {
        public string Reason { get; }

        public PowerEventArgs(string reason)
        {
            Reason = reason;
        }
    }

    public class PowerMonitor : IDisposable
    {
        public event EventHandler<PowerEventArgs>? Suspend;
        public event EventHandler<PowerEventArgs>? Resume;

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
        private DeviceNotifyCallbackRoutine _callback;

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

        private bool _isService;
        private string _programName;

        public bool IsSleeping { get; private set; }


        public PowerMonitor(bool isService)
        {
            IsSleeping = false;
            _isService = isService;
            _programName = _isService ? "Service" : "application";

            try
            {
                _callback = NotificationCallback;
                _gcHandle = GCHandle.Alloc(_callback);

                var parameters = new DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS
                {
                    Callback = _callback,
                    Context = IntPtr.Zero
                };

                var recipient = Marshal.AllocHGlobal(Marshal.SizeOf(parameters));
                Marshal.StructureToPtr(parameters, recipient, false);

                var result = PowerRegisterSuspendResumeNotification(DEVICE_NOTIFY_CALLBACK, recipient, out _registrationHandle);

                Marshal.FreeHGlobal(recipient);

                if (result != 0)
                {
                    EventLogger.LogEvent($"[{_programName}] Failed to register suspend resume notification.", EventLogEntryType.Error);
                    Dispose();
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Failed to register suspend resume notification.");
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent($"[{_programName}] Failed to initialize PowerMonitor: " + ex.Message, EventLogEntryType.Error);
                Dispose();
                throw new Exception("Failed to initialize PowerMonitor.", ex);
            }
        }

        private uint NotificationCallback(IntPtr context, int type, IntPtr setting)
        {
            try
            {
                switch (type)
                {
                    case PBT_APMSUSPEND:
#if DEBUG
                        EventLogger.LogEvent("PBT_APMSUSPEND", EventLogEntryType.Information);
#endif
                        // System is suspending operation
                        //スリープした理由は見なくてよい。
                        Suspend?.Invoke(this, new PowerEventArgs(""));
                        IsSleeping = true;
                        return 0;
                    case PBT_APMRESUMEAUTOMATIC:
#if DEBUG
                        EventLogger.LogEvent("PBT_APMRESUMEAUTOMATIC", EventLogEntryType.Information);
#endif                  
                        // System is resuming automatically from a low-power state
                        var reason = GetResumeReasonFromEventLog();
                        Resume?.Invoke(this, new PowerEventArgs($"reason: {reason}"));
                        IsSleeping = false;
                        return 0;
                    case PBT_APMRESUMECRITICAL:
#if DEBUG
                        EventLogger.LogEvent("PBT_APMRESUMECRITICAL", EventLogEntryType.Information);
#endif
                        // System is resuming after a critical suspension
                        Resume?.Invoke(this, new PowerEventArgs("reason after a critical suspension"));
                        IsSleeping = false;
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
            }
            catch (Exception ex)
            {
                var errorMessage = $"[{_programName}] Error in NotificationCallback: {ex.Message}\nStack Trace: {ex.StackTrace}\nContext: {context}\nType: {type}";
                EventLogger.LogEvent(errorMessage, EventLogEntryType.Error);

                // Optional: return an error code specific to your application
                return 1;
            }
            return 0;
        }

        private string GetResumeReasonFromEventLog()
        {
            try
            {
                // Access the event log
                using (var eventLog = new EventLog("System"))
                {
                    // Use LINQ to filter by EventID and get the latest one
                    var latestEntry = eventLog.Entries.Cast<EventLogEntry>()
                                        .Where(entry => entry.InstanceId == 506)  // Look for Event ID 506 which is typically related to resume
                                        .LastOrDefault();
                    var reason = latestEntry?.ReplacementStrings[0] ?? "";  // The first replacement string usually contains the reason
                    return reason.ToString();
                    // [モダン スタンバイの SleepStudy | Microsoft Learn](https://learn.microsoft.com/ja-jp/windows-hardware/design/device-experiences/modern-standby-sleepstudy)
                    // 1   電源ボタン　＝人力
                    // 15  カバー　＝人力
                    // 11  画面オフ要求　＝謎
                    // 20  休止状態、またはシャットダウン　＝人力
                    // 16777220    PDC タスク クライアント: メンテナンス スケジューラー　＝この場合はメンテナンスを待って復帰する
                    // 4	ユーザー入力
                    // 32	入力マウス　＝この場合の復帰は強制的にスリープにすればよい
                    // 33  入力タッチパッド　＝この場合の復帰は強制的にスリープにすればよい
                    //+		TimeGenerated	{2024/07/23 14:00:29}	System.DateTime 時間が1秒以内などでないとおかしい。
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent($"[{_programName}] Failed to retrieve resume reason from event log: {ex.Message}", EventLogEntryType.Error);
            }
            return "Unknown Resume Reason";
        }


        public void Dispose()
        {
            try
            {
                if (_registrationHandle != IntPtr.Zero)
                {
                    PowerUnregisterSuspendResumeNotification(_registrationHandle);
                    _registrationHandle = IntPtr.Zero;
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent($"[{_programName}] Failed to unregister PowerMonitor : {ex.Message}", EventLogEntryType.Error);
            }

            try
            {
                if (_gcHandle.IsAllocated)
                {
                    _gcHandle.Free();
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent($"[{_programName}] Failed to dispose PowerMonitor: {ex.Message}", EventLogEntryType.Error);
            }
        }
    }
}
