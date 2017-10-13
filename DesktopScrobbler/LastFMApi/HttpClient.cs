using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using LastFM.ApiClient.Models;

namespace LastFM.ApiClient
{
    // The base API communication client, allowing the generation of basic requests and passing responses back
    // from JSON converted models
    public class HttpClient
    {
        // The type of the request being made
        public enum HttpRequestType
        {
            Get,
            Post,
            Delete            
        }

        // Whether or not the API client should enforce HTTPS requests
        internal bool EnforceHTTPS { get; set; }

        // Any post-response processing before attempting to convert the reponse back to a JSON object
        internal Func<string, string> HttpResponsePreProcessing { get; set; }

        // The private user-agent string being passed to the API
        private const string _userAgentString = "Last.fm Desktop Scrobbler v";

        public ResponseError LastErrorResponse { get; set; }

        // A helper function to automatically determine the version number of the application using the API client
        public string GetApplicationVersionNumber()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        // A method used to validate the URL and ensure it has the correct termination, and if necessary,
        // enforce the request to be HTTPS
        private string ValidateURLPath(string urlPath)
        {
            // If HTTPS is in enforcement
            if (this.EnforceHTTPS)
            {
                // If the url starts with http and isn't a https request
                if (urlPath.Trim().ToLower().StartsWith("http") && !urlPath.Trim().ToLower().StartsWith("https"))
                {
                    // Swap the URL to enforce HTTPS
                    urlPath = urlPath.Trim().Replace("http:", "https:");
                }
            }

            // Make sure the request terminates with a trailing forwardslash
            if (!urlPath.Trim().EndsWith("/"))
            {
                urlPath = $"{urlPath.Trim()}/";
            }
            return urlPath;
        }

        // A helper function to take a list of parameters and convert them into a Url query string
        private string GetQueryString(params KeyValuePair<string, string>[] requestParameters)
        {
            // Start with an empty string
            string queryString = string.Empty;

            // If we have anything to convert
            if (requestParameters != null)
            {
                // Iterate the key value paairs
                foreach (KeyValuePair<string, string> parameterPair in requestParameters)
                {
                    // Ignore any defined as being 'the method'
                    if (!string.IsNullOrEmpty(parameterPair.Key) && parameterPair.Key.ToLower() != "method")
                    {
                        // Append the key and value in the format of (&key=value) ensuring we Url Encode the value safely
                        queryString += $"&{HttpUtility.UrlEncode(parameterPair.Key)}={HttpUtility.UrlEncode(parameterPair.Value)}";
                    }
                }
            }

            return queryString;
        }

        // Internal core method for sending a request to the API, using the specified request type and Url and returning an object of the type requested
        private async Task<T> PerformRequest<T>(HttpRequestType requestType, string requestUrl, string queryStringOrBody) where T: class
        {
            // Create a new instance of the model tpe being requested
            T instance = default(T);

            // Internal representation of the response message
            HttpResponseMessage responseMessage = null;

            // Create an empty handler.  Sometimes you might want to allow/disallow re-direction requests, which you can do here
            HttpClientHandler handler = new HttpClientHandler()
            {
            };            

            // Create a new HTTP client instance with which to send the request
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient(handler);

            //Set the user-agent string
            client.DefaultRequestHeaders.Add("User-Agent", $"{_userAgentString}{GetApplicationVersionNumber()}");

            // Determine if the 'Expect' header contains the 'continue' parameter
            client.DefaultRequestHeaders.ExpectContinue = false;

            switch (requestType)
            {
                // Execute the delete request to the Url using the relevant query string (as deletes normally don't support the body method)
                case HttpRequestType.Delete:
                {
                    responseMessage = await client.DeleteAsync($"{requestUrl}{queryStringOrBody}").ConfigureAwait(false);
                    break;
                }
                // Execute the POST request to the url passing the query string in the body
                case HttpRequestType.Post:
                {
                    responseMessage = await client.PostAsync($"{requestUrl}", new StringContent(queryStringOrBody)).ConfigureAwait(false);
                    break;
                }
                // Execute a GET request passing the query string as a query string
                default:
                {
                    responseMessage = await client.GetAsync($"{requestUrl}{queryStringOrBody}").ConfigureAwait(false);
                    break;
                }
            }
            
            // If there is a response from the Url
            if (responseMessage != null)
            {
                // Read the entire response
                var responseString = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                // If we need to pre-process the response
                if (HttpResponsePreProcessing != null)
                {
                    // Pre-process it
                    responseString = HttpResponsePreProcessing(responseString);
                }

                // If the response was in the 'success' state (or for Windows IIS, the response was found)
                if (responseMessage.IsSuccessStatusCode || responseMessage.StatusCode == HttpStatusCode.Found)
                {
                    // Try to deserialize the response as the JSON object type requested
                    instance = JsonConvert.DeserializeObject<T>(responseString);
                }
                else
                {
                    // Convert the response into an appropriate Last.fm error code
                    this.LastErrorResponse = JsonConvert.DeserializeObject<ResponseError>(responseString);

                    throw new ResponseException();
                }
            }

            // Return the object type
            return instance;
        }

