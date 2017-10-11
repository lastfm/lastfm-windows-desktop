using LastFM.ApiClient;
using LastFM.ApiClient.Models;
using LastFM.Common.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using LastFM.Common.Localization;
using Newtonsoft.Json;

namespace LastFM.Common.Factories
{
    /// <summary>
    /// The engine room of Scrobbling.  The interaction between the API and the User Interface
    /// </summary>
    public static class ScrobbleFactory
    {
        // The representation of whether or not we have a succesful connection to the Ui
        public enum OnlineState
        {
            Online,
            Offline
        }

        // An instance of the API client
        private static LastFMClient _lastFMClient = null;

        // Whether or not Scrobbling is currently enabled
        private static bool _scrobblingActive = false;

        // Whether or not the factory has been initialized
        private static bool _isInitialized = false;

        // A hook to the base User Interface
        private static NotificationThread _uiThread = null;

        // Method definition for notifying the factory a plugin has started monitoring a media item
        public delegate void TrackMonitoringStarted(MediaItem mediaItem, bool wasResumed);

        // Method definition for notifying the factory a plugin has continued to monitor a media item
        public delegate void TrackMonitoring(MediaItem mediaItem, int playerPosition);

        // Method definition for notifying the factory a plugin has stopped monitoring a media item
        public delegate void TrackMonitoringEnded(MediaItem mediaItem);

        // Method definition for notifying the factory a plugin has requested to scrobble a track
        public delegate void ScrobbleTrack(MediaItem mediaItem);

        // Method definition for notifying the User interface when the user has gone offline
        // (usually due to no connection being available)
        public delegate void OnlineStatusUpdate(OnlineState currentState, UserInfo latestUserInfo);

        // Implementation of the method for notifying the User interface when the user has gone offline
        public static OnlineStatusUpdate OnlineStatusUpdated { get; set; }

        // Configuration for the minimum number of seconds a track must have been monitored for
        // before being 'scrobbalable'
        public static int MinimumScrobbleSeconds { get; set;} = 30;

        // The plugins the Scrobbler has available to use for monitoring media
        public static List<IScrobbleSource> ScrobblePlugins { get; set; } = new List<IScrobbleSource>();

        // Property defining whether or not Scrobbling is currently enabled
        public static bool ScrobblingEnabled
        {
            get
            {
                return _scrobblingActive;
            }

            set
            {
                _scrobblingActive = value;

                // If the Scrobbler has already been initialized (user is re-instating scrobbling), and is active
                // then tell each of the enabled plugins to re-start.
                if(_scrobblingActive && _isInitialized)
                {
                    foreach (IScrobbleSource scrobbler in ScrobblePlugins)
                    {
                        if (Convert.ToBoolean(Core.Settings.ScrobblerStatus.FirstOrDefault(plugin => plugin.Identifier == scrobbler.SourceIdentifier)?.IsEnabled))
                        {
                            scrobbler.IsEnabled = true;
                        }
                    }

                    // As scrobbling has been enabled, and we're not tracking when the last scrobble was
                    // automatically send any outstanding scrobbles.
                    CheckScrobbleState();
                }
                else if (_isInitialized)
                {
                    // If the scrobbler is already initialized and the user has disabled scrobbling,
                    // disable ALL of the plugins (irrelevant of previous state)
                    foreach(IScrobbleSource plugin in ScrobblePlugins)
                    {
                        plugin.IsEnabled = false;
                    }
                }
            }
        }

        // Entry point for the Scrobbler.  Accepts the API client and user interfaces instances in use.
        public static async Task Initialize(LastFMClient lastFMClient, NotificationThread uiThread)
        {
            _uiThread = uiThread;
            _lastFMClient = lastFMClient;

            // Initialize the plugins, irrespective of enabled state
            foreach (IScrobbleSource source in ScrobblePlugins)
            {
                source.InitializeSource(MinimumScrobbleSeconds, ScrobbleSource_OnTrackMonitoringStarted, Scrobble_OnTrackMonitoring, ScrobbleSource_OnTrackMonitoringEnded, ScrobbleSource_OnScrobbleTrack);
            }

            _isInitialized = true;
        }

        // Method used to notify the user interface that a plugin has continued to monitor a media item
        private static void Scrobble_OnTrackMonitoring(MediaItem mediaItem, int playerPosition)
        {
            _uiThread.TrackMonitoringProgress(mediaItem, playerPosition);
        }

        // Method used to tell the factory to scrobble any outstanding scrobbles.
        private static void ScrobbleSource_OnScrobbleTrack(MediaItem mediaItem)
        {
            CheckScrobbleState();
        }

