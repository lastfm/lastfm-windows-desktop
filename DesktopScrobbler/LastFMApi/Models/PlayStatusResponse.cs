using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastFM.ApiClient.Models
{
    public class PlayStatusResponse
    {
        [JsonProperty("nowPlaying")]
        public Scrobble NowPlaying { get; set; }
    }
}