        // Internal core method for sending a request to the API, using the specified request type and Url and returning an object of the type requested
        private async Task<T> PerformRequest<T>(HttpRequestType requestType, string requestUrl, string queryString, HttpContent bodyContent) where T : class
        {
            // Create a default instance of the object type that we are expecting
            T instance = default(T);

            // Create a default instance of the response message
            HttpResponseMessage responseMessage = null;

            // Create an empty handler.  Sometimes you might want to allow/disallow re-direction requests, which you can do here
            HttpClientHandler handler = new HttpClientHandler()
            {
            };

            // Create a new HTTP client instance with which to send the request
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient(handler);

            //Set the user-agent string
            client.DefaultRequestHeaders.Add("User-Agent", $"{_userAgentString}{GetApplicationVersionNumber()}");

            switch (requestType)
            {
                case HttpRequestType.Delete:
                    {
                        // Execute the delete request to the Url using the relevant query string (as deletes normally don't support the body method)
                        responseMessage = await client.DeleteAsync($"{requestUrl}{bodyContent}").ConfigureAwait(false);
                        break;
                    }
                case HttpRequestType.Post:
                    {
                        // Execute the POST request to the url passing the query string on the URL and the body content
                        responseMessage = await client.PostAsync($"{requestUrl}{queryString}", bodyContent).ConfigureAwait(false);
                        break;
                    }

                default:
                    {
                        // Execute a GET request passing the body content as the query string
                        responseMessage = await client.GetAsync($"{requestUrl}{bodyContent}").ConfigureAwait(false);
                        break;
                    }
            }

            // If there is a response from the Url
            if (responseMessage != null)
            {
                // Read the entire response
                var responseString = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                // If we need to pre-process the response
                if (HttpResponsePreProcessing != null)
                {
                    // Pre-process it
                    responseString = HttpResponsePreProcessing(responseString);
                }

                // If the response was in the 'success' state (or for Windows IIS, the response was found)
                if (responseMessage.IsSuccessStatusCode || responseMessage.StatusCode == HttpStatusCode.Found)
                {
                    List<string> conversionErrors = new List<string>();

                    // Try to deserialize the response as the JSON object type requested tracking any errors from conversion
                    // (for debugging purposes)
                    instance = JsonConvert.DeserializeObject<T>(responseString, new JsonSerializerSettings() {
                    Error = delegate (object sender, ErrorEventArgs args)
                    {
                        conversionErrors.Add(args.ErrorContext.Error.Message);
                        args.ErrorContext.Handled = true;
                    }});
                }
            }

            return instance;
        }

        // Exposed method to allow the sending of a request of the specified type, to the specified Url (request path), using the specified method name and 
        // request parameters as appropriate returning the result as an object of the specified type
        internal async Task<T> SendRequest<T>(HttpRequestType requestType, string requestPath, string method, KeyValuePair<string, string>[] requestParameters) where T : class
        {
            // Create a local instance of the object type being requested
            T instance = null;

            // Check that the Url and method have been specified
            if (!string.IsNullOrEmpty(requestPath) && !string.IsNullOrEmpty(method))
            {
                // Validate the Url
                requestPath = ValidateURLPath(requestPath);

                // Build the aprrpopriate query string
                string requestUrl = $"{requestPath}?method={method}";                
                string queryString = GetQueryString(requestParameters);

                // Perform the request and get the result in the form of the object type we need
                instance = await PerformRequest<T>(requestType, requestUrl, queryString).ConfigureAwait(false);
            }

            return instance;
        }

        // Exposed method to allow the sending of a request of the specified type, to the specified Url (request path), using the specified method name and 
        // body content as appropriate returning the result as an object of the specified type
        internal async Task<T> SendRequest<T>(HttpRequestType requestType, string requestPath, string method, HttpContent bodyContent, KeyValuePair<string, string>[] requestParameters) where T : class
        {
            // Create a local instance of the object type being requested
            T instance = null;

            // Check that the Url and method have been specified
            if (!string.IsNullOrEmpty(requestPath) && !string.IsNullOrEmpty(method))
            {
                // Validate the Url
                requestPath = ValidateURLPath(requestPath);

                // Perform the request and get the result in the form of the object type we need
                instance = await PerformRequest<T>(requestType, requestPath, null, bodyContent).ConfigureAwait(false);
            }

            return instance;
        }

    }
}
