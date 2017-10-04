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
    public class iTunesScrobblePlugin : IScrobbleSource
    {
        private MediaItem _currentMediaItem = null;
        private MediaItem _lastQueuedItem = null;

        private List<MediaItem> _mediaToScrobble = new List<MediaItem>();

        private object _mediaLock = new object();

        private int _minimumScrobbleSeconds = 30;
        private int _currentMediaPlayTime = 0;

        private bool _isIntialized = false;
        private bool _isEnabled = false;
        private bool _lastStatePaused = false;

        private Timer _scrobbleTimer = null;

        private TrackStarted _onTrackStarted = null;
        private TrackEnded _onTrackEnded = null;
        private ScrobbleTrack _onScrobbleTrack = null;

        public Guid SourceIdentifier
        {
            get
            {
                return new Guid("a458e8af-4282-4bd7-8894-14969c63a7d5");
            }
        }

        public string SourceDescription
        {
            get
            {
                return "iTunes";
            }
        }

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
                    _scrobbleTimer?.Start();
                }
                else
                {
                    _scrobbleTimer?.Stop();
                }
            }
        }

        public void ClearQueuedMedia()
        {
            _scrobbleTimer?.Stop();

            lock (_mediaLock)
            {
                _mediaToScrobble?.Clear();
            }

            _scrobbleTimer?.Start();
        }

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

        public void InitializeSource(int minimumScrobbleSeconds, TrackStarted onTrackStartedCallback, TrackEnded onTrackEndedCallback, ScrobbleTrack onScrobbleTrack)
        {
            _minimumScrobbleSeconds = minimumScrobbleSeconds;

            _onTrackStarted = onTrackStartedCallback;
            _onTrackEnded = onTrackEndedCallback;
            _onScrobbleTrack = onScrobbleTrack;

            _isIntialized = true;

            try
            {
                _scrobbleTimer = new Timer();
                _scrobbleTimer.Interval = 1000;

                    _scrobbleTimer.Elapsed += async (o, e) =>
                    {
                        _scrobbleTimer.Stop();

                        // Check for the iTunes process to ensure it's running.
                        // If we don't check for it, the plugin would end up launching it, which we don't want
                        Process[] iTunesProcesses = Process.GetProcessesByName("iTunes");

                        if (iTunesProcesses.Length > 0)
                        {
                            try
                            {

                                if (_isEnabled)
                                {
                                    iTunesApp iTunesApp = new iTunesApp();
                                    Console.WriteLine("iTunes Plugin successfully connected to iTunes COM library.");

                                    Console.WriteLine("iTunes Plugin checking media state...");
                                    MediaItem mediaDetail = await GetMediaDetail(iTunesApp);

                                    ITPlayerState playerState = iTunesApp?.PlayerState ?? ITPlayerState.ITPlayerStateStopped;
                                    double playerPosition = iTunesApp?.PlayerPosition ?? 0;

                                    bool hasMedia = mediaDetail != null;
                                    bool hasReachedTrackEnd = hasMedia && (int)playerPosition >= (int)mediaDetail?.TrackLength && mediaDetail?.TrackLength > 0;
                                    bool hasTrackChanged = _currentMediaItem?.TrackName != mediaDetail?.TrackName;
                                    bool isPlaying = playerState == ITPlayerState.ITPlayerStatePlaying;
                                    bool isPaused = playerState == ITPlayerState.ITPlayerStateStopped;

                                    bool canScrobble = _currentMediaPlayTime >= _minimumScrobbleSeconds && _currentMediaPlayTime == Math.Min(Convert.ToInt32(_currentMediaItem?.TrackLength) / 2, 4 * 60);

                                    Console.WriteLine($"iTunes Media Player Plugin: Position {playerPosition} of { mediaDetail?.TrackLength }, Tracker time: {_currentMediaPlayTime}...");

                                    if ((isPlaying && hasMedia && hasTrackChanged) || hasReachedTrackEnd)
                                    {
                                        _lastStatePaused = false;
                                        _currentMediaPlayTime = 1;

                                        if (_currentMediaItem != null)
                                        {
                                            _onTrackEnded?.Invoke(_currentMediaItem);

                                            _onScrobbleTrack?.Invoke(_currentMediaItem);
                                        }

                                        Console.WriteLine("Raising Track Change Method.");

                                        if (hasTrackChanged)
                                        {
                                            _currentMediaItem = mediaDetail;
                                            _onTrackStarted?.Invoke(mediaDetail, false);
                                            mediaDetail.StartedPlaying = DateTime.Now;
                                        }
                                        else if (hasReachedTrackEnd)
                                        {
                                            _currentMediaItem = null;
                                        }
                                    }
                                    else if (isPlaying && hasMedia && canScrobble)
                                    {
                                        _lastQueuedItem = mediaDetail;

                                        lock (_mediaLock)
                                        {
                                            _mediaToScrobble.Add(_currentMediaItem);
                                        }
                                        _currentMediaPlayTime++;

                                        Console.WriteLine($"Track {mediaDetail.TrackName} queued for Scrobbling.");
                                    }
                                    // The media player is playing, and is still playing the same track
                                    else if (isPlaying && !hasTrackChanged)
                                    {
                                        if (_lastStatePaused)
                                        {
                                            _onTrackStarted?.Invoke(_currentMediaItem, _lastStatePaused);
                                        }
                                        _currentMediaPlayTime++;
                                    }
                                    // The media player is not playing
                                    else if (!isPlaying)
                                    {
                                        // If we had been playing, invoke the Track Ended callback
                                        if (_currentMediaPlayTime > 0)
                                        {
                                            _onTrackEnded?.Invoke(mediaDetail);
                                        }

                                        // Set the persisted pause state
                                        _lastStatePaused = isPaused;

                                        // If we're not paused (FF, Rewind)
                                        if (!isPaused)
                                        {
                                            // Reset the state tracking how long we played this track for
                                            _lastStatePaused = false;
                                            _currentMediaPlayTime = 0;
                                        }
                                    }
                                    
                                    if (iTunesApp != null)
                                    {
                                        Marshal.ReleaseComObject(iTunesApp);
                                        iTunesApp = null;
                                    }
                                    System.GC.Collect();
                                }

                                Console.WriteLine("iTunes Plugin checking media state complete.");
                            }
                            catch (COMException cEx)
                            {
                                // Ignore the COM exception, the library is probably just tearing down.
                            }
                            catch (Exception)
                            {
                                // Some other exception occured, at some point consider logging it...?
                            }
                        }
                        else if (iTunesProcesses.Length == 0 && _currentMediaItem != null)
                        {
                            _onTrackEnded?.Invoke(_currentMediaItem);
                            _currentMediaItem = null;
                            Console.WriteLine("iTunes process not detected.  Waiting for iTunes process to start...");
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

        public void Dispose()
        {
            _scrobbleTimer?.Stop();
            _scrobbleTimer?.Dispose();
        }
    }
}
