using LastFM.ApiClient.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastFM.ApiClient.Models
{
    public class UserInfo
    {

        //{"user":{"name":"DJ_VorTechS","image":[{"#text":"","size":"small"},{"#text":"","size":"medium"},{"#text":"","size":"large"},{"#text":"","size":"extralarge"}],"url":"https://www.last.fm/user/DJ_VorTechS","country":"","age":"0","gender":"n","subscriber":"0","playcount":"103","playlists":"0","bootstrap":"0","registered":{"#text":1152274443,"unixtime":"1152274443"},"type":"user"}}

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
