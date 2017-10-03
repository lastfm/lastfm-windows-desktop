using LastFM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static LastFM.Common.Factories.ScrobbleFactory;
using LastFM.ApiClient.Models;
using System.Diagnostics;
using System.Windows.Forms;
using WMPLib;

namespace DesktopScrobbler
{
    public class WindowsMediaScrobbleSource : IScrobbleSource
    {
        private MediaItem _currentMediaItem = null;
        private MediaItem _lastQueuedItem = null;

        private List<MediaItem> _mediaToScrobble = new List<MediaItem>();

        private object _mediaLock = new object();

        private int _minimumScrobbleSeconds = 30;
        private int _currentMediaPlayTime = 0;
        private bool _lastStatePaused = false;

        private WMPLib.WMPPlayState _currentPlaystate = WMPLib.WMPPlayState.wmppsWaiting;

        private bool _isIntialized = false;
        private bool _isEnabled = false;
        private System.Timers.Timer _scrobbleTimer = null;

        private TrackStarted _onTrackStarted = null;
        private TrackEnded _onTrackEnded = null;
        private ScrobbleTrack _onScrobbleTrack = null;

        private WindowsMediaPlayer _mediaPlayer = null;

        public WindowsMediaScrobbleSource() {
        }

        public WindowsMediaScrobbleSource(Form mediaPlayerHost)
        {
            _mediaPlayer = mediaPlayerHost as WindowsMediaPlayer;
        }

        public Guid SourceIdentifier
        {
            get
            {
                return new Guid("7471fa52-0007-43c9-a644-945fbc7f5897");
            }
        }

        public string SourceDescription
        {
            get
            {
                return "Windows Media Player";
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
                _scrobbleTimer = new System.Timers.Timer();
                _scrobbleTimer.Interval = 1000;

                    _scrobbleTimer.Elapsed += async (o, e) =>
                    {
                        _scrobbleTimer.Stop();

                        // Check for the iTunes process to ensure it's running.
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
                            Console.WriteLine("Windows Media Player Plugin checking media state...");

                            if (_isEnabled)
                            {
                                MediaItem mediaDetail = await GetMediaDetail();

                                WMPPlayState playerState = _mediaPlayer.Player.playState;
                                double playerPosition = _mediaPlayer.Player.Ctlcontrols.currentPosition;

                                bool hasMedia = mediaDetail != null;
                                bool hasReachedTrackEnd = hasMedia && (int)playerPosition >= (int)mediaDetail.TrackLength;
                                bool hasTrackChanged = _currentMediaItem?.TrackName != mediaDetail?.TrackName;
                                bool isNotAlreadyQueued = _mediaToScrobble.Count(mediaItem => mediaItem.TrackName == mediaDetail?.TrackName) == 0;
                                bool isPlaying = playerState == WMPPlayState.wmppsPlaying;
                                bool isPaused = playerState == WMPPlayState.wmppsPaused;

                                bool canScrobble = _currentMediaPlayTime >= _minimumScrobbleSeconds && _currentMediaPlayTime >= Math.Min(_currentMediaItem.TrackLength / 2, 4 * 60);
                                bool isNotLastQueuedItem = mediaDetail?.TrackName != _lastQueuedItem?.TrackName;


                                Console.WriteLine($"Windows Media Player Plugin: Position {playerPosition} of { mediaDetail.TrackLength }, Tracker time: {_currentMediaPlayTime}...");

                                if ((isPlaying && hasMedia && isNotAlreadyQueued && hasTrackChanged) || hasReachedTrackEnd)
                                {
                                    _lastStatePaused = false;
                                    _currentMediaPlayTime = 1;

                                    if (_currentMediaItem != null)
                                    {
                                        _onTrackEnded?.Invoke(_currentMediaItem);

                                        _onScrobbleTrack?.Invoke(_currentMediaItem);
                                    }

                                    Console.WriteLine("Raising Track Change Method.");

                                    if(mediaDetail?.TrackName != _currentMediaItem?.TrackName)
                                    {
                                        _currentMediaItem = mediaDetail;
                                        _onTrackStarted?.Invoke(mediaDetail, false);
                                        mediaDetail.StartedPlaying = DateTime.Now;
                                    }
                                }
                                else if (isPlaying && hasMedia && isNotAlreadyQueued && isNotLastQueuedItem && canScrobble)
                                {
                                    _lastQueuedItem = mediaDetail;

                                    lock (_mediaLock)
                                    {
                                        _mediaToScrobble.Add(_currentMediaItem);
                                    }

                                    Console.WriteLine($"Track {mediaDetail.TrackName} queued for Scrobbling.");
                                }
                                // The media player is playing, and is still playing the same track
                                else if (isPlaying && !hasTrackChanged)
                                {
                                    if (_currentMediaPlayTime == 0 || _lastStatePaused)
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
                                        _onTrackEnded.Invoke(mediaDetail);
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
                            }

                            Console.WriteLine("Windows Media Plugin checking media state complete.");
                        }
                        else if (_currentMediaItem != null)
                        {
                            _onTrackEnded?.Invoke(_currentMediaItem);
                            _currentMediaItem = null;
                        }

                        _scrobbleTimer.Start();
                    };                
            }
            catch (Exception ex)
            {
                _scrobbleTimer.Start();
            }
        }

        [STAThread]
        private async Task<MediaItem> GetMediaDetail()
        {
            MediaItem playerMedia = null;

            try
            {
                var currentMedia = _mediaPlayer?.Player?.Ctlcontrols?.currentItem;

                _currentPlaystate = _mediaPlayer?.Player?.playState != null ? (WMPLib.WMPPlayState)_mediaPlayer?.Player?.playState : WMPLib.WMPPlayState.wmppsWaiting;

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

        public void Dispose()
        {
            _scrobbleTimer?.Stop();
            _scrobbleTimer?.Dispose();

            if (_mediaPlayer != null)
            {
                _mediaPlayer.Close();
                _mediaPlayer = null;
            }
        }
    }
}
