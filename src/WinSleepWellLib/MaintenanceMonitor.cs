using System;
using Microsoft.Win32.TaskScheduler;


namespace WinSleepWell
{
    public class MaintenanceMonitor : IDisposable
    {
        private bool _isService;
        private string _programName;

        public MaintenanceMonitor(bool isService)
        {
            _isService = isService;
            _programName = _isService ? "Service" : "application";
        }

        public void Dispose()
        {
            // Do nothing
        }

        public bool IsAutomaticMaintenanceRunning()
        {
            using (var ts = new TaskService())
            {
                return CheckFolderForRunningMaintenanceTasks(ts.RootFolder);
            }
        }

        // Check if any maintenance tasks are running in the given folder
        private bool CheckFolderForRunningMaintenanceTasks(TaskFolder folder)
        {
            // Check tasks in the current folder
            foreach (var task in folder.AllTasks)
            {
                if (task.Xml.Contains("<MaintenanceSettings>"))
                {
                    if (task.State == TaskState.Running)
                    {
                        return true; // Return true if any task is running
                    }
                }
            }

            // All automatic maintenance tasks are located in the root folder, so the following is unnecessary
            //// Recursively check subfolders
            //foreach (var subFolder in folder.SubFolders)
            //{
            //    if (CheckFolderForRunningMaintenanceTasks(subFolder))
            //    {
            //        return true; // Return true if any running tasks are found in the subfolder
            //    }
            //}

            return false; // Return false if no tasks are running
        }

        // The following are examples of tasks associated with automatic maintenance on this system.
        // These tasks may vary depending on the environment and the specific configuration of Windows.
        // Examples of automatic maintenance tasks:
        // "IMESharePointDictionary",
        // ".NET Framework NGEN v4.0.30319",
        // ".NET Framework NGEN v4.0.30319 64",
        // ".NET Framework NGEN v4.0.30319 64 Critical",
        // ".NET Framework NGEN v4.0.30319 Critical",
        // "ProgramDataUpdater",
        // "StartupAppTask",
        // "appuriverifierdaily",
        // "CleanupTemporaryState",
        // "DsSvcCleanup",
        // "Backup",
        // "Pre-staged app cleanup",
        // "BgTaskRegistrationMaintenanceTask",
        // "ProactiveScan",
        // "UsbCeip",
        // "ScheduledDefrag",
        // "Scheduled",
        // "SilentCleanup",
        // "Microsoft-Windows-DiskDiagnosticDataCollector",
        // "Diagnostics",
        // "StorageSense",
        // "DmClient",
        // "File History (maintenance mode)",
        // "UsageDataReporting",
        // "ScanForUpdatesAsUser",
        // "WinSAT",
        // "RunFullMemoryDiagnostic",
        // "LPRemove",
        // "AnalyzeSystem",
        // "VerifyWinRE",
        // "RegIdleBackup",
        // "StartComponentCleanup",
        // "BackgroundUploadTask",
        // "Account Cleanup",
        // "IndexerAutomaticMaintenance",
        // "ThemesSyncedImageDownload",
        // "MaintenanceTasks",
        // "HybridDriveCachePrepopulate",
        // "HybridDriveCacheRebalance",
        // "ResPriStaticDbSync",
        // "WsSwapAssessmentTask",
        // "SR",
        // "SynchronizeTime",
        // "SynchronizeTimeZone",
        // "Windows Defender Cache Maintenance",
        // "Windows Defender Cleanup",
        // "Windows Defender Scheduled Scan",
        // "Windows Defender Verification",
        // "Work Folders Maintenance Work"
    }

}