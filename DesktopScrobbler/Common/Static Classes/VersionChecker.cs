using LastFM.Common.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace LastFM.Common.Static_Classes
{
    // Helper class for checking whether a new version of the application is available
    public static class VersionChecker
    {
        // State machine for the result of a version check
        public class VersionState
        {
            // Property for representing if the remote version is newer
            public bool IsNewVersion { get; set; }

            // The version number of the remote version
            public string Version { get; set; }

            // The download Url for the remote version
            public string Url { get; set; }
        }

        // Asynchronous method for checking the specified Url for any new version numbers
        public static async Task<VersionState> CheckVersion(string versionURL)
        {
            // Load the Url, and read the response as an array of bytes
            byte[] versionPageBytes = await LoadPage(versionURL).ConfigureAwait(false);

            // Convert the bytes into UTF-8 formatted text
            string versionPageSource = Encoding.UTF8.GetString(versionPageBytes);

            // Scrape the response string, and get an appropriate version state from it
            VersionState versionFromSource = await GetVersionFromSource(versionPageSource).ConfigureAwait(false);

            return versionFromSource;
        }

        // Load the specified Url (synchronously) and return the response as a byte array
        private static async Task<byte[]> LoadPage(string versionURL)
        {
            byte[] numArray = await LoadPageAsync(versionURL).ConfigureAwait(false);
            return numArray;
        }

        // Load the specified Url asynchronously and return the response as a byte array
        private static async Task<byte[]> LoadPageAsync(string url)
        {
            // Create an internal memory stream to write to
            MemoryStream content = new MemoryStream();
            try
            {
                // Create a webrequest to the specified Url
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);

                // Read the response from the Url
                WebResponse response = webReq.GetResponse();
                try
                {
                    // Get the response and read it into the memory stream
                    Stream responseStream = response.GetResponseStream();
                    try
                    {
                        responseStream.CopyTo((Stream)content);
                    }
                    finally
                    {
                        if (responseStream != null)
                        {
                            responseStream.Dispose();
                        }
                    }
                }
                finally
                {
                    if (response != null)
                    {
                        response.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            // Return the memory stream as a byte array
            return content.ToArray();
        }

        // Helper function to remove all cached downloads to keep the file system tidy and clutter-free
        internal static void CleanUpDownloads()
        {
            // If there is a downloads folder
            if (Directory.Exists(Core.UserDownloadsPath))
            {
                try
                {
                    // Delete it
                    Directory.Delete(Core.UserDownloadsPath, true);
                }
                catch (Exception ex)
                {
                    // If the deletion faile, there's not a lot that we can do about it
                }
            }
        }

        // Helper function to read the response from a Url and return a version state machine
        internal static async Task<VersionState> GetVersionFromSource(string pageSource)
        {
            // Create an instance of an HtmlAgilityPack Html document that we can load the Url response into
            HtmlDocument updateDoc = new HtmlDocument();

            // Create a default state machine of there being no new update
            VersionState state = new VersionState() { IsNewVersion = false, Url = String.Empty, Version = string.Empty };

            //Load the page source into the HtmlAgilityPack document so that we can parse the DOM effectively
            updateDoc.LoadHtml(pageSource);

            // Find any 'H3' elements in the document
            var pageH3s = updateDoc.DocumentNode.Descendants("h3");

            if (pageH3s != null)
            {
                // The first one that has a 'Zip' file as its source, is our upgrade file
                var firstZipLink = pageH3s.Select(h3 => h3.Descendants("a").FirstOrDefault(a => a.Attributes["href"] != null && a.Attributes["href"].Value != null && a.Attributes["href"].Value.ToLower().Contains(".zip"))).FirstOrDefault(item => item != null);

                if (firstZipLink != null)
                {
                    List<string> linkText = firstZipLink.InnerText.Split(' ').ToList();
                    if (linkText.Count >= 3)
                    {
                        // Use the link text to get the version number
                        string webVersionInfo = linkText[1];

                        // Get the current version number (note the process DOESN'T take into account revisions...)
                        string appVersionInfo = ApplicationUtility.BuildVersion();

                        // Create an appropriate state machine for this versions
                        ApplicationUtility.VersionState currentState = ApplicationUtility.CompareVersions(appVersionInfo, webVersionInfo);

                        // Alter the state machine based on whether the remote version is newer
                        if (currentState == ApplicationUtility.VersionState.WebIsNewer)
                        {
                            state.IsNewVersion = true;
                            state.Version = webVersionInfo;
                            state.Url = firstZipLink.Attributes["href"].Value;
                        }
                    }
                }
            }

            // Clear up and close the HtmlAgilityPack document
            updateDoc = null;

            return state;
        }

        // Helper function for downloading an update from the given version state machine
        internal static async Task DownloadUpdate(VersionChecker.VersionState downloadInfo, string downloadDirectory, DownloadProgressChangedEventHandler progressEventHandler, AsyncCompletedEventHandler downloadCompleted)
        {
            // Convert the remote download Url into a filename for the local file system
            string downloadOSFilename = new Uri(downloadInfo.Url).PathAndQuery.Replace('/', Path.DirectorySeparatorChar);
            FileInfo downloadFileInfo = new FileInfo(downloadOSFilename);

            // Creat a client for downloading the file
            using (WebClient client = new WebClient())
            {
                // Set a delegate to the passed in instance for when there is an update to the download progress
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(progressEventHandler);

                // Set a delegate to the passed in instance for when the download has completed
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadCompleted);

                // Download the specified remote, saving it as the specified filesystem name
                client.DownloadFileAsync(new Uri(downloadInfo.Url), $"{downloadDirectory}{downloadFileInfo.Name}", downloadInfo);
            }
        }
    }
}
