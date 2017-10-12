using LastFM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iTunesLib;
using System.Timers;
using System.Runtime.InteropServices;
using static LastFM.Common.Factories.ScrobbleFactory;
using LastFM.ApiClient.Models;
using System.Diagnostics;

namespace ITunesScrobblePlugin
{
    // The embedded iTunes Scrobbler Source.  This uses the Apple iTunes COM library, and every <X> seconds
    // connects to the COM library, scans the current state of the media player and interacts with the Scrobbler
    // accordingly
    public class iTunesScrobblePlugin : IScrobbleSource
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

        // How often the scrobble timer should fire (in seconds) to track the state of iTunes
        // Increase or reduce this to cope with the iTunes scripting and closing issues
        private int _timerInterval = 5;

        // State determining if the plugin has been successfully initialized
        private bool _isIntialized = false;

        // State determining if the plugin is currently enabled (and should track iTunes state)
        private bool _isEnabled = false;

        // State determining if the iTunes media player was last known to have been 'paused' (stopped in iTunes cause)
        private bool _lastStatePaused = false;

        // Internal instance of the scrobble timer
        private Timer _scrobbleTimer = null;

        // Delegate method for telling the Scrobbler that a new track is being monitored
        private TrackMonitoringStarted _onTrackMonitoringStarted = null;

        // Delegate method for telling the Scrobbler that monitoring is continuing
        private TrackMonitoring _onTrackMonitoring = null;

        // Delegate method for telling the Scrobbler that monitoring of the current item has ended
        private TrackMonitoringEnded _onTrackMonitoringEnded = null;

        // Delegate method for Scrobbling the current media item
        private ScrobbleTrack _onScrobbleTrack = null;

        // The unique identifier for this plugin
        public Guid SourceIdentifier
        {
            get
            {
                return new Guid("a458e8af-4282-4bd7-8894-14969c63a7d5");
            }
        }

