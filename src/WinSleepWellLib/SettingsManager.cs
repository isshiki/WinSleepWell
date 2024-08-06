using System.IO;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace WinSleepWell
{
    public class SettingsManager
    {
        private string _settingsFilePath = "settings.json";

        public void SaveSettings(Settings settings)
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(_settingsFilePath, json);
        }

        public Settings LoadSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                var settings = JsonConvert.DeserializeObject<Settings>(json);
                return settings ?? new Settings(); // nullの場合、新しいSettingsオブジェクトを返す
            }
            return new Settings();
        }


    }
}
