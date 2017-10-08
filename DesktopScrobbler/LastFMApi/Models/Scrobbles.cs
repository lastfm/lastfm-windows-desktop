using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    // A Last.fm API for a list of scrobbles, and their accepted status
    public class Scrobbles
    {
        // The overall acceptance of the scrobbles
        [JsonProperty("@attr")]
        public AcceptedResult AcceptedResult { get; set; }

        // The list of associated scrobble items
        [JsonProperty("scrobble")]
        public Scrobble[] ScrobbleItems { get; set; }
    }
}
