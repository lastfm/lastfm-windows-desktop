
using LastFM.ApiClient.Models;
using Newtonsoft.Json;

// A Last.fm API model for a list of Scrobble results
public class ScrobbleResponse
{
    // The list of scrobble results
    [JsonProperty("scrobbles")]
    public Scrobbles Scrobbles { get; set; }
}
