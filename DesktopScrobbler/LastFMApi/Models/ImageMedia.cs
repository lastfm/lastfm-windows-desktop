using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    public class ImageMedia
    {
        [JsonProperty("#text")]
        public string Text { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }
    }
}
