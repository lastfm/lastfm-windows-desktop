using System;

namespace LastFM.Common.Classes
{
    /// <summary>
    /// Class used by the application settings to determine the enabled/disabled state of
    /// a plugin source
    /// </summary>
    public class ScrobblerSourceStatus
    {
        // A unique identifier for the plugin
        public Guid Identifier { get; set; }

        // Whether or not the plugin is enabled
        public bool IsEnabled { get; set; }
    }
}
