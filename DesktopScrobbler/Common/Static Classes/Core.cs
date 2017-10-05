using System;
using LastFM.Common.Helpers;

namespace LastFM.Common
{
    public static class Core
    {
        public static string FILENAME_SETTINGS = "Settings.cfg";
        public static string FAILEDSCROBBLE_LIMITEXCEEDEDFILENAMEEXTENSION = ".scle";
        public static string FAILEDSCROBBLE_NOCONNECTION = ".scnc";
        public static string APPLICATION_TITLE = "Last.fm Desktop Scrobbler";

        public static string UserDownloadsPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Last.fm\\Desktop Scrobbler\\v3\\Updates\\";
        public static string UserSettingsPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Last.fm\\Desktop Scrobbler\\v3\\Settings\\";
        public static string UserCachePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Last.fm\\Desktop Scrobbler\\v3\\Cache\\";

        public const string UpdateUrl = "http://twitterinanapp.com/last-fm-updates-page/";

        public static bool ApplicationIsShuttingDown = false;

        public static Settings Settings => ApplicationConfiguration.Settings;

        public static void InitializeSettings()
        {
            PathHelper.CheckPaths(UserSettingsPath, UserCachePath, UserDownloadsPath);

            ApplicationConfiguration.Initialize();
        }

        public static void SaveSettings()
        {
            ApplicationConfiguration.SaveSettings();
        }
    }
}
