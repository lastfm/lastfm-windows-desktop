using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LastFM.ApiClient.Models
{
    public class UserInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image")]
        public List<ImageMedia> Image { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("age")]
        public int Age { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("subscriber")]
        public uint Subscriber { get; set; }

        [JsonProperty("playcount")]
        public Int64 PlayCount { get; set; }

        [JsonProperty("playlists")]
        public Int64 Playlists { get; set; }

        [JsonProperty("bootstrap")]
        public uint BootStrap { get; set; }

        [JsonProperty("registered")]
        public DateItem Registered { get; set; }
    }
}
