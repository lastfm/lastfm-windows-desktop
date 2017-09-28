using System;
using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    class AuthenticationToken
    {
        [JsonProperty("token")]
        public String Token { get; set; }
    }
}
