using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastFM.ApiClient.Models
{
    public class AcceptedResult
    {
        [JsonProperty("accepted")]
        public int Accepted { get; set; }

        [JsonProperty("ignored")]
        public int Ignored { get; set; }
    }
}
