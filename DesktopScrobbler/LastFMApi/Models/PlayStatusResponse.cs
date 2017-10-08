using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    // A Last.fm scrobble response for when the API is notified that a track started playing
    public class PlayStatusResponse
    {
        // The response returned as a scrobble (despite it not really being one)
        [JsonProperty("nowPlaying")]
        public Scrobble NowPlaying { get; set; }
    }
}
