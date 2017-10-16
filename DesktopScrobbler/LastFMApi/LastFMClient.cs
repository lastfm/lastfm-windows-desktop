using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LastFM.ApiClient.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using LastFM.ApiClient.Helpers;
using LastFMApiClient.Helpers;

namespace LastFM.ApiClient
{
    // A dedicated API client supporting specific requests used by the Desktop Scrobbler
    public class LastFMClient : HttpClient
    {
        // Default constructor, modified so that we can support request / response logging
        public LastFMClient(string logFilePath): base(logFilePath)
        {
            _logfilePathAndName = logFilePath;
        }

        // The application name used on the API website
        private string _apiApplicationName = string.Empty;

        // The API key given by the API website
        private string _apiKey = string.Empty;

        // The API secret given by the API website
        private string _apiSecret = string.Empty;

        // The base URL for the requests
        private string _baseUrl = string.Empty;

        // The API version path
        private string _apiPath = "/2.0/";

        private string _logfilePathAndName = string.Empty;

        // The current authentication token
        private AuthenticationToken _authToken = null;

        // The current session token
        private SessionToken _sessionToken = null;

        // The publicly exposed Session token 
        public SessionToken SessionToken
        {
            get { return _sessionToken; }
            set { _sessionToken = value; }
        }

        // Unique human-readable representation of the current monitoring status
        public enum MonitoringStatus
        {
            StartedMonitoring,
            StoppedMonitoring
        }

        // Unique human-readable representation of the Love Status
        public enum LoveStatus
        {
            Love,
            Unlove
        }

        // The publicly exposts authentication token
        public string AuthenticationToken => _authToken?.Token;

        // The constructor for this API client
        public LastFMClient(string lastFmBaseUrl, string apiKey, string apiSecret, string logPathAndFilename) : base(logPathAndFilename)
        {
            // Store the api details
            _apiKey = apiKey;
            _apiSecret = apiSecret;

            // Ensure the base Url is correctly terminated
            if (lastFmBaseUrl.Trim().EndsWith("/"))
            {
                lastFmBaseUrl = lastFmBaseUrl.Substring(0, lastFmBaseUrl.Trim().Length - 1);
            }

            // Set the base Url
            _baseUrl = lastFmBaseUrl;

            // Set an application wide certificate validation handler
            ServicePointManager.ServerCertificateValidationCallback += HttpCertificateValidationHelper.ValidateServerCertificate;

            // Set up a response processor to strip out any responses that might contain Html
            base.HttpResponsePreProcessing = (responseString) =>
            {
                var clearedString = responseString.Replace("\n", "");
                var regex = @"\<head(.+)body\>";
                return System.Text.RegularExpressions.Regex.Replace(clearedString, regex, "");
            };
        }

        // Public method for clearing the current tokens when the user is logged out
        public void LoggedOut()
        {
            _authToken = null;
            _sessionToken = null;
        }

        // Public method for retrieving an authentication token
        public Task<bool> GetAuthenticationToken()
        {
            return Authenticate();
        }

