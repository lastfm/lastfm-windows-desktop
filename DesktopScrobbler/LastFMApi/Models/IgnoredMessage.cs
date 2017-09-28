using Newtonsoft.Json;
using static LastFM.ApiClient.Enums.ReasonCodes;

namespace LastFM.ApiClient.Models
{
    public class IgnoredMessage
    {
        [JsonProperty("code")]
        public IgnoredReason Code { get; set; }

        [JsonProperty("#text")]
        public string Text { get; set; }
    }
}
