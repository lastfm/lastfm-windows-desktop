using LastFM.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static LastFM.Common.Factories.ScrobbleFactory;
using LastFM.ApiClient.Models;
using System.Diagnostics;
using System.Windows.Forms;
using WMPLib;

namespace DesktopScrobbler
{
    // The embedded Windows Media Player Scrobbler Source.  This uses an AXImport'ed Windows Media Player library, and every <X> seconds
    // connects to the COM library, scans the current state of the media player and interacts with the Scrobbler
    // accordingly.  The AXImport'ed library exists as an OCX interop library, which is added to a hidden form as it can only
    // exist as part of a form (although, we're not 100% sure that's necessarily true!)

    // A future test might involve loading the OCX without 'siting' it, but we have a recollection that the player requires
    // a site owner for it to migrate / shadow the player before it becomes available in a valid state

    public class WindowsMediaScrobbleSource : IScrobbleSource
    {
        // The currently tracked media item
        private MediaItem _currentMediaItem = null;

        // The current queue of media items that are waiting to be scrobbled
        private List<MediaItem> _mediaToScrobble = new List<MediaItem>();

        // A sync lock for ensuring the media queue is updated by only one process at a time
        private object _mediaLock = new object();

        // The default minimum number of seconds a media item has been 'tracked' for (listened to by the user) before it is
        // considered 'scrobbleable'
        private int _minimumScrobbleSeconds = 30;

        // How long the current media item has been successfully tracked for
        private int _currentMediaTrackingTime = 0;

        // State determining if the current media item has already been added to the scrobble queue
        private bool _currentMediaWasScrobbled = false;

        // State determining if the media player was last known to have been 'paused'
        private bool _lastStatePaused = false;

        // State determining if the plugin has been successfully initialized
        private bool _isIntialized = false;

        // State determining if the plugin is currently enabled (and should track iTunes state)
        private bool _isEnabled = false;

        // Internal instance of the scrobble timer
        private System.Timers.Timer _scrobbleTimer = null;

        // Delegate method for telling the Scrobbler that a new track is being monitored
        private TrackMonitoringStarted _onTrackMonitoringStarted = null;

        // Delegate method for telling the Scrobbler that monitoring is continuing
        private TrackMonitoring _onTrackMonitoring = null;

        // Delegate method for telling the Scrobbler that monitoring of the current item has ended
        private TrackMonitoringEnded _onTrackMonitoringEnded = null;

        // Delegate method for Scrobbling the current media item
        private ScrobbleTrack _onScrobbleTrack = null;

        // The current instance of the Windows Media Player host form
        private WindowsMediaPlayer _mediaPlayer = null;

        // Instantiation of the plugin (outside of the standard dependency injection) that accepts the host
        // for the media player control
        public WindowsMediaScrobbleSource(Form mediaPlayerHost)
        {
            _mediaPlayer = mediaPlayerHost as WindowsMediaPlayer;
        }

        // The unique identifier for this plugin
        public Guid SourceIdentifier
        {
            get
            {
                return new Guid("7471fa52-0007-43c9-a644-945fbc7f5897");
            }
        }

        // The human-readable description of this plugin
        public string SourceDescription
        {
            get
            {
                return "Windows Media Player";
            }
        }

        // Property to expose or set the current state defining if this plugin is enabled
        public bool IsEnabled
        {
            get
            {
                return _isEnabled && _isIntialized;
            }

            set
            {
                _isEnabled = value;

                if(_isEnabled)
                {
                    _scrobbleTimer?.Stop();
                    _scrobbleTimer?.Start();
                }
                else
                {
                    _scrobbleTimer?.Stop();
                }
            }
        }

        // Helper function for safely clearing the media item queue
        public void ClearQueuedMedia()
        {
            _scrobbleTimer?.Stop();

            lock (_mediaLock)
            {
                _mediaToScrobble?.Clear();
            }

            _scrobbleTimer?.Start();
        }

        // Property to expose, or safely set the media item queue
        public List<MediaItem> MediaToScrobble
        {
            get
            {
                return _mediaToScrobble;
            }

            private set
            {
                lock (_mediaLock)
                {
                    _mediaToScrobble = value;
                }
            }
        }

