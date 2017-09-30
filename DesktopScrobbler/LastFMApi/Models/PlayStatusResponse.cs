using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    public class PlayStatusResponse
    {
        [JsonProperty("nowPlaying")]
        public Scrobble NowPlaying { get; set; }
    }
}
