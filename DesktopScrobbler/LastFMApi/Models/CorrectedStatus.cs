using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastFM.ApiClient.Models
{
    public class CorrectedStatus
    {
        [JsonProperty("corrected")]
        public string Corrected { get; set; }

        [JsonProperty("#text")]
        public string CorrectedText { get; set; }
    }
}