        // Plugin intialization function
        public void InitializeSource(int minimumScrobbleSeconds, TrackMonitoringStarted onTrackMonitoringStarted, TrackMonitoring onTrackMonitoring, TrackMonitoringEnded onTrackMonitoringEnded, ScrobbleTrack onScrobbleTrack)
        {
            _minimumScrobbleSeconds = minimumScrobbleSeconds;

            _onTrackMonitoringStarted = onTrackMonitoringStarted;
            _onTrackMonitoring = onTrackMonitoring;
            _onTrackMonitoringEnded = onTrackMonitoringEnded;
            _onScrobbleTrack = onScrobbleTrack;

            _isIntialized = true;

            try
            {
                // Create a new scrobbler timer, to fire every second
                _scrobbleTimer = new System.Timers.Timer();
                _scrobbleTimer.Interval = 1000;

                // The ananoymous delegate event that occurs every time the timer fires (elapses)
                _scrobbleTimer.Elapsed += async (o, e) =>
                    {
                        // Stop the timer to prevent multiple executions at the same time
                        _scrobbleTimer.Stop();

                        // Check for the Windows Media Player process to ensure it's running.
                        // If we don't check for it, the plugin would end up launching it, which we don't want
                        Process[] wmpProcesses = Process.GetProcessesByName("wmplayer");

                        if (wmpProcesses.Length > 0 && _mediaPlayer == null)
                        {
                            _mediaPlayer = new WindowsMediaPlayer();
                            Console.WriteLine("Windows Media Player Plugin successfully connected to the WMP COM library.");
                        }
                        else if (wmpProcesses.Length == 0 && _mediaPlayer != null)
                        {
                            Console.WriteLine("Windows Media Player process not detected.  Waiting for Windows Media Player process to start...");
                        }

                        if (_mediaPlayer != null)
                        {
                            #if DebugWMPScrobbler
                                Console.WriteLine("Windows Media Player Plugin checking media state...");
                            #endif

                            if (_isEnabled)
                            {
                                // Get the current media from Windows Media Player itself (using our helper function)
                                MediaItem mediaDetail = await GetMediaDetail().ConfigureAwait(false);

                                // Get the media player state
                                WMPPlayState playerState = _mediaPlayer?.Player?.playState ?? WMPPlayState.wmppsStopped;
                                double playerPosition = _mediaPlayer?.Player?.Ctlcontrols?.currentPosition ?? 0;

                                // Determine if there is any media loaded
                                bool hasMedia = mediaDetail != null;

                                // Determine if the current track is deemed to have 'ended' as it's been fully listened to
                                bool hasReachedTrackEnd = hasMedia && (int)playerPosition + 1 >= (int)mediaDetail?.TrackLength && mediaDetail?.TrackLength > 0;

                                // Determine if the current track playing isn't the last one we knew about
                                bool hasTrackChanged = _currentMediaItem?.TrackName != mediaDetail?.TrackName;

                                // Determine if the media player is in the 'Paused' state
                                bool isPaused = playerState == WMPPlayState.wmppsPaused;

                                // Determine if the media player is in the 'Playing' state
                                bool isPlaying = playerState == WMPPlayState.wmppsPlaying;

                                // Determine if the current media item is scrobbleable
                                bool canScrobble = _currentMediaTrackingTime >= _minimumScrobbleSeconds &&
                                    (_currentMediaTrackingTime >= Convert.ToInt32(Math.Min(Convert.ToInt32(_currentMediaItem?.TrackLength) / 2, 4 * 60)) && !_currentMediaWasScrobbled);


#if DebugWMPScrobbler
                                    Console.WriteLine($"Windows Media Player Plugin: Position {playerPosition} of { mediaDetail?.TrackLength }, Tracker time: {_currentMediaTrackingTime}...");
#endif

                                // If the media player is still playing and the track has changed, or the current media has reached it's end (and therefore the media player has stopped)
                                if ((isPlaying && hasMedia && hasTrackChanged) || hasReachedTrackEnd)
                                {
                                    // Reset the last paused state
                                    _lastStatePaused = false;

                                    // Reset the current tracking time to the default number of timer seconds
                                    _currentMediaTrackingTime = 1;

                                    // If we knew about a media item before we got here
                                    if (_currentMediaItem != null)
                                    {
                                        // Fire the 'track monitoring has ended' event for the previous item
                                        _onTrackMonitoringEnded?.Invoke(_currentMediaItem);

                                        // Fire the 'scrobble the item' event for the previous item
                                        _onScrobbleTrack?.Invoke(_currentMediaItem);
                                    }

                                    Console.WriteLine("Windows Media Player: Raising Track Change Method.");

                                    // If the reason we are here is because there is a new track being monitored
                                    if (hasTrackChanged)
                                    {
                                        // Track when we started monitoring the new item (to pass to the Last.fm API)
                                        mediaDetail.StartedPlaying = DateTime.Now;

                                        // Set the current monitor item, to what the media player has told us is playing
                                        _currentMediaItem = mediaDetail;

                                        // Fire the 'track monitoring has started' even for the new item
                                        _onTrackMonitoringStarted?.Invoke(mediaDetail, false);
                                    }
                                    // Otherwise if we got here because the current item has ended, and no new item is playing
                                    else if (hasReachedTrackEnd)
                                    {
                                        // Clear the currently tracked media item, so that if the user starts playing it again, it is treated
                                        // as an entirely new scrobble
                                        _currentMediaItem = null;
                                    }

                                    // Reset the flag determining if the current item has been added to the Scrobble queue
                                    _currentMediaWasScrobbled = false;
                                }
                                // If the media playing is playing and has media associated, we have reached the point where the item has been
                                // tracked beyond the minimum number of tracking seconds and the item hasn't already been added to the scrobble queue
                                else if (isPlaying && hasMedia && canScrobble && !_currentMediaWasScrobbled)
                                {
                                    // Safely add the media item to the scrobble queue
                                    lock (_mediaLock)
                                    {
                                        _mediaToScrobble.Add(_currentMediaItem);
                                    }

                                    // Fire the 'we are still tracking this item' event
                                    _onTrackMonitoring?.Invoke(_currentMediaItem, (int)playerPosition);

                                    // Update the current media tracking time
                                    _currentMediaTrackingTime++;

                                    // Mark the item as having been added to the scrobble queue
                                    //(potential improvement, move this property to _currentMediaItem and remove the local variable)
                                    _currentMediaWasScrobbled = true;

                                    Console.WriteLine($"Windows Media Player: Track {mediaDetail.TrackName} queued for Scrobbling.");
                                }
                                // The media player is playing, and is still playing the same track
                                else if (isPlaying && !hasTrackChanged)
                                {
                                    // If the media player wasn't last in the paused state
                                    if (_lastStatePaused)
                                    {
                                        // Fire the 'we started monitoring this item' event
                                        _onTrackMonitoringStarted?.Invoke(_currentMediaItem, _lastStatePaused);
                                    }

                                    // Fire the 'we are still monitoring this item event' (possibly should be inside an else, although won't hurt
                                    // where it is)
                                    _onTrackMonitoring?.Invoke(_currentMediaItem, (int)playerPosition);

                                    // Update the current media tracking time
                                    _currentMediaTrackingTime++;
                                }
                                // The media player is not playing
                                else if (!isPlaying)
                                {
                                    // If we had been playing, invoke the Track Ended callback
                                    if (_currentMediaTrackingTime > 0)
                                    {
                                        _onTrackMonitoringEnded?.Invoke(mediaDetail);
                                        _currentMediaItem = null;
                                    }

                                    // Set the persisted pause state
                                    _lastStatePaused = isPaused;

                                    // If we're not paused (FF, Rewind)
                                    if (!isPaused)
                                    {
                                        // Reset the state tracking how long we played this track for
                                        _lastStatePaused = false;
                                        _currentMediaTrackingTime = 0;
                                        _currentMediaWasScrobbled = false;
                                    }
                                }
                            }

                            #if DebugWMPScrobbler
                                Console.WriteLine("Windows Media Plugin checking media state complete.");
                            #endif
                        }
                        else if (_currentMediaItem != null)
                        {
                            _onTrackMonitoringEnded?.Invoke(_currentMediaItem);
                            _currentMediaItem = null;
                            _currentMediaWasScrobbled = false;
                        }

                        _scrobbleTimer?.Start();
                    };                
            }
            catch (Exception ex)
            {
                try
                {
                    _scrobbleTimer?.Start();
                }
                catch (Exception exception)
                {
                    // Can occur if you close the application as it's starting up
                }
            }
        }

        [STAThread]
        // Private function for querying Windows Media Player about what's currently playing
        // (Note: the STAThread attribute may be redundant)
        private async Task<MediaItem> GetMediaDetail()
        {
            MediaItem playerMedia = null;

            try
            {
                var currentMedia = _mediaPlayer?.Player?.Ctlcontrols?.currentItem;

                if (currentMedia != null)
                {
                    playerMedia = new MediaItem() { TrackName = currentMedia?.getItemInfo("Title"), AlbumName = currentMedia?.getItemInfo("Album"), ArtistName = currentMedia?.getItemInfo("Artist"), TrackLength = Convert.ToDouble(currentMedia?.duration), AlbumArtist = currentMedia?.getItemInfo("AlbumArtist") };
                }
            }
            catch (Exception ex)
            {
            }

            return playerMedia;
        }

        // Helper function for cleaning up when the plugin is closed
        public void Dispose()
        {
            _scrobbleTimer?.Stop();
            _scrobbleTimer?.Dispose();

            if (_mediaPlayer != null)
            {
                try
                {
                    _mediaPlayer?.BeginInvoke(new MethodInvoker(() =>
                    {
                        _mediaPlayer?.Close();
                    }));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                _mediaPlayer = null;
            }
        }
    }
}
