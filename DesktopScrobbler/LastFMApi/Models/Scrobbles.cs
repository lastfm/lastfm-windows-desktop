using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    public class Scrobbles
    {
        [JsonProperty("@attr")]
        public AcceptedResult AcceptedResult { get; set; }

        [JsonProperty("scrobble")]
        public Scrobble[] ScrobbleItems { get; set; }
    }
}
