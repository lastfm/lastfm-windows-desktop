using System;
using System.IO;
using Newtonsoft.Json;

namespace LastFM.Common
{
    public static class ApplicationConfiguration
    {
        public static Settings Settings { get; set; } = null;

        public static void Initialize()
        {
            try
            {
                string settingsFileContent = File.ReadAllText($"{Core.UserSettingsPath}{Core.FILENAME_SETTINGS}");

                if (!string.IsNullOrEmpty(settingsFileContent))
                {
                    Settings = JsonConvert.DeserializeObject<Settings>(settingsFileContent);
                }
            }
            catch (Exception ex)
            {
                // Not much we can do if we can't read the settings file....
                Settings = new Settings();
            }
        }

        public static void SaveSettings()
        {
            try
            {
                File.WriteAllText($"{Core.UserSettingsPath}{Core.FILENAME_SETTINGS}", JsonConvert.SerializeObject(Settings));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
