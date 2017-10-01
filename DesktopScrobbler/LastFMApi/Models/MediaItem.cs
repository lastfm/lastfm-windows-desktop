using Newtonsoft.Json;
using System;

namespace LastFM.ApiClient.Models
{
    public class MediaItem
    {
        [JsonProperty("track")]
        public string TrackName { get; set; }

        [JsonProperty("duration")]
        public double TrackLength { get; set; }

        [JsonProperty("artist")]
        public string ArtistName { get; set; }

        [JsonProperty("album")]
        public string AlbumName { get; set; }

        [JsonProperty("albumArtist")]
        public string AlbumArtist { get; set; }

        public DateTime StartedPlaying { get; set; }
    }
}
