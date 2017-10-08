using System;
using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    // The model for a Last.fm API authentication token
    public class AuthenticationToken
    {
        // The authentication token
        [JsonProperty("token")]
        public String Token { get; set; }
    }
}
