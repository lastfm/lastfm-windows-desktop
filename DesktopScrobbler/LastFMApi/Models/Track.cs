using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    // A Last.fm API model for a track
    public class Track
    {
        // The track details
        [JsonProperty("track")]
        public TrackDetail Info { get; set; }

    }
}