        // The human-readable description of this plugin
        public string SourceDescription
        {
            get
            {
                return "iTunes";
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
            // Stop the scrobble timer so that no updaes can be done
            _scrobbleTimer?.Stop();

            // Safely clear the queue
            lock (_mediaLock)
            {
                _mediaToScrobble?.Clear();
            }

            // Re-start the timer
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
                // Create a new scrobbler timer, to fire at the specified number of seconds
                _scrobbleTimer = new Timer();
                _scrobbleTimer.Interval = 1000 * _timerInterval;

                // The ananoymous delegate event that occurs every time the timer fires (elapses)
                _scrobbleTimer.Elapsed += async (o, e) =>
                    {
                        // Stop the timer to prevent multiple executions at the same time
                        _scrobbleTimer.Stop();

                        // Check for the iTunes process to ensure it's running.
                        // If we don't check for it, the plugin would end up launching it when we connect, which we don't want
                        Process[] iTunesProcesses = Process.GetProcessesByName("iTunes");

                        if (iTunesProcesses.Length > 0)
                        {
                            try
                            {

                                if (_isEnabled)
                                {
                                    iTunesApp iTunesApp = new iTunesApp();

                                    #if DebugiTunes
                                        Console.WriteLine("iTunes Plugin successfully connected to iTunes COM library.");
                                        Console.WriteLine("iTunes Plugin checking media state...");
                                    #endif

                                    // Get the current media from iTunes itself (using our helper function)
                                    MediaItem mediaDetail = await GetMediaDetail(iTunesApp).ConfigureAwait(false);

                                    ITPlayerState playerState = ITPlayerState.ITPlayerStateStopped;
                                    double playerPosition = 0;

                                    try
                                    {
                                        // Get the iTunes media player state
                                        playerState = iTunesApp?.PlayerState ?? ITPlayerState.ITPlayerStateStopped;
                                        playerPosition = iTunesApp?.PlayerPosition ?? 0;
                                    }
                                    catch (COMException comEx)
                                    {
                                        // If the player is in an invalid state, this is going to happen!
                                    }
                                    
                                    // Determine if there is any media loaded
                                    bool hasMedia = mediaDetail != null;

                                    // Determine if the current track is deemed to have 'ended' as it's been fully listened to
                                    bool hasReachedTrackEnd = hasMedia && (int)playerPosition + _timerInterval >= (int)mediaDetail?.TrackLength && mediaDetail?.TrackLength > 0;

                                    // Determine if the current track playing isn't the last one we knew about
                                    bool hasTrackChanged = _currentMediaItem?.TrackName != mediaDetail?.TrackName;

                                    // Determine if the media player is in the 'Playing' state
                                    bool isPlaying = playerState == ITPlayerState.ITPlayerStatePlaying;

                                    // Determine if the media player is in the 'Stopped' (paused) state
                                    bool isPaused = playerState == ITPlayerState.ITPlayerStateStopped;

                                    // Determine if the current media item is scrobbleable
                                    bool canScrobble = _currentMediaTrackingTime >= _minimumScrobbleSeconds &&
                                        (_currentMediaTrackingTime >= Convert.ToInt32(Math.Min(Convert.ToInt32(_currentMediaItem?.TrackLength) / 2, 4 * 60)) && !_currentMediaWasScrobbled);

#if DebugiTunes
                                        Console.WriteLine($"iTunes Media Player Plugin: Position {playerPosition} of { mediaDetail?.TrackLength }, Tracker time: {_currentMediaTrackingTime}...");
#endif

                                    // If we have reached the point where the item has been tracked beyond the minimum number of tracking seconds 
                                    // and the item hasn't already been added to the scrobble queue
                                    if (canScrobble && !_currentMediaWasScrobbled)
                                    {
                                        // Safely add the media item to the scrobble queue
                                        lock (_mediaLock)
                                        {
                                            _mediaToScrobble.Add(_currentMediaItem);
                                        }

                                        // Fire the 'we are still tracking this item' event
                                        _onTrackMonitoring?.BeginInvoke(_currentMediaItem, (int)playerPosition, null, null);

                                        // Mark the item as having been added to the scrobble queue
                                        //(potential improvement, move this property to _currentMediaItem and remove the local variable)
                                        _currentMediaWasScrobbled = true;

                                        Console.WriteLine($"Track {mediaDetail.TrackName} queued for Scrobbling.");
                                    }

                                    // If the media player is still playing and the track has changed, or the current media has reached it's end (and therefore the media player has stopped)
                                    if ((isPlaying && hasMedia && hasTrackChanged) || hasReachedTrackEnd)
                                    {
                                        // Reset the last paused state
                                        _lastStatePaused = false;

                                        // Reset the current tracking time to the default number of timer seconds
                                        _currentMediaTrackingTime = _timerInterval;

                                        // If we knew about a media item before we got here
                                        if (_currentMediaItem != null)
                                        {
                                            // Fire the 'track monitoring has ended' event for the previous item
                                            _onTrackMonitoringEnded?.BeginInvoke(_currentMediaItem, null, null);

                                            // Fire the 'scrobble the item' event for the previous item
                                            _onScrobbleTrack?.BeginInvoke(_currentMediaItem, null, null);
                                        }

                                        Console.WriteLine("iTunes: Raising Track Change Method.");

                                        // If the reason we are here is because there is a new track being monitored
                                        if (hasTrackChanged)
                                        {
                                            // Set the current monitor item, to what the media player has told us is playing
                                            _currentMediaItem = mediaDetail;

                                            // Fire the 'track monitoring has started' even for the new item
                                            _onTrackMonitoringStarted?.BeginInvoke(mediaDetail, false, null, null);

                                            // Track when we started monitoring the new item (to pass to the Last.fm API)
                                            mediaDetail.StartedPlaying = DateTime.Now;
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
                                    // The media player is playing, and is still playing the same track
                                    else if (isPlaying && !hasTrackChanged)
                                    {
                                        // If the media player wasn't last in the paused state
                                        if (_lastStatePaused)
                                        {
                                            // Fire the 'we started monitoring this item' event
                                            _onTrackMonitoringStarted?.BeginInvoke(_currentMediaItem, _lastStatePaused, null, null);

                                            // Reset the pause state flag
                                            _lastStatePaused = false;
                                        }

                                        // Fire the 'we are still monitoring this item event' (possibly should be inside an else, although won't hurt
                                        // where it is)
                                        _onTrackMonitoring?.BeginInvoke(_currentMediaItem, (int)playerPosition, null, null);

                                        // Update the current media tracking time
                                        _currentMediaTrackingTime += _timerInterval;
                                    }
                                    // The media player is not playing
                                    else if (!isPlaying)
                                    {
                                        // If we had been playing, invoke the Track Ended callback
                                        if (_currentMediaTrackingTime > _timerInterval && !_lastStatePaused)
                                        {
                                            // Fire the 'we've stopped tracking this item' event
                                            _onTrackMonitoringEnded?.BeginInvoke(mediaDetail, null, null);
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
                                    
                                    // Release the iTunes COM library, so that the user _might_ be able to exit iTunes without getting a warning
                                    if (iTunesApp != null)
                                    {
                                        Marshal.ReleaseComObject(iTunesApp);
                                        iTunesApp = null;
                                    }
                                    System.GC.Collect();
                                }
#if DebugiTunes
                                Console.WriteLine("iTunes Plugin checking media state complete.");
#endif
                            }
                            catch (COMException cEx)
                            {
                                // Ignore the COM exception, the library is either trying to communicate with a property
                                // that isn't available.  IE PlayerPosition is not available when the player is stopped.

                                // It might also be tearing down
                            }
                            catch (Exception)
                            {
                                // Some other exception occured, at some point consider logging it...?
                            }
                        }
                        else if (iTunesProcesses.Length == 0 && _currentMediaItem != null)
                        {
                            _onTrackMonitoringEnded?.BeginInvoke(_currentMediaItem, null, null);
                            _currentMediaItem = null;
                            _currentMediaWasScrobbled = false;
                            Console.WriteLine("iTunes process not detected.  Waiting for iTunes process to start...");
                        }

                        
                        _scrobbleTimer?.Start();
                    };                
            }
            catch (Exception ex)
            {
                try
                {
                    // If an unexpected error occured, don't let that stop us from performing any tracking,
                    // as the error might only be temporary
                    _scrobbleTimer?.Start();
                }
                catch (Exception exception)
                {
                    // Can occur if you close the application as it's starting up
                }
            }
        }

        // Private function for querying iTunes about what's currently playing
        private async Task<MediaItem> GetMediaDetail(iTunesApp appInstance)
        {
            MediaItem iTunesMediaDetail = null;

            try
            {
                iTunesMediaDetail = new MediaItem() { TrackName = appInstance?.CurrentTrack?.Name, AlbumName = appInstance?.CurrentTrack?.Album, ArtistName = appInstance?.CurrentTrack?.Artist, TrackLength = Convert.ToDouble(appInstance?.CurrentTrack?.Duration) };
            }
            catch (Exception ex)
            {
            }

            return iTunesMediaDetail;
        }

        // Helper function for cleaning up when the plugin is closed
        public void Dispose()
        {
            _scrobbleTimer?.Stop();
            _scrobbleTimer?.Dispose();
        }
    }
}
