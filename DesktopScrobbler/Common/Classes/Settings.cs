using LastFM.Common.Classes;
using System.Collections.Generic;

namespace LastFM.Common
{
    /// <summary>
    /// Class represeting the various application settings
    /// </summary>
    public class Settings
    {
        // Whether or not we should close the main Ui to tray (and hide it from the task bar)
        // (On by default)
        public bool CloseToTray { get; set; } = true;

        // Whether or not the application should minimize the main Ui when the application starts
        // (On by default)
        public bool StartMinimized { get; set; } = true;

        // The latest session token retrieved from the Last.fm API
        public string SessionToken { get; set; }

        // The last username of the last authenticated user that signed in to the Last.fm API via this application
        public string Username { get; set; }

        // Whether or not the user has successfully authenticated the application with the Last.fm API
        public bool UserHasAuthorizedApp { get; set; } = false;

        // A list of plugins with their appropriate 'IsEnabled' status used to determine which plugins
        // we are allowed to monitor with
        public List<ScrobblerSourceStatus> ScrobblerStatus { get; set; } = new List<ScrobblerSourceStatus>();

        // Whether or not the application should show popup notifications
        // On by default)
        public bool ShowNotifications { get; set; } = true;

        // Whether or not the application should show a popup notification for the track changed status
        // On by default, and NOT accessible from the current Ui, but still in use if you manually change the settings file)
        public bool? ShowTrackChanges { get; set; } = true;

        // Whether or not the application should show a popup notification for when a scrobble is sent to the Last.fm API
        // Off by default, and NOT accessible from the current Ui, but still in use if you manually change the settings file)
        public bool? ShowScrobbleNotifications { get; set; } = false;
    }
}
