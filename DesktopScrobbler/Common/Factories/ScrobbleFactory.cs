using LastFM.ApiClient;
using LastFM.ApiClient.Models;
using LastFM.Common.Classes;
using Newtonsoft.Json.Linq;
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
        private static LastFMClient _lastFMClient = null;
        private static Timer _scrobbleTimer = null;
        private static int _scrobbleTimerSeconds = 0;

        private static bool _scrobblingActive = false;
        private static NotificationThread _uiThread = null;

        public delegate void TrackStarted(MediaItem mediaItem);
        public delegate void TrackEnded(MediaItem mediaItem);

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

                if(_scrobblingActive)
                {
                    _scrobbleTimer.Start();

                    foreach (IScrobbleSource scrobbler in ScrobblePlugins)
                    {
                        if (Convert.ToBoolean(Core.Settings.ScrobblerStatus.FirstOrDefault(plugin => plugin.Identifier == scrobbler.SourceIdentifier)?.IsEnabled))
                        {
                            scrobbler.IsEnabled = true;
                        }
                    }
                }
                else
                {
                    _scrobbleTimer.Stop();

                    foreach(IScrobbleSource plugin in ScrobblePlugins)
                    {
                        plugin.IsEnabled = false;
                    }
                }
            }
        }

        public static void Initialize(LastFMClient lastFMClient, NotificationThread uiThread)
        {
            _uiThread = uiThread;
            _lastFMClient = lastFMClient;

            // Get the plugins
            foreach(IScrobbleSource source in ScrobblePlugins)
            {
                source.InitializeSource(MinimumScrobbleSeconds, ScrobbleSource_OnTrackStarted, ScrobbleSource_OnTrackEnded);
            }

            _scrobbleTimer = new Timer(1000);

            _scrobbleTimer.Elapsed += ScrobbleTimer_Elapsed;

        }

        private static void ScrobbleSource_OnTrackStarted(MediaItem mediaItem)
        {
            mediaItem.StartedPlaying = DateTime.Now;
            _lastFMClient.SendPlayStatusChanged(mediaItem, LastFMClient.PlayStatus.StartedListening);
        }

        private static void ScrobbleSource_OnTrackEnded(MediaItem mediaItem)
        {
            _lastFMClient.SendPlayStatusChanged(mediaItem, LastFMClient.PlayStatus.StoppedListening);
        }

        private static async void ScrobbleTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _scrobbleTimer.Stop();

            if(_scrobblingActive)
            {
                _scrobbleTimerSeconds++;

                if (_scrobbleTimerSeconds >= MinimumScrobbleSeconds)
                {
                    _scrobbleTimerSeconds = 0;
                    await CheckScrobbleState();
                }
            }

            _uiThread?.SetStatus($"Next Scrobble check in {MinimumScrobbleSeconds - _scrobbleTimerSeconds} second(s)");

            if (_scrobblingActive)
            {
                _scrobbleTimer.Start();
            }
        }

        private static async Task CheckScrobbleState()
        {
            List<IScrobbleSource> sourcesToScrobbleFrom = null;

            if (_scrobblingActive)
            {
                _uiThread.SetStatus("Checking Scrobble State...");

                List<MediaItem> sourceMedia = new List<MediaItem>();

                foreach (IScrobbleSource source in ScrobblePlugins?.Where(plugin => plugin.IsEnabled).ToList())
                {
                    sourceMedia.AddRange(source.MediaToScrobble);
                }

                if (sourceMedia != null && sourceMedia.Any())
                {

                    if (await CanScrobble())
                    {
                        _uiThread.SetStatus($"Scrobbling {sourceMedia.Count} item(s)....");

                        ScrobbleResponse scrobbleResult = await _lastFMClient.SendScrobbles(sourceMedia);
                        CacheFailedItems(scrobbleResult.Scrobbles.ScrobbleItems.ToList());
                    }
                    else
                    {
                        // Cache to the local database
                        CacheFailedItems(sourceMedia);
                    }
                }
            }

        }

        private static void CacheFailedItems(List<Scrobble> scrobbles)
        {
            if (scrobbles != null)
            {
                List<Scrobble> failedItems = scrobbles?.Where(item => item.IgnoredMessage.Code == ApiClient.Enums.ReasonCodes.IgnoredReason.ScrobbleLimitExceeded)?.ToList();

                string fileToWrite = $"{Core.UserSettingsPath}\\FailedScrobbles_{DateTime.Now:dd_MMM_yyyy}{Core.FAILEDSCROBBLE_LIMITEXCEEDEDFILENAMEEXTENSION}";
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

        private static void CacheFailedItems(List<MediaItem> scrobbles)
        {
            if (scrobbles != null)
            {
                string fileToWrite = $"{Core.UserSettingsPath}\\FailedScrobbles_{DateTime.Now:dd_MMM_yyyy}{Core.FAILEDSCROBBLE_NOCONNECTION}";
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

            try
            {
                var userInfo = await _lastFMClient.GetUserInfo(Core.Settings.Username);
                canScrobble = !string.IsNullOrEmpty(userInfo?.Name);
            }
            catch (Exception ex)
            {

            }

            return canScrobble;
        }

        public static void Dispose()
        {
            _scrobbleTimer?.Stop();
            _scrobbleTimer = null;

            foreach (IScrobbleSource plugin in ScrobblePlugins)
            {
                plugin.Dispose();
            }
        }

    }
}
