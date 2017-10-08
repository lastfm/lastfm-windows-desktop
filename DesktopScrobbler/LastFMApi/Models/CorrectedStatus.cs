using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    // The Last.fm API model for correction made to a scrobbled media item
    public class CorrectedStatus
    {
        [JsonProperty("corrected")]
        // A number representing where a correct was made
        // 0 = there was no correction, 1 =  there was a correction
        public string Corrected { get; set; }

        [JsonProperty("#text")]
        // The 'new' (or passed) text associated with the media item
        public string CorrectedText { get; set; }
    }
}
