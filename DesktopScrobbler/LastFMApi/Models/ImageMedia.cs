using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    // Last.fm API model for media in the form of images
    public class ImageMedia
    {
        // The Url or data associated with the image
        [JsonProperty("#text")]
        public string Text { get; set; }

        // The size representation (small, medium or large)
        [JsonProperty("size")]
        public string Size { get; set; }
    }
}
