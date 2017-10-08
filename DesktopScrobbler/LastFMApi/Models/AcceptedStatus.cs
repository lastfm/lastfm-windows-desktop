using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    // Model representing the Accepted and Ignored state of a Scrobble
    public class AcceptedResult
    {
        // How many scrobble items were accpeted
        [JsonProperty("accepted")]
        public int Accepted { get; set; }

        // How many scrobble items were ignored
        [JsonProperty("ignored")]
        public int Ignored { get; set; }
    }
}
