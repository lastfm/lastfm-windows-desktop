using LastFM.ApiClient.Enums;
using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    public class ResponseError
    {
        [JsonProperty("error")]
        public ReasonCodes.ErrorCode Error { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
