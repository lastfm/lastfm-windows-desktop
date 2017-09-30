using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    public class AcceptedResult
    {
        [JsonProperty("accepted")]
        public int Accepted { get; set; }

        [JsonProperty("ignored")]
        public int Ignored { get; set; }
    }
}
