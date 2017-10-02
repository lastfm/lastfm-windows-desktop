using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace LastFM.ApiClient
{
    public class HttpClient
    {
        public enum HttpRequestType
        {
            Get,
            Post,
            Delete            
        }

        internal bool EnforceHTTPS { get; set; }

        //private bool _responseAsJson;

        internal Func<string, string> HttpResponsePreProcessing { get; set; }

        private const string _userAgentString = "LastFM Desktop Scrobbler v";

        public string GetApplicationVersionNumber()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private string ValidateURLPath(string urlPath)
        {
            if (EnforceHTTPS)
            {
                if (urlPath.Trim().ToLower().StartsWith("http") && !urlPath.Trim().ToLower().StartsWith("https"))
                {
                    urlPath = urlPath.Trim().Replace("http:", "https:");
                }
            }

            if (!urlPath.Trim().EndsWith("/"))
            {
                urlPath = $"{urlPath.Trim()}/";
            }
            return urlPath;
        }

        private string GetQueryString(params KeyValuePair<string, string>[] requestParameters)
        {
            string queryString = string.Empty;

            if (requestParameters != null)
            {
                foreach (KeyValuePair<string, string> parameterPair in requestParameters)
                {
                    if (!string.IsNullOrEmpty(parameterPair.Key) && parameterPair.Key.ToLower() != "method")
                    {
                        queryString += $"&{HttpUtility.UrlEncode(parameterPair.Key)}={HttpUtility.UrlEncode(parameterPair.Value)}";
                    }
                }
            }

            return queryString;
        }

        private async Task<T> PerformRequest<T>(HttpRequestType requestType, string requestUrl, string queryStringOrBody) where T: class
        {
            T instance = default(T);

            HttpResponseMessage responseMessage = null;

            HttpClientHandler handler = new HttpClientHandler()
            {
            };            

            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", $"{_userAgentString}{GetApplicationVersionNumber()}");

            client.DefaultRequestHeaders.ExpectContinue = false;

            switch (requestType)
            {
                case HttpRequestType.Delete:
                {
                    responseMessage = await client.DeleteAsync($"{requestUrl}{queryStringOrBody}");
                    break;
                }
                case HttpRequestType.Post:
                {
                    responseMessage = await client.PostAsync($"{requestUrl}", new StringContent(queryStringOrBody));
                    break;
                }
                default:
                {
                    responseMessage = await client.GetAsync($"{requestUrl}{queryStringOrBody}");
                    break;
                }
            }
            
            if (responseMessage != null)
            {
                var responseString = await responseMessage.Content.ReadAsStringAsync();

                if (HttpResponsePreProcessing != null)
                {
                    responseString = HttpResponsePreProcessing(responseString);
                }

                if (responseMessage.IsSuccessStatusCode || responseMessage.StatusCode == HttpStatusCode.Found)
                {
                    instance = JsonConvert.DeserializeObject<T>(responseString);
                }
                else if (responseMessage.StatusCode == HttpStatusCode.InternalServerError)
                {
                    
                }
            }

            return instance;
        }

        private async Task<T> PerformRequest<T>(HttpRequestType requestType, string requestUrl, string queryString, HttpContent bodyContent) where T : class
        {
            T instance = default(T);

            HttpResponseMessage responseMessage = null;

            HttpClientHandler handler = new HttpClientHandler()
            {
            };

            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", $"{_userAgentString}{GetApplicationVersionNumber()}");

            switch (requestType)
            {
                case HttpRequestType.Delete:
                    {
                        responseMessage = await client.DeleteAsync($"{requestUrl}{bodyContent}");
                        break;
                    }
                case HttpRequestType.Post:
                    {
                        responseMessage = await client.PostAsync($"{requestUrl}{queryString}", bodyContent);
                        break;
                    }

                default:
                    {
                        responseMessage = await client.GetAsync($"{requestUrl}{bodyContent}");
                        break;
                    }
            }

            if (responseMessage != null)
            {
                var responseString = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (HttpResponsePreProcessing != null)
                {
                    responseString = HttpResponsePreProcessing(responseString);
                }

                if (responseMessage.IsSuccessStatusCode || responseMessage.StatusCode == HttpStatusCode.Found)
                {
                    List<string> conversionErrors = new List<string>();

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

        internal async Task<T> SendRequest<T>(HttpRequestType requestType, string requestPath, string method, KeyValuePair<string, string>[] requestParameters) where T : class
        {
            T instance = null;

            if (!string.IsNullOrEmpty(requestPath) && !string.IsNullOrEmpty(method))
            {
                requestPath = ValidateURLPath(requestPath);

                string requestUrl = $"{requestPath}?method={method}";                
                string queryString = GetQueryString(requestParameters);

                instance = await PerformRequest<T>(requestType, requestUrl, queryString);
            }

            return instance;
        }

        internal async Task<T> SendRequest<T>(HttpRequestType requestType, string requestPath, string method, HttpContent bodyContent, KeyValuePair<string, string>[] requestParameters) where T : class
        {
            T instance = null;

            if (!string.IsNullOrEmpty(requestPath) && !string.IsNullOrEmpty(method))
            {
                requestPath = ValidateURLPath(requestPath);

                //string requestUrl = $"{requestPath}?method={method}";
                string requestUrl = $"{requestPath}";

                //string queryString = GetQueryString(requestParameters);

                instance = await PerformRequest<T>(requestType, requestUrl, null, bodyContent);
            }

            return instance;
        }

    }
}
