using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    public class CorrectedStatus
    {
        [JsonProperty("corrected")]
        public string Corrected { get; set; }

        [JsonProperty("#text")]
        public string CorrectedText { get; set; }
    }
}
