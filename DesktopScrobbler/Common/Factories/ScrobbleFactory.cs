using LastFM.ApiClient;
using LastFM.ApiClient.Models;
using LastFM.Common.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;

namespace LastFM.Common.Factories
{
    public static class ScrobbleFactory
    {
        public enum OnlineState
        {
            Online,
            Offline
        }

        private static LastFMClient _lastFMClient = null;
        //private static Timer _scrobbleTimer = null;
        //private static int _scrobbleTimerSeconds = 0;

        private static bool _scrobblingActive = false;
        private static bool _isInitialized = false;

        private static NotificationThread _uiThread = null;

        public delegate void TrackStarted(MediaItem mediaItem, bool wasResumed);
        public delegate void TrackEnded(MediaItem mediaItem);
        public delegate void ScrobbleTrack(MediaItem mediaItem);

        public delegate void OnlineStatusUpdate(OnlineState currentState, UserInfo latestUserInfo);

        public static OnlineStatusUpdate OnlineStatusUpdated { get; set; }

        public static int MinimumScrobbleSeconds { get; set;} = 30;

        public static List<IScrobbleSource> ScrobblePlugins { get; set; } = new List<IScrobbleSource>();

        public static bool ScrobblingEnabled
        {
            get
            {
                return _scrobblingActive;
            }

            set
            {
                _scrobblingActive = value;

                if(_scrobblingActive && _isInitialized)
                {
                    foreach (IScrobbleSource scrobbler in ScrobblePlugins)
                    {
                        if (Convert.ToBoolean(Core.Settings.ScrobblerStatus.FirstOrDefault(plugin => plugin.Identifier == scrobbler.SourceIdentifier)?.IsEnabled))
                        {
                            scrobbler.IsEnabled = true;
                        }
                    }
                }
                else if (_isInitialized)
                {
                    foreach(IScrobbleSource plugin in ScrobblePlugins)
                    {
                        plugin.IsEnabled = false;
                    }
                }
            }
        }

        public static async Task Initialize(LastFMClient lastFMClient, NotificationThread uiThread)
        {
            _uiThread = uiThread;
            _lastFMClient = lastFMClient;

            // Perform any cached scrobbles as soon as the scrobbler starts
            await CheckScrobbleState();

            // Initialize the plugins, irrespective of enabled state
            foreach (IScrobbleSource source in ScrobblePlugins)
            {
                source.InitializeSource(MinimumScrobbleSeconds, ScrobbleSource_OnTrackStarted, ScrobbleSource_OnTrackEnded, ScrobbleSource_OnScrobbleTrack);
            }

            _isInitialized = true;
        }

        private static void ScrobbleSource_OnScrobbleTrack(MediaItem mediaItem)
        {
            CheckScrobbleState();
        }

        private static async void ScrobbleSource_OnTrackStarted(MediaItem mediaItem, bool wasResumed)
        {
            mediaItem.StartedPlaying = DateTime.Now;

            _uiThread.TrackChanged(mediaItem, wasResumed);

            try
            {
                await _lastFMClient.SendPlayStatusChanged(mediaItem, LastFMClient.PlayStatus.StartedListening);
            }
            catch (Exception)
            {
                // No connection available...
            }
        }

        private static void ScrobbleSource_OnTrackEnded(MediaItem mediaItem)
        {
            _lastFMClient.SendPlayStatusChanged(mediaItem, LastFMClient.PlayStatus.StoppedListening);
            _uiThread.TrackChanged(null, false);
        }