        // Method used to notify the user interface, and the Last.fm API that a plugin has started monitoring a media item
        private static async void ScrobbleSource_OnTrackMonitoringStarted(MediaItem mediaItem, bool wasResumed)
        {
            mediaItem.StartedPlaying = DateTime.Now;

            _uiThread.TrackMonitoringStarted(mediaItem, wasResumed);

            try
            {
                _lastFMClient.SendMonitoringStatusChanged(mediaItem, LastFMClient.MonitoringStatus.StartedMonitoring).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // No connection available...
            }

            var loveStatus = await GetLoveStatus(mediaItem).ConfigureAwait(false);
        }

        // Method used to get whether the user has previously loved the current media item from the Last.fm API
        private static async Task<Track> GetLoveStatus(MediaItem mediaItem)
        {
            Track responseObject = null;

            try
            {
                // Get the current love status from the API
                responseObject = await _lastFMClient.GetLoveStatus(mediaItem).ConfigureAwait(false);

                // Pass the current love status back to the user interface
                _uiThread.ResetLoveTrackState(Convert.ToBoolean(responseObject?.Info?.UserLoved) ? LastFMClient.LoveStatus.Unlove : LastFMClient.LoveStatus.Love);
            }
            catch (Exception e)
            {
            }

            return responseObject;
        }

        // Method use to notify the user interface, and the Last.fm API (undocumented method) that a plugin has stopped monitoring a media item
        private static void ScrobbleSource_OnTrackMonitoringEnded(MediaItem mediaItem)
        {
            // Notify ell the API monitoring has stopped
            _lastFMClient.SendMonitoringStatusChanged(mediaItem, LastFMClient.MonitoringStatus.StoppedMonitoring);

            // Notify the user interface monitoring has stopped
            _uiThread.TrackMonitoringEnded(mediaItem);
        }

        // Method used to tell the Scrobbler to attempt to scrobble and cached, or queued media items if scrobbling is enabled
        private static async Task CheckScrobbleState()
        {
            List<IScrobbleSource> sourcesToScrobbleFrom = null;

            if (_scrobblingActive)
            {
                // Tell the Ui that the scrobbler is scrobbling
                _uiThread.SetStatus(LocalizationStrings.NotificationThread_Status_CheckingScrobbleStatus);

                List<MediaItem> sourceMedia = new List<MediaItem>();

                // Load any of the cached media items
                sourceMedia = await LoadCachedScrobbles().ConfigureAwait(false);

                // Retrieve from each of the enabled plugins, any media that has been queued but not scrobbled
                foreach (IScrobbleSource source in ScrobblePlugins?.Where(plugin => plugin.IsEnabled).ToList())
                {
                    List<MediaItem> pluginMedia = source.MediaToScrobble;

                    sourceMedia.AddRange(pluginMedia);

                    // Clear the plugins queued media
                    source.ClearQueuedMedia();
                }

                // Communicate with the API to get the current user details from the session and only attempt
                // scrobble if there is communication with the API
                if (await CanScrobble().ConfigureAwait(false))
                {
                    if (sourceMedia != null && sourceMedia.Any())
                    {
                        Console.WriteLine($"Scrobbling {sourceMedia.Count} item(s)....");

                        // Notify the user interface that there are a number of media items about to be scrobbled
                        _uiThread.SetStatus(string.Format(LocalizationStrings.NotificationThread_Status_Scrobbling, sourceMedia.Count));

                        try
                        {
                            // Scrobbling must ONLY send up to 50 items at a time, but we don't want to notify the user interface on
                            // each batch processed.  This list is used to batch up the result of each batch sent to the API
                            ScrobbleResponse overallScrobbleResult = new ScrobbleResponse() { Scrobbles = new Scrobbles() { AcceptedResult = new AcceptedResult(), ScrobbleItems =  new Scrobble[] {}}};

                            do
                            {
                                // Ensure the media is split up into groups of 50
                                List<MediaItem> scrobbleBatch = sourceMedia.Take(50).ToList();

                                // Send the scrobbles to the API
                                ScrobbleResponse scrobbleResult = await _lastFMClient.SendScrobbles(scrobbleBatch).ConfigureAwait(false);

                                // Only remove the items in the batch AFTER a successful scrobble request is sent
                                // so that we can cache the full media list without manipulating the lists again.
                                sourceMedia.RemoveRange(0, scrobbleBatch.Count);

                                // Check the response from LastFM, and cache anything where the API Limit was exceeded
                                CacheFailedItems(scrobbleResult.Scrobbles.ScrobbleItems.ToList());

                                // Track the result of the scrobble
                                overallScrobbleResult.Scrobbles.AcceptedResult.Accepted = scrobbleResult.Scrobbles.AcceptedResult.Accepted;
                                overallScrobbleResult.Scrobbles.AcceptedResult.Ignored = scrobbleResult.Scrobbles.AcceptedResult.Ignored;

                                // Append the current scrobble results to the overall batch
                                List<Scrobble> scrobbledItems = overallScrobbleResult.Scrobbles.ScrobbleItems.ToList();
                                scrobbledItems.AddRange(scrobbleResult.Scrobbles.ScrobbleItems.ToList());

                                overallScrobbleResult.Scrobbles.ScrobbleItems = scrobbledItems.ToArray();
                            }
                            while (sourceMedia.Count > 0);

                            // Now push the scrobble result back to the user interface
                            ShowScrobbleResult(overallScrobbleResult);
                        }
                        catch (Exception ex)
                        {
                            // The API wasn't available.  Cache the media so we can try again.
                            CacheOfflineItems(sourceMedia);

                            // Show the result of the scrobble
                            ShowScrobbleResult(sourceMedia);
                        }
                    }
                    else
                    {
                        // The API wasn't available.  Cache the media so we can try again.
                        CacheOfflineItems(sourceMedia);
                    }
                }
                else
                {
                    // The API wasn't available.  Cache the media so we can try again.
                    CacheOfflineItems(sourceMedia);
                }

                // Tell the user interface to show the current scrobble state (in case the user has gone offline)
                _uiThread?.ShowScrobbleState();
            }
        }

