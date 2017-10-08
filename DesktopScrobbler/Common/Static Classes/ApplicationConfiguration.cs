using System;
using System.IO;
using System.Linq;
using LastFM.Common.Classes;
using LastFM.Common.Factories;
using Newtonsoft.Json;

namespace LastFM.Common
{
    /// <summary>
    /// Houses the application configuration settings, as well as reading / writing them
    /// </summary>
    public static class ApplicationConfiguration
    {
        /// <summary>
        /// The instancing of the current application settings
        /// </summary>
        public static Settings Settings { get; set; } = null;

        /// <summary>
        /// An initializer for the setting, which will load the settings file (if it exists)
        /// or creates a new set of settings, and saves them immediately.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // Read the contents of the settings file (from %appdata%)
                string settingsFileContent = File.ReadAllText($"{Core.UserSettingsPath}{Core.FILENAME_SETTINGS}");

                if (!string.IsNullOrEmpty(settingsFileContent))
                {
                    // Try and deserialize them
                    Settings = JsonConvert.DeserializeObject<Settings>(settingsFileContent);
                }
            }
            catch (Exception ex)
            {
                // Not much we can do if we can't read the settings file....
                Settings = new Settings();

                // Check the plugins have an appropriate default 'enabled' state registered in the settings
                // (in case new plugins are loaded)
                CheckPluginDefaultStatus();
            }
        }


        /// <summary>
        /// Checks the default state of the plugins to make sure configuration entries have been created for them
        /// </summary>
        public static void CheckPluginDefaultStatus()
        {
            // The Last.fm preferred configuration for this application is to have iTunes on by default, and Windows Media Player disabled
            // This is because this application exists to solve the problem that iTunes broke their event driven model, by removing the events.
            IScrobbleSource windowsMediaSource = ScrobbleFactory.ScrobblePlugins.FirstOrDefault(plugin => plugin.SourceIdentifier == Guid.Parse("a458e8af-4282-4bd7-8894-14969c63a7d5"));
            IScrobbleSource iTunesSource = ScrobbleFactory.ScrobblePlugins.FirstOrDefault(plugin => plugin.SourceIdentifier == Guid.Parse("7471fa52-0007-43c9-a644-945fbc7f5897"));

            // Check to see if anything has been configured already, if not, add our two defaults
            if (Settings.ScrobblerStatus.Count == 0)
            {
                Settings.ScrobblerStatus.Add(new ScrobblerSourceStatus() { Identifier = iTunesSource.SourceIdentifier, IsEnabled = true });
                Settings.ScrobblerStatus.Add(new ScrobblerSourceStatus() { Identifier = windowsMediaSource.SourceIdentifier, IsEnabled = false });
            }

            // Now iterate all plugins (in case any others have been loaded) and default them to being off
            foreach (IScrobbleSource plugin in ScrobbleFactory.ScrobblePlugins)
            {
                if (Settings.ScrobblerStatus.FirstOrDefault(settingsPlugin => settingsPlugin.Identifier == plugin.SourceIdentifier) == null)
                {
                    Settings.ScrobblerStatus.Add(new ScrobblerSourceStatus() { Identifier = plugin.SourceIdentifier, IsEnabled=false});
                }
            }
        }

        /// <summary>
        /// Saves the the settings file with the settings in their current state
        /// </summary>
        public static void SaveSettings()
        {
            try
            {
                // Serialize the settings and write them to a local file (in %appdata&)
                File.WriteAllText($"{Core.UserSettingsPath}{Core.FILENAME_SETTINGS}", JsonConvert.SerializeObject(Settings));
            }
            catch (Exception)
            {
                // Silently fail the creation of the settings, most likely due to permissions issue
                // or a missing directory.  As the application always attempts to create the paths
                // this isn't a concern as failure results in a default valid configuration anyway
            }
        }
    }
}
