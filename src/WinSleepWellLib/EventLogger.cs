using System;
using System.Diagnostics;

namespace WinSleepWell
{
    public static class EventLogger
    {
        public static void LogEvent(string message, EventLogEntryType type)
        {
            try
            {
                if (!EventLog.SourceExists("WinSleepWell"))
                {
                    EventLog.CreateEventSource("WinSleepWell", "Application");
                }

                using (var eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "WinSleepWell";
                    eventLog.WriteEntry(message, type);
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