        // Method for passing the failed scrobbles up to the user interface
        private static void ShowScrobbleResult(List<MediaItem> sourceMedia)
        {
            if (Core.Settings.ShowNotifications && (Core.Settings.ShowScrobbleNotifications == null || Core.Settings.ShowScrobbleNotifications==true) && sourceMedia != null && sourceMedia.Count > 0)
            {
                string balloonText =  string.Format(LocalizationStrings.PopupNotifications_FailedToScrobble, sourceMedia.Count());

                _uiThread.ShowNotification(Core.APPLICATION_TITLE, balloonText);
            }
        }

        // Method for passing the results of a successful scrobble to the user interface
        // 'Successful' being that the API accepted the scrobble request, even though it may have reject some for given reasons
        private static void ShowScrobbleResult(ScrobbleResponse scrobbleResult)
        {
            if (Core.Settings.ShowNotifications && (Core.Settings.ShowScrobbleNotifications==null || Core.Settings.ShowScrobbleNotifications==true))
            {
                int successfulScrobbleCount = scrobbleResult.Scrobbles.AcceptedResult.Accepted;
                //int ignoredScrobbles = scrobbleResult.Scrobbles.AcceptedResult.Ignored;

                //string resultText = (successfulScrobbleCount > 0) ? $"Accepted: {successfulScrobbleCount}" : "";
                //resultText += !string.IsNullOrEmpty(resultText) && ignoredScrobbles > 0 ? ", " : "";
                //resultText += (ignoredScrobbles > 0) ? $"Ignored: {ignoredScrobbles}" : "";

                //string balloonText = $"Successfully scrobbled {scrobbleResult.Scrobbles.ScrobbleItems.Count()} track(s).\r\n{resultText}";

                if (successfulScrobbleCount > 0)
                {
                    string balloonText = string.Format(LocalizationStrings.PopupNotifications_ScrobbleSuccess, scrobbleResult.Scrobbles.ScrobbleItems.Count());

                    _uiThread.ShowNotification(Core.APPLICATION_TITLE, balloonText);
                }
            }
        }

        // Method for loading ALL of the cached scrobbles (irresepctive of type) from the file system, to push to the API.
        private async static Task<List<MediaItem>> LoadCachedScrobbles()
        {
            List<MediaItem> failedMedia = new List<MediaItem>();

            List<MediaItem> offlineScrobbleMedia = await LoadOfflineScrobbles().ConfigureAwait(false);
            List<MediaItem> failedScrobbleMedia = await LoadFailedScrobbles().ConfigureAwait(false);

            if(offlineScrobbleMedia != null  && offlineScrobbleMedia.Any())
            {
                failedMedia.AddRange(offlineScrobbleMedia);
            }

            if(failedScrobbleMedia != null && failedScrobbleMedia.Any())
            {
                failedMedia.AddRange(failedScrobbleMedia);
            }

            return failedMedia;
        }