        private static async Task CheckScrobbleState()
        {
            List<IScrobbleSource> sourcesToScrobbleFrom = null;

            if (_scrobblingActive)
            {
                _uiThread.SetStatus("Checking Scrobble State...");

                List<MediaItem> sourceMedia = new List<MediaItem>();

                sourceMedia = await LoadCachedScrobbles();

                foreach (IScrobbleSource source in ScrobblePlugins?.Where(plugin => plugin.IsEnabled).ToList())
                {
                    List<MediaItem> pluginMedia = source.MediaToScrobble;

                    sourceMedia.AddRange(pluginMedia);
                    source.ClearQueuedMedia();
                }

                if (await CanScrobble())
                {
                    if (sourceMedia != null && sourceMedia.Any())
                    {
                        _uiThread.SetStatus($"Scrobbling {sourceMedia.Count} item(s)....");

                        try
                        {
                            do
                            {
                                // Ensure the media is split up into groups of 50
                                List<MediaItem> scrobbleBatch = sourceMedia.Take(50).ToList();
                                ScrobbleResponse scrobbleResult = await _lastFMClient.SendScrobbles(scrobbleBatch);

                                // Only remove the items in the batch AFTER a successful scrobble request is sent
                                // so that we can cache the full media list without manipulating the lists again.
                                sourceMedia.RemoveRange(0, scrobbleBatch.Count);

                                // Check the response from LastFM, and cache anything where the API Limit was exceeded
                                CacheFailedItems(scrobbleResult.Scrobbles.ScrobbleItems.ToList());

                                // Show the result of the scrobble
                                ShowScrobbleResult(scrobbleResult);
                            }

                            while (sourceMedia.Count > 0);
                        }
                        catch (Exception)
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

                _uiThread?.ShowScrobbleState();
            }
        }

        private static void ShowScrobbleResult(List<MediaItem> sourceMedia)
        {
            if (Core.Settings.ShowScrobbleNotifications)
            {
                string balloonText = $"Failed to scrobble {sourceMedia.Count()} track(s).";

                _uiThread.ShowNotification(Core.APPLICATION_TITLE, balloonText);
            }
        }

        private static void ShowScrobbleResult(ScrobbleResponse scrobbleResult)
        {
            if (Core.Settings.ShowScrobbleNotifications)
            {
                int successfulScrobbleCount = scrobbleResult.Scrobbles.AcceptedResult.Accepted;
                //int ignoredScrobbles = scrobbleResult.Scrobbles.AcceptedResult.Ignored;

                //string resultText = (successfulScrobbleCount > 0) ? $"Accepted: {successfulScrobbleCount}" : "";
                //resultText += !string.IsNullOrEmpty(resultText) && ignoredScrobbles > 0 ? ", " : "";
                //resultText += (ignoredScrobbles > 0) ? $"Ignored: {ignoredScrobbles}" : "";

                //string balloonText = $"Successfully scrobbled {scrobbleResult.Scrobbles.ScrobbleItems.Count()} track(s).\r\n{resultText}";

                if (successfulScrobbleCount > 0)
                {
                    string balloonText = $"Successfully scrobbled {scrobbleResult.Scrobbles.ScrobbleItems.Count()} track(s).";

                    _uiThread.ShowNotification(Core.APPLICATION_TITLE, balloonText);
                }
            }
        }

        private async static Task<List<MediaItem>> LoadCachedScrobbles()
        {
            List<MediaItem> failedMedia = new List<MediaItem>();

            List<MediaItem> offlineScrobbleMedia = await LoadOfflineScrobbles();
            List<MediaItem> failedScrobbleMedia = await LoadFailedScrobbles();

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

                    mediaItems = JsonConvert.DeserializeObject<List<MediaItem>>(serializedScrobbles);
                }
                catch (Exception ex)
                {
                    // The file couldn't be loaded... we probably should report this back somehow.
                    // ...but that's not part of the spec for this phase of the project!
                }
            }

            return mediaItems;
        }

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


        private static async Task<bool> CanScrobble()
        {
            bool canScrobble = false;
            UserInfo currentUser = null;

            try
            {
                currentUser = await _lastFMClient.GetUserInfo(Core.Settings.Username);
                canScrobble = !string.IsNullOrEmpty(currentUser?.Name);
            }
            catch (Exception ex)
            {

            }

            OnlineStatusUpdated?.Invoke((canScrobble) ? OnlineState.Online : OnlineState.Offline, currentUser);

            return canScrobble;
        }

        public static async void Dispose()
        {
            // Get the unscrobbled media
            List<MediaItem> sourceMedia = new List<MediaItem>();

            sourceMedia = await LoadCachedScrobbles();

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
