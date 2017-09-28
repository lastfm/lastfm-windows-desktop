using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastFM.ApiClient.Models
{
    public class MediaItem
    {
        [JsonProperty("track")]
        public string TrackName { get; set; }

        [JsonProperty("duration")]
        public double TrackLength { get; set; }

        [JsonProperty("artist")]
        public string ArtistName { get; set; }

        [JsonProperty("album")]
        public string AlbumName { get; set; }

        public DateTime StartedPlaying { get; set; }
    }
}
