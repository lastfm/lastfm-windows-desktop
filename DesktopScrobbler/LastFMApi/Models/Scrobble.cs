using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    // A Last.fm API model for an individual scrobble result
    public class Scrobble
    {
        // The corrected (or not) details associated with the artist
        [JsonProperty("artist")]
        public CorrectedStatus Artist { get; set; }

        // The corrected (or not) status of the scrobble
        [JsonProperty("ignoredMessage")]
        public IgnoredMessage IgnoredMessage { get; set; }

        // The corrected (or not) details associated with the album artist
        [JsonProperty("albumArtist")]
        public CorrectedStatus AlbumArtist { get; set; }

        // The corrected (or not) details associated with the album name
        [JsonProperty("album")]
        public CorrectedStatus Album { get; set; }

        // The corrected (or not) details associated with the track name
        [JsonProperty("track")]
        public CorrectedStatus Track { get; set; }

    }
}
