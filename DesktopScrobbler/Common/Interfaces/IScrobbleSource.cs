using LastFM.ApiClient.Models;
using System;
using System.Collections.Generic;
using static LastFM.Common.Factories.ScrobbleFactory;

namespace LastFM.Common
{
    public interface IScrobbleSource: IDisposable
    {
        Guid SourceIdentifier { get; }

        string SourceDescription { get; }

        void InitializeSource(int minimumScrobbleSeconds, TrackMonitoringStarted onTrackMonitoringStarted, TrackMonitoring onTrackingMonitoring, TrackMonitoringEnded onTrackMonitoringEnded, ScrobbleTrack onScrobbleTrack);

        bool IsEnabled { get; set; }

        List<MediaItem> MediaToScrobble{ get; }

        void ClearQueuedMedia();

    }
}
