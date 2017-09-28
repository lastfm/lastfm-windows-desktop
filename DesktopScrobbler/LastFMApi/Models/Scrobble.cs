using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    public class Scrobble
    {
        [JsonProperty("artist")]
        public CorrectedStatus artist { get; set; }

        [JsonProperty("ignoredMessage")]
        public IgnoredMessage IgnoredMessage { get; set; }

        [JsonProperty("albumArtist")]
        public CorrectedStatus AlbumArtist { get; set; }

        [JsonProperty("album")]
        public CorrectedStatus Album { get; set; }

        [JsonProperty("track")]
        public CorrectedStatus Track { get; set; }

    }
}
