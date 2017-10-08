using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LastFM.ApiClient.Models
{
    // A Last.fm API model representing the details of a user
    public class UserInfo
    {
        // The name (username) of the user
        [JsonProperty("name")]
        public string Name { get; set; }

        // Any images associated with the user
        [JsonProperty("image")]
        public List<ImageMedia> Image { get; set; }

        // The user's profile Url
        [JsonProperty("url")]
        public string Url { get; set; }

        // The country of residence for the user
        [JsonProperty("country")]
        public string Country { get; set; }

        // How old the user is
        [JsonProperty("age")]
        public int Age { get; set; }

        // The gender of the user
        [JsonProperty("gender")]
        public string Gender { get; set; }

        // Whether the user is a subscriber or not
        [JsonProperty("subscriber")]
        public uint Subscriber { get; set; }

        // How many times the user has submitted 'Now playing' requests to the API
        [JsonProperty("playcount")]
        public Int64 PlayCount { get; set; }

        // How many playlists the user has
        [JsonProperty("playlists")]
        public Int64 Playlists { get; set; }
    }
}
