using System;
using LastFM.Common.Helpers;
using LastFM.Common.Localization;

namespace LastFM.Common
{
    // Core run-time properties associate with the application
    public static class Core
    {
        // Name of the application settings file
        public static string FILENAME_SETTINGS = "Settings.cfg";

        // Extension part of the files created when scrobbles are failed due to API limit exceeded
        public static string FAILEDSCROBBLE_LIMITEXCEEDEDFILENAMEEXTENSION = ".scle";

        // Extension part of the files created when the user is offline due to no connection to Last.fm
        public static string FAILEDSCROBBLE_NOCONNECTION = ".scnc";

        // The name of the application
        public static string APPLICATION_TITLE = LocalizationStrings.General_ApplicationTitle;

        // The file system path where the application will place downloaded updates
        public static string UserDownloadsPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Last.fm\\Desktop Scrobbler\\v3\\Updates\\";

        // The file system path where the application stores configuration files
        public static string UserSettingsPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Last.fm\\Desktop Scrobbler\\v3\\Settings\\";

        // The file system path where the application stores cached scrobbles
        public static string UserCachePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Last.fm\\Desktop Scrobbler\\v3\\Cache\\";

        // The Url where the application checks for an updated version
        public const string UpdateUrl = "http://twitterinanapp.com/last-fm-updates-page/";

        // The Url to the terms of use (as defined by Last.fm)
        public const string TermsUrl = "https://www.last.fm/legal/terms";

        // Setting dictating whether the application is currently in the process of shutting down
        public static bool ApplicationIsShuttingDown = false;

        // The current application settings in use
        public static Settings Settings => ApplicationConfiguration.Settings;

        // Helper function for initializing the application (Check that the application paths and settings exist)
        public static void InitializeApplication()
        {
            // Check that the necessary application paths exist
            PathHelper.CheckPaths(UserSettingsPath, UserCachePath, UserDownloadsPath);

            // Initialize the settings sub-system
            ApplicationConfiguration.Initialize();
        }

        // Helper function for saving changes to the setting file
        public static void SaveSettings()
        {
            // Tell the settings sub-sytem to save the current settings
            ApplicationConfiguration.SaveSettings();
        }
    }
}
