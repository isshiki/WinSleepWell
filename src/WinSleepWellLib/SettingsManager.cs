using System.Diagnostics;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace WinSleepWell
{
    public class SettingsManager
    {
        private bool _isService;
        private string _programName;
        private string _settingsFilePath;

        public SettingsManager(bool isService)
        {
            _isService = isService;
            _programName = _isService ? "Service" : "application";

            try
            {
                // 1. Check if settings.json exists in the bin\Release root directory
                string binReleasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "settings.json");

                // 2. If not found, check in the same directory as the executable (App or Service)
                string exeDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

                if (File.Exists(binReleasePath))
                {
                    _settingsFilePath = Path.GetFullPath(binReleasePath);
                }
                else if (File.Exists(exeDirectoryPath))
                {
                    _settingsFilePath = Path.GetFullPath(exeDirectoryPath);
                }
                else
                {
                    // Default to the executable directory if none found
                    _settingsFilePath = exeDirectoryPath;
                }
            }
            catch (Exception ex)
            {
                _settingsFilePath = "settings.json";
                HandleError("Failed to get settings.json file path.", ex);
            }
        }

        public void Dispose()
        {
            // Do nothing
        }

        public void SaveSettings(Settings settings)
        {
            try
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                HandleError($"Failed to save settings to {_settingsFilePath}.", ex);
            }
        }

        public Settings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    return JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
                }
                return new Settings();
            }
            catch (Exception ex)
            {
                HandleError($"Failed to load settings from {_settingsFilePath}.", ex);
                return new Settings(); // Return default settings if loading fails
            }
        }

        private void HandleError(string message, Exception ex)
        {
            string programName = _isService ? "Service" : "application";
            string fullMessage = $"{message}\n\nError Details: {ex.Message}";

            EventLogger.LogEvent($"[{programName}] {fullMessage}", EventLogEntryType.Error);

            if (!_isService && Environment.UserInteractive)
            {
                MessageBox.Show(fullMessage, $"Error in WinSleepWell {programName}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
