using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LastFM.ApiClient.Models;
using LastFM.Common.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using LastFM.ApiClient.Helpers;

namespace LastFM.ApiClient
{
    public class LastFMClient : HttpClient
    {
        private string _apiApplicationName = string.Empty;
        private string _apiKey = string.Empty;
        private string _apiSecret = string.Empty;

        private string _baseUrl = string.Empty;

        private string _apiPath = "/2.0/";

        private AuthenticationToken _authToken = null;
        private SessionToken _sessionToken = null;

        public SessionToken SessionToken
        {
            get { return _sessionToken; }
            set { _sessionToken = value; }
        }

        public enum PlayStatus
        {
            StartedListening,
            StoppedListening
        }

        public enum LoveStatus
        {
            Love,
            Unlove
        }

        public string AuthenticationToken => _authToken?.Token;

        public LastFMClient(string lastFmBaseUrl, string apiKey, string apiSecret)
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;

            if (lastFmBaseUrl.Trim().EndsWith("/"))
            {
                lastFmBaseUrl = lastFmBaseUrl.Substring(0, lastFmBaseUrl.Trim().Length - 1);
            }

            _baseUrl = lastFmBaseUrl;

            ServicePointManager.ServerCertificateValidationCallback += HttpCertificateValidationHelper.ValidateServerCertificate;

            base.HttpResponsePreProcessing = (responseString) =>
            {
                var clearedString = responseString.Replace("\n", "");
                var regex = @"\<head(.+)body\>";
                return System.Text.RegularExpressions.Regex.Replace(clearedString, regex, "");
            };
        }

        public void LoggedOut()
        {
            _authToken = null;
            _sessionToken = null;
        }

        public Task<bool> GetAuthorisationToken()
        {
            return Authenticate();
        }

