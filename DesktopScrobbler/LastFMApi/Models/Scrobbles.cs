using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastFM.ApiClient.Models
{
    public class Scrobbles
    {
        [JsonProperty("@attr")]
        public AcceptedResult AcceptedResult { get; set; }

        [JsonProperty("scrobble")]
        public Scrobble[] ScrobbleItems { get; set; }
    }
}
