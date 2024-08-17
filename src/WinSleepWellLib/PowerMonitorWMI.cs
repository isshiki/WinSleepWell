/*
The PowerMonitor class in this project has two versions: a fast, low-level Win32 API version and a simpler, slightly higher-level WMI version.
For production, we have adopted the Win32 API version in order to prioritize real-time performance. This PowerMonitor class is implemented in the PowerMonitor.cs file.
The PowerMonitorWMI class implemented in this PowerMonitorWMI.cs file is the unused WMI version, retained for potential future functionality changes and possibilities.
*/

using System;
using System.Diagnostics;
using System.Management;
using System.Text;

namespace WinSleepWell
{
    public class PowerEventWMIArgs : EventArgs
    {
        public string Reason { get; }

        public PowerEventWMIArgs(string reason)
        {
            Reason = reason;
        }
    }

    public class PowerMonitorWMI : IDisposable
    {
        public event EventHandler<PowerEventWMIArgs>? Suspend;
        public event EventHandler<PowerEventWMIArgs>? Resume;

        private ManagementEventWatcher _suspendWatcher;
        private ManagementEventWatcher _resumeWatcher;

        private bool _isService;
        private string _programName;

        public bool IsSleeping { get; private set; }

        public PowerMonitorWMI(bool isService)
        {
            IsSleeping = false;
            _isService = isService;
            _programName = _isService ? "Service" : "Application";

            try
            {
                // Monitor Windows Suspend
                _suspendWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_PowerManagementEvent WHERE EventType = 4")); // EventType 4 is for suspend
                _suspendWatcher.EventArrived += new EventArrivedEventHandler(SuspendNotification);
                _suspendWatcher.Start();

                // Monitor Windows Resume
                _resumeWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_PowerManagementEvent WHERE EventType = 7")); // EventType 7 is for resume
                _resumeWatcher.EventArrived += new EventArrivedEventHandler(ResumeNotification);
                _resumeWatcher.Start();
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent($"[{_programName}] Error initializing PowerMonitor: {ex.Message}", EventLogEntryType.Error);
                Dispose();
                throw;
            }
        }

        private void SuspendNotification(object sender, EventArrivedEventArgs e)
        {
            try
            {
                // System is suspending operation
                //var reason = e.NewEvent.Properties["Reason"]?.Value as string ?? "";
                var reason = "Suspending...";
                LogEventProperties(e);
                Suspend?.Invoke(this, new PowerEventWMIArgs(reason));
                IsSleeping = true;
            }
            catch (Exception ex)
            {
                var errorMessage = $"[{_programName}] Error in SuspendNotification: {ex.Message}\nStack Trace: {ex.StackTrace}";
                EventLogger.LogEvent(errorMessage, EventLogEntryType.Error);
            }
        }

        private void ResumeNotification(object sender, EventArrivedEventArgs e)
        {
            try
            {
                // System is resuming operation
                //var reason = e.NewEvent.Properties["Reason"]?.Value as string ?? "";
                var reason = GetResumeReasonFromEventLog();
                LogEventProperties(e);
                Resume?.Invoke(this, new PowerEventWMIArgs(reason));
                IsSleeping = false;
            }
            catch (Exception ex)
            {
                var errorMessage = $"[{_programName}] Error in ResumeNotification: {ex.Message}\nStack Trace: {ex.StackTrace}";
                EventLogger.LogEvent(errorMessage, EventLogEntryType.Error);
            }
        }

        private string GetResumeReasonFromEventLog()
        {
            try
            {
                // Access the system event log
                var eventLog = new EventLog("System");
                foreach (EventLogEntry entry in eventLog.Entries)
                {
                    // Look for Event ID 506 which is typically related to resume
                    if (entry.InstanceId == 506)
                    {
                        var reason = entry.ReplacementStrings[0]; // The first replacement string usually contains the reason
                        return $"Resume Reason: {reason}";
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent($"[{_programName}] Error retrieving resume reason from event log: {ex.Message}", EventLogEntryType.Error);
            }
            return "Unknown Resume Reason";
        }

        // Logs all properties of the event to the event log in a single entry
        private void LogEventProperties(EventArrivedEventArgs e)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("Event Properties:");

                foreach (var property in e.NewEvent.Properties)
                {
                    sb.AppendLine($"Property Name: {property.Name}, Value: {property.Value}");
                }

                EventLogger.LogEvent(sb.ToString(), EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent($"[{_programName}] Error logging event properties: {ex.Message}", EventLogEntryType.Error);
            }
        }

        public void Dispose()
        {
            try
            {
                if (_suspendWatcher != null)
                {
                    _suspendWatcher.Stop();
                    _suspendWatcher.Dispose();
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent($"[{_programName}] Error disposing PowerMonitor suspend: {ex.Message}", EventLogEntryType.Error);
            }

            try
            {
                if (_resumeWatcher != null)
                {
                    _resumeWatcher.Stop();
                    _resumeWatcher.Dispose();
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent($"[{_programName}] Error disposing PowerMonitor resume: {ex.Message}", EventLogEntryType.Error);
            }
        }
    }
}