        // Method for loading the failed scrobbles, where scrobbling to the API has failed but the
        // user had a connection (usually as a result of the API limit being exceeded)
        private static async Task<List<MediaItem>> LoadFailedScrobbles()
        {
            List<MediaItem> mediaItems = null;

            string loadingPattern = $"*{Core.FAILEDSCROBBLE_NOCONNECTION}";
            string[] availableFiles = Directory.GetFiles(Core.UserCachePath, loadingPattern);

            foreach (string availableFile in availableFiles)
            {
                try
                {
                    string serializedScrobbles = File.ReadAllText(availableFile);
                    File.Delete(availableFile);

                    List<Scrobble> failedScrobbles = JsonConvert.DeserializeObject<List<Scrobble>>(serializedScrobbles);

                    foreach(Scrobble failedScrobble in failedScrobbles)
                    {
                        MediaItem psuedoItem = new MediaItem()
                        {
                            AlbumName = failedScrobble.Album.CorrectedText,
                            ArtistName = failedScrobble.Artist.CorrectedText,
                            TrackName = failedScrobble.Track.CorrectedText
                        };

                        if (mediaItems == null)
                        {
                            mediaItems = new List<MediaItem>();
                        }

                        mediaItems.Add(psuedoItem);
                    }
                }
                catch (Exception ex)
                {
                    // The file couldn't be loaded... we probably should report this back somehow.
                    // ...but that's not part of the spec for this phase of the project!
                }
            }

            return mediaItems;

        }

        // Method for loading the cached scrobbles, where items were not pushed to the API because the
        // user was offline
        private static async Task<List<MediaItem>> LoadOfflineScrobbles()
        {
            List<MediaItem> mediaItems = null;

            string loadingPattern = $"*{Core.FAILEDSCROBBLE_NOCONNECTION}";
            string[] availableFiles = Directory.GetFiles(Core.UserCachePath, loadingPattern);

            foreach(string availableFile in availableFiles)
            {
                try
                {
                    string serializedScrobbles = File.ReadAllText(availableFile);
                    File.Delete(availableFile);

                    var loadedItems = JsonConvert.DeserializeObject<List<MediaItem>>(serializedScrobbles);
                    if (loadedItems != null && loadedItems.Any())
                    {
                        if (mediaItems == null)
                        {
                            mediaItems = new List<MediaItem>();
                        }

                        mediaItems.AddRange(loadedItems);
                    }
                }
                catch (Exception ex)
                {
                    // The file couldn't be loaded... we probably should report this back somehow.
                    // ...but that's not part of the spec for this phase of the project!
                }
            }

            return mediaItems;
        }

        // Method for caching scrobbles where the result is that the API limit has been exceeded
        private static void CacheFailedItems(List<Scrobble> scrobbles)
        {
            if (scrobbles != null)
            {
                List<Scrobble> failedItems = scrobbles?.Where(item => item.IgnoredMessage.Code == ApiClient.Enums.ReasonCodes.IgnoredReason.ScrobbleLimitExceeded)?.ToList();

                if (failedItems.Any())
                {
                    string fileToWrite = $"{Core.UserCachePath}\\FailedScrobbles_{DateTime.Now:dd_MMM_yyyy}{Core.FAILEDSCROBBLE_LIMITEXCEEDEDFILENAMEEXTENSION}";
                    string dataToWrite = JsonConvert.SerializeObject(failedItems);

                    try
                    {
                        File.WriteAllText(fileToWrite, dataToWrite, Encoding.UTF8);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
        }

        // Method for caching media items that haven't been pushed to the API because the user is offline
        private static void CacheOfflineItems(List<MediaItem> scrobbles)
        {
            if (scrobbles != null && scrobbles.Any())
            {
                string fileToWrite = $"{Core.UserCachePath}\\OfflineScrobbles_{DateTime.Now:dd_MMM_yyyy}{Core.FAILEDSCROBBLE_NOCONNECTION}";
                string dataToWrite = JsonConvert.SerializeObject(scrobbles);

                try
                {
                    File.WriteAllText(fileToWrite, dataToWrite, Encoding.UTF8);
                }
                catch (Exception e)
                {
                }
            }
        }

        // Method used to determine if scrobbling can occur, by requesting details of the currently authenticated user
        private static async Task<bool> CanScrobble()
        {
            bool canScrobble = false;
            UserInfo currentUser = null;

            try
            {
                currentUser = await _lastFMClient.GetUserInfo(Core.Settings.Username).ConfigureAwait(false);
                canScrobble = !string.IsNullOrEmpty(currentUser?.Name);
            }
            catch (Exception ex)
            {

            }

            OnlineStatusUpdated?.BeginInvoke((canScrobble) ? OnlineState.Online : OnlineState.Offline, currentUser, null, null);

            return canScrobble;
        }

        // Method for cleaning up when the scrobbler is destroyed
        public static async void Dispose()
        {
            // Get the unscrobbled media
            List<MediaItem> sourceMedia = new List<MediaItem>();

            sourceMedia = await LoadCachedScrobbles().ConfigureAwait(false);

            foreach (IScrobbleSource plugin in ScrobblePlugins)
            {
                List<MediaItem> pluginMedia = plugin.MediaToScrobble;

                sourceMedia.AddRange(pluginMedia);
                plugin.ClearQueuedMedia();

                plugin.IsEnabled = false;

                try
                {
                    plugin.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            CacheOfflineItems(sourceMedia);
        }

    }
}