        // Private method for retrieving an authentication token
        private async Task<bool> Authenticate()
        {
            try
            {
                // Setup a base list of parameters
                var baseParameters = new Dictionary<string, string>();

                // Add the parameters always required by the API
                AddRequiredRequestParams(baseParameters, "auth.gettoken", null, false);

                // Retrieve the Authentication token without using authentication
                _authToken = await UnauthenticatedGet<AuthenticationToken>("auth.gettoken", baseParameters.ToArray()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.FileLogger.Write(_logfilePathAndName, "API Client Request", $"Failed to authenticate due to an error: {ex.Message}");
                Console.WriteLine(ex);
            }

            return !string.IsNullOrEmpty(_authToken?.Token);
        }

        // Public method for sending changes to the monitoring status
        public async Task<Scrobble> SendMonitoringStatusChanged(MediaItem mediaItem, MonitoringStatus newMonitoringStatus)
        {
            string updateMethod = string.Empty;

            // Create an empty list of parameters
            var baseParameters = new Dictionary<string, string>();

            PlayStatusResponse response = null;

            // If monitoring has started
            if (newMonitoringStatus == MonitoringStatus.StartedMonitoring)
            {
                // Use the updateNowPlaying API method
                updateMethod = "track.updateNowPlaying";

                // Add details of the track to the parameter list
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
            // Or the monitoring status is stopped
            else if (newMonitoringStatus == MonitoringStatus.StoppedMonitoring)
            {
                // Use the removeNowPlaying API method
                updateMethod = "track.removeNowPlaying";
            }

            // If a method has been specified (ie. someone hasn't tried to send a monitoring state > 1
            if (!string.IsNullOrEmpty(updateMethod))
            {
                // Add the required parameters (including the session token etc)
                AddRequiredRequestParams(baseParameters, updateMethod, _sessionToken.Key);

                // Convert the paremeters into body content
                FormUrlEncodedContent postContent = new FormUrlEncodedContent(baseParameters);

                // Send the request and get the response
                response = await Post<PlayStatusResponse>(updateMethod, postContent, baseParameters.ToArray()).ConfigureAwait(false);

#if DebugAPICalls
                Console.WriteLine($"Sent Playing Status request ({updateMethod}), response:\r\n {response}");
#endif
            }

            return response?.NowPlaying;
        }

        // Public method for getting the 'Love track' status for the specified media item
        public async Task<Track> GetLoveStatus(MediaItem mediaItem)
        {
            // Create an empty parameter base
            var baseParameters = new Dictionary<string, string>();

            // Use the getInfo API method
            string updateMethod = "track.getInfo";

            Track responseData = null;

            // Add basic details of the track to the API parameters
            baseParameters.Add("artist", mediaItem.ArtistName);
            baseParameters.Add("track", mediaItem.TrackName);

            // Add the relevant authorisation required parameters
            AddRequiredRequestParams(baseParameters, updateMethod, _sessionToken.Key);

            if (!string.IsNullOrEmpty(updateMethod))
            {
                // Convert the parameters to body content
                FormUrlEncodedContent postContent = new FormUrlEncodedContent(baseParameters);

                // Send the request and get the result
                responseData = await Post<Track>(updateMethod, postContent, baseParameters.ToArray()).ConfigureAwait(false);

#if DebugAPICalls
                Console.WriteLine($"Sent Get love status request ({updateMethod}), response:\r\n {responseData}");
#endif
            }

            return responseData;
        }

        // Public method for changing the 'Love' status for the specified media item
        public async Task LoveTrack(LoveStatus loveStatus, MediaItem mediaItem)
        {
            // Create an empty parameter base
            var baseParameters = new Dictionary<string, string>();
            string updateMethod = string.Empty;

            // Add basic details of the track to the parameters
            baseParameters.Add("artist", mediaItem.ArtistName);
            baseParameters.Add("track", mediaItem.TrackName);

            // If the user just 'Loved' the track
            if (loveStatus == LoveStatus.Love)
            {
                // Use the API method 'love'
                updateMethod = "track.love";
            }
            // Or if the user just 'unloved' the track
            else if (loveStatus == LoveStatus.Unlove)
            {
                // Use the API method 'unlove;
                updateMethod = "track.unlove";
            }

            // Add the required authentication parameters
            AddRequiredRequestParams(baseParameters, updateMethod, _sessionToken.Key);

            // If a method has been specified (ie the caller hasn't tried to specify a value > 1 
            if (!string.IsNullOrEmpty(updateMethod))
            {
                // Convert the parameters to body content
                FormUrlEncodedContent postContent = new FormUrlEncodedContent(baseParameters);

                // Post the request and get the response
                var rawResponse = await Post<JObject>(updateMethod, postContent, baseParameters.ToArray()).ConfigureAwait(false);

#if DebugAPICalls
                Console.WriteLine($"Sent Love/Unlove track request ({updateMethod}), response:\r\n {rawResponse}");
#endif
            }
        }

        // Public method for sending the specified media as Scrobbles to the API
        public async Task<ScrobbleResponse> SendScrobbles(List<MediaItem> mediaItems)
        {
            ScrobbleResponse response = null;

            // Create an empty parameter base
            var baseParameters = new Dictionary<string, string>();
            int mediaItemCount = 0;

            // Iterate each of the media items
            foreach (MediaItem mediaItem in mediaItems)
            {
                // Adds the basic details of the media as an array
                baseParameters.Add($"artist[{mediaItemCount}]", mediaItem.ArtistName);
                baseParameters.Add($"album[{mediaItemCount}]", mediaItem.AlbumName);

                if (!string.IsNullOrEmpty(mediaItem.AlbumArtist))
                {
                    baseParameters.Add($"albumArtist[{mediaItemCount}]", mediaItem.AlbumArtist);
                }

                baseParameters.Add($"track[{mediaItemCount}]", mediaItem.TrackName);
                baseParameters.Add($"duration[{mediaItemCount}]", mediaItem.TrackLength.ToString());
                baseParameters.Add($"timestamp[{mediaItemCount}]", UnixTimeStampHelper.GetUnixTimeStampFromDateTime(mediaItem.StartedPlaying).ToString("#0"));

                ++mediaItemCount;
            }

            // Add the required authentication parameters
            AddRequiredRequestParams(baseParameters, "track.scrobble", _sessionToken.Key);

            // Convert the parameters into body content
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(baseParameters);

            // Post the scrobbles and get the result
            var rawResponse = await Post<JObject>("track.scrobble", postContent, baseParameters.ToArray()).ConfigureAwait(false);

#if DebugAPICalls
            Console.WriteLine($"Sent Scrobble request, response:\r\n {rawResponse}");
#endif
            // Use a dedicate method to get the scrobble response because the API does NOT correctly return the response
            // in a correct JSON formatted manner
            return GetScrobbleResponseFromScrobble(rawResponse);
        }

        // Private method for converting an incorrectly formatted scrobble response into a correctly formatted response from this client
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

        // Public method for getting a Session Token from the API
        public async Task<SessionToken> GetSessionToken()
        {
            // Create an empty parameter base
            var baseParameters = new Dictionary<string, string>();

            // Add the current authentication token
            baseParameters.Add("token", _authToken.Token);

            // Add any required parameters
            AddRequiredRequestParams(baseParameters, "auth.getSession", null, true);

            try
            {
                // Post the request using unauthenticated methods and get the response
                var userSession = await UnauthenticatedGet<Session>("auth.getSession", baseParameters.ToArray()).ConfigureAwait(false);

                // Persist the current session token
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
            // Create an empty parameter base
            var baseParameters = new Dictionary<string, string>();

            // Add the username of the current user
            baseParameters.Add("user", currentUser);

            // Add any required parameters
            AddRequiredRequestParams(baseParameters, "user.getinfo", _sessionToken.Key, false);
            
            // Post the request and retrieve the user details
            User currentUserInfo = await UnauthenticatedGet<User>("user.getinfo", baseParameters.ToArray()).ConfigureAwait(false);

            return currentUserInfo?.UserDetail;
        }

        // Private method used to add any parameters required by the Last.fm API
        private void AddRequiredRequestParams(Dictionary<string, string> requestParameters, string methodName, string sessionKey, bool requiresSignature = true)
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

            // api_sig if one is required by the method being called
            if (requiresSignature)
            {
                requestParameters.Add("api_sig", GetMethodSignature(requestParameters));
            }

            // Force responses to be in JSON
            requestParameters.Add("format", "json");
        }

        // Private method for building the signature required by the Last.fm API to validate a request
        private string GetMethodSignature(Dictionary<string, string> methodParameters = null)
        {
            var builder = new StringBuilder();

            // Iterate all the parameters to be sent to the request
            foreach (var kv in methodParameters.OrderBy(kv => kv.Key, StringComparer.Ordinal))
            {
                // Append the key and value (with no separator)
                builder.Append(kv.Key);
                builder.Append(kv.Value);
            }

            // Add the API secret (required)
            builder.Append(_apiSecret);

            // Get the string, and MD5 hash it
            var hashedSignature = MD5.GetHashString(builder.ToString());

            return hashedSignature;
        }

        // Private methods for simplifying access to the API client request functions
        #region HTTP Functions

        private async Task<T> UnauthenticatedGet<T>(string method, params KeyValuePair<string, string>[] parameters) where T : class
        {
            return await base.SendRequest<T>(HttpRequestType.Get, $"{_baseUrl}{_apiPath}", method, parameters).ConfigureAwait(false);
        }

        private async Task<T> Post<T>(string method, HttpContent bodyContent, params KeyValuePair<string, string>[] parameters) where T : class
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
