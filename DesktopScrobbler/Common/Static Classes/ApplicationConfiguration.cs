using System;
using System.IO;
using System.Linq;
using LastFM.Common.Classes;
using LastFM.Common.Factories;
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

                CheckPluginDefaultStatus();
            }
        }

        public static void CheckPluginDefaultStatus()
        {
            foreach (IScrobbleSource plugin in ScrobbleFactory.ScrobblePlugins)
            {
                if (Settings.ScrobblerStatus.FirstOrDefault(settingsPlugin => settingsPlugin.Identifier == plugin.SourceIdentifier) == null)
                {
                    Settings.ScrobblerStatus.Add(new ScrobblerSourceStatus() { Identifier = plugin.SourceIdentifier, IsEnabled=true});
                }
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
