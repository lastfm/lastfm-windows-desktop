
using LastFM.ApiClient.Models;
using Newtonsoft.Json;

public class ScrobbleResponse
{
    [JsonProperty("scrobbles")]
    public Scrobbles Scrobbles { get; set; }
}
