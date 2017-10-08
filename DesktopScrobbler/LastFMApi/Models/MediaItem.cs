using Newtonsoft.Json;
using System;

namespace LastFM.ApiClient.Models
{
    // A Last.fm API model for a media item
    public class MediaItem
    {
        // The name of the track
        [JsonProperty("track")]
        public string TrackName { get; set; }

        // The duration of the track (in seconds)
        [JsonProperty("duration")]
        public double TrackLength { get; set; }

        // The name of the track artist
        [JsonProperty("artist")]
        public string ArtistName { get; set; }

        // The name of the album
        [JsonProperty("album")]
        public string AlbumName { get; set; }

        // The name of the album artist
        [JsonProperty("albumArtist")]
        public string AlbumArtist { get; set; }

        // The time the track started playing (not passed to the API) but used to convert this
        // value by the appropriate API function into a Unix timestamp
        public DateTime StartedPlaying { get; set; }
    }
}