        private async Task<bool> Authenticate()
        {
            AuthenticationToken authToken = null;

            try
            {
                authToken = await GetAuthToken().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return !string.IsNullOrEmpty(authToken?.Token);
        }

        private async Task<AuthenticationToken> GetAuthToken()
        {
            var baseParameters = new Dictionary<string, string>();

            //baseParameters.Add(new KeyValuePair<string, string>("api_sig", GetMethodSignature("auth.gettoken", false, baseParameters)));
            AddRequiredRequestParams(baseParameters, "auth.gettoken", null, false);

            _authToken = await UnauthenticatedGet<AuthenticationToken>("auth.gettoken", baseParameters.ToArray()).ConfigureAwait(false);

            return _authToken;
        }

        public async Task<Scrobble> SendPlayStatusChanged(MediaItem mediaItem, PlayStatus newPlayStatus)
        {
            string updateMethod = string.Empty;
            var baseParameters = new Dictionary<string, string>();

            PlayStatusResponse response = null;

            if (newPlayStatus == PlayStatus.StartedListening)
            {
                updateMethod = "track.updateNowPlaying";

                baseParameters.Add("track", mediaItem.TrackName);
                baseParameters.Add("artist", mediaItem.ArtistName);

                if (!string.IsNullOrEmpty(mediaItem.AlbumName))
                {
                    baseParameters.Add("album", mediaItem.AlbumName);
                }

                if (mediaItem.TrackLength > 0)
                {
                    baseParameters.Add("duration", mediaItem.TrackLength.ToString());
                }
            }
            else if (newPlayStatus == PlayStatus.StoppedListening)
            {
                updateMethod = "track.removeNowPlaying";
            }

            if (!string.IsNullOrEmpty(updateMethod))
            {
                AddRequiredRequestParams(baseParameters, updateMethod, _sessionToken.Key);

                FormUrlEncodedContent postContent = new FormUrlEncodedContent(baseParameters);

                response = await Post<PlayStatusResponse>(updateMethod, postContent, baseParameters.ToArray()).ConfigureAwait(false);

                Console.WriteLine($"Sent Playing Status request ({updateMethod}), response:\r\n {response}");
            }

            return response?.NowPlaying;
        }

        public async Task<Track> GetLoveStatus(MediaItem mediaItem)
        {
            var baseParameters = new Dictionary<string, string>();
            string updateMethod = "track.getInfo";

            Track responseData = null;

            baseParameters.Add("artist", mediaItem.ArtistName);
            baseParameters.Add("track", mediaItem.TrackName);

            AddRequiredRequestParams(baseParameters, updateMethod, _sessionToken.Key);

            if (!string.IsNullOrEmpty(updateMethod))
            {
                FormUrlEncodedContent postContent = new FormUrlEncodedContent(baseParameters);

                responseData = await Post<Track>(updateMethod, postContent, baseParameters.ToArray()).ConfigureAwait(false);

                Console.WriteLine($"Sent Get love status request ({updateMethod}), response:\r\n {responseData}");
            }

            return responseData;
        }

        public async Task LoveTrack(LoveStatus loveStatus, MediaItem mediaItem)
        {
            var baseParameters = new Dictionary<string, string>();
            string updateMethod = string.Empty;

            baseParameters.Add("artist", mediaItem.ArtistName);
            baseParameters.Add("track", mediaItem.TrackName);

            if (loveStatus == LoveStatus.Love)
            {
                updateMethod = "track.love";
            }
            else if (loveStatus == LoveStatus.Unlove)
            {
                updateMethod = "track.unlove";
            }

            AddRequiredRequestParams(baseParameters, updateMethod, _sessionToken.Key);

            if (!string.IsNullOrEmpty(updateMethod))
            {
                FormUrlEncodedContent postContent = new FormUrlEncodedContent(baseParameters);

                var rawResponse = await Post<JObject>(updateMethod, postContent, baseParameters.ToArray()).ConfigureAwait(false);

                Console.WriteLine($"Sent Love/Unlove track request ({updateMethod}), response:\r\n {rawResponse}");
            }
        }

        public async Task<ScrobbleResponse> SendScrobbles(List<MediaItem> mediaItems)
        {
            ScrobbleResponse response = null;

            var baseParameters = new Dictionary<string, string>();
            int mediaItemCount = 0;

            foreach (MediaItem mediaItem in mediaItems)
            {
                baseParameters.Add($"artist[{mediaItemCount}]", mediaItem.ArtistName);
                baseParameters.Add($"album[{mediaItemCount}]", mediaItem.AlbumName);

                if (!string.IsNullOrEmpty(mediaItem.AlbumArtist))
                {
                    baseParameters.Add($"albumArtist[{mediaItemCount}]", mediaItem.AlbumArtist);
                }

                baseParameters.Add($"track[{mediaItemCount}]", mediaItem.TrackName);
                baseParameters.Add($"duration[{mediaItemCount}]", mediaItem.TrackLength.ToString());
                baseParameters.Add($"timestamp[{mediaItemCount}]", UnixTimeStampHelper.GetUnixTimeStampFromDateTime(mediaItem.StartedPlaying).ToString("#0"));
                //baseParameters.Add($"timestamp[{mediaItemCount}]", mediaItem.UnixTimeStamp);

                ++mediaItemCount;
            }

            AddRequiredRequestParams(baseParameters, "track.scrobble", _sessionToken.Key);

            FormUrlEncodedContent postContent = new FormUrlEncodedContent(baseParameters);

            var rawResponse = await Post<JObject>("track.scrobble", postContent, baseParameters.ToArray()).ConfigureAwait(false);

            Console.WriteLine($"Sent Scrobble request, response:\r\n {rawResponse}");

            return GetScrobbleResponseFromScrobble(rawResponse);
        }

        private ScrobbleResponse GetScrobbleResponseFromScrobble(JObject scrobbleResponse)
        {
            // De-serializing the scrobble response seems to be hit and miss if you follow 'normal convention'.
            // JsonConvert.Deserialize fails to de-serialize the scrobble response with a 'Path scrobbles.scrobble.artist' error.

            // However, splitting out the JSON string from the root node, and converting the objects into the relevant types
            // seems to solve the problem nicely.  From these response we can then build up the overall scrobble response, setting
            // the relevant flags.  

            ScrobbleResponse response = new ScrobbleResponse();
            List<Scrobble> scrobbleResults = new List<Scrobble>();

            if (scrobbleResponse["scrobbles"]["scrobble"] is JArray)
            {
                // Convert the scrobble responses into an array
                Scrobble[] scrobbles = JsonConvert.DeserializeObject<Scrobble[]>(scrobbleResponse["scrobbles"]["scrobble"].ToString());
                if (scrobbles != null)
                {
                    scrobbleResults.AddRange(scrobbles.ToList());
                }
            }
            else
            {
                Scrobble scrobble = JsonConvert.DeserializeObject<Scrobble>(scrobbleResponse["scrobbles"]["scrobble"].ToString());
                if (scrobble != null)
                {
                    scrobbleResults.Add(scrobble);
                }
            }

            // Parse the results, and set the relevant properties
            int ignored = scrobbleResults.Count(item => item.IgnoredMessage.Code != Enums.ReasonCodes.IgnoredReason.AllOk);
            int accepted = scrobbleResults.Count(item => item.IgnoredMessage.Code == Enums.ReasonCodes.IgnoredReason.AllOk);

            response.Scrobbles = new Scrobbles()
            {
                AcceptedResult = new Models.AcceptedResult()
                {
                    Ignored = ignored,
                    Accepted = accepted
                },
                ScrobbleItems = scrobbleResults.ToArray()
            };

            return response;
        }

        public async Task<SessionToken> GetSessionToken()
        {
            var baseParameters = new Dictionary<string, string>();

            baseParameters.Add("token", _authToken.Token);
            AddRequiredRequestParams(baseParameters, "auth.getSession", null, true);

            try
            {
                var userSession = await UnauthenticatedGet<Session>("auth.getSession", baseParameters.ToArray()).ConfigureAwait(false);
                _sessionToken = userSession?.SessionToken;
            }
            catch (Exception e)
            {
                _sessionToken = null;
                Console.WriteLine(e);
            }

            return _sessionToken;
        }

        /// <summary>
        /// Retrieves details from LastFM of the specified user
        /// </summary>
        /// <param name="currentUser">Username of the user to retrieve details for</param>
        /// <returns></returns>
        public async Task<UserInfo> GetUserInfo(string currentUser)
        {
            var baseParameters = new Dictionary<string, string>();

            baseParameters.Add("user", currentUser);
            AddRequiredRequestParams(baseParameters, "user.getinfo", _sessionToken.Key, false);
            
            User currentUserInfo = await UnauthenticatedGet<User>("user.getinfo", baseParameters.ToArray()).ConfigureAwait(false);

            return currentUserInfo?.UserDetail;
        }

        public void AddRequiredRequestParams(Dictionary<string, string> requestParameters, string methodName, string sessionKey, bool requiresSignature = true)
        {
            // method
            requestParameters.Add("method", methodName);

            // api key
            requestParameters.Add("api_key", _apiKey);

            // session key
            if (!string.IsNullOrEmpty(sessionKey))
            {
                requestParameters.Add("sk", sessionKey);
            }

            // api_sig
            if (requiresSignature)
            {
                requestParameters.Add("api_sig", GetMethodSignature(requestParameters));
            }

            requestParameters.Add("format", "json");
        }

        public async Task GetSessionInfo()
        {
            // Make a call to auth.getSessionInfo, and check the response
                
        }

        public string GetMethodSignature(Dictionary<string, string> methodParameters = null)
        {
            var builder = new StringBuilder();

            foreach (var kv in methodParameters.OrderBy(kv => kv.Key, StringComparer.Ordinal))
            {
                builder.Append(kv.Key);
                builder.Append(kv.Value);
            }

            builder.Append(_apiSecret);

            var hashedSignature = MD5.GetHashString(builder.ToString());

            return hashedSignature;
        }

        #region HTTP Functions

        public async Task<T> UnauthenticatedGet<T>(string method, params KeyValuePair<string, string>[] parameters) where T : class
        {
            return await base.SendRequest<T>(HttpRequestType.Get, $"{_baseUrl}{_apiPath}", method, parameters).ConfigureAwait(false);
        }

        public async Task<T> Post<T>(string method, HttpContent bodyContent, params KeyValuePair<string, string>[] parameters) where T : class
        {
            T instance = null;
           
            if (!string.IsNullOrEmpty(_sessionToken?.Key))
            {
                instance = await base.SendRequest<T>(HttpRequestType.Post, $"{_baseUrl}{_apiPath}", method, bodyContent, parameters).ConfigureAwait(false);
            }

            return instance;
        }

 
        #endregion

    }
}
