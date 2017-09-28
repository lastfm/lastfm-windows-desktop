using LastFM.Common.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
