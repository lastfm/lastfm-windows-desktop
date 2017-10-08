using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    // A Last.fm API model representing the details of a track
    public class TrackDetail
    {
        // The Last.fm Id for the track
        [JsonProperty("id")]
        public int Id { get; set; }

        // The name of the track
        [JsonProperty("name")]
        public string Name { get; set; }

        // The Last.fm Url for the track
        [JsonProperty("url")]
        public string Url { get; set; }

        // Whether or not the user currently 'Loves' this track
        [JsonProperty("userloved")]
        public int UserLoved { get; set; }
    }
}
