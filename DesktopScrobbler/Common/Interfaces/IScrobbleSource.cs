using LastFM.ApiClient.Models;
using System;
using System.Collections.Generic;
using static LastFM.Common.Factories.ScrobbleFactory;

namespace LastFM.Common
{
    // The plugin (dependency injection) interface (definition) for Scrobble plugins
    public interface IScrobbleSource: IDisposable
    {
        // A unique identifier for the plugin (not validated)
        Guid SourceIdentifier { get; }

        // A human-readable description of the plugin (displayed to the user)
        string SourceDescription { get; }

        // The event called when an instance of the plugin is created (by the Scrobble Factory), which receives delegated for various scrobbling states 
        // and the minimum number of seconds a media item is track before, before being considered 'valid'
        void InitializeSource(int minimumScrobbleSeconds, TrackMonitoringStarted onTrackMonitoringStarted, TrackMonitoring onTrackingMonitoring, TrackMonitoringEnded onTrackMonitoringEnded, ScrobbleTrack onScrobbleTrack);

        // A property to determine if a plugin is enabled or not (two-way, as the plugin can be disabled separate from the scrobble factory)
        bool IsEnabled { get; set; }

        // The queue of media items that are waiting to be scrobbled
        List<MediaItem> MediaToScrobble{ get; }

        // A method used (called by the Scrobble factory) for safely clearing the queued media items
        void ClearQueuedMedia();

    }
}
