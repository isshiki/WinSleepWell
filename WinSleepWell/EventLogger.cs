using System.Diagnostics;

namespace WinSleepWell
{
    public static class EventLogger
    {
        public static void LogEvent(string message, EventLogEntryType type)
        {
            if (!EventLog.SourceExists("WinSleepWell"))
            {
                EventLog.CreateEventSource("WinSleepWell", "System");
            }

            using (EventLog eventLog = new EventLog("System"))
            {
                eventLog.Source = "WinSleepWell";
                eventLog.WriteEntry(message, type);
            }
        }
    }
}
