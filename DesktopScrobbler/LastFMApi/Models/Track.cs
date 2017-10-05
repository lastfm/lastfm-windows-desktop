using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    public class Track
    {
        [JsonProperty("track")]
        public TrackDetail Info { get; set; }

    }
}
