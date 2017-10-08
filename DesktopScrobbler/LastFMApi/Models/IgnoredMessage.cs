using Newtonsoft.Json;
using static LastFM.ApiClient.Enums.ReasonCodes;

namespace LastFM.ApiClient.Models
{
    // A Last.fm model for representing why an item being scrobbled was ignored
    public class IgnoredMessage
    {
        // The reason why the item was ignored (largely un-important unless it's because the API limit was exceeded)
        [JsonProperty("code")]
        public IgnoredReason Code { get; set; }
    }
}
