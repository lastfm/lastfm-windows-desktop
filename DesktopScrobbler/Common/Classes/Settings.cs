using LastFM.Common.Classes;
using System.Collections.Generic;

namespace LastFM.Common
{
    public class Settings
    {
        public bool CloseToTray { get; set; } = true;

        public bool StartMinimized { get; set; } = true;

        public string SessionToken { get; set; }

        public string Username { get; set; }

        public bool UserHasAuthorizedApp { get; set; } = false;

        public List<ScrobblerSourceStatus> ScrobblerStatus { get; set; } = new List<ScrobblerSourceStatus>();

        public bool ShowNotifications { get; set; } = true;

        public bool? ShowTrackChanges { get; set; } = false;

        public bool? ShowScrobbleNotifications { get; set; } = false;
    }
}
