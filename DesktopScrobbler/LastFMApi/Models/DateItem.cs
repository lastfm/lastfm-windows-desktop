using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastFM.ApiClient.Models
{
    public class DateItem
    {
        [JsonProperty("#text")]
        public string Text { get; set; }

        [JsonProperty("unixtime")]
        public string UnixTime { get; set; }

        public DateTime Date
        {
            get {

                var convertedDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                convertedDate = convertedDate.AddSeconds(double.Parse(this.UnixTime));

                return convertedDate;
            }
        }
    }
}
