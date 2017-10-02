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

                        iTunesApp iTunesApp = null;

                        // Check for the iTunes process to ensure it's running.
                        // If we don't check for it, the plugin would end up launching it, which we don't want
                        Process[] iTunesProcesses = Process.GetProcessesByName("iTunes");

                        if (iTunesProcesses.Length > 0)
                        {
                            try
                            {
                                iTunesApp = new iTunesApp();
                                Console.WriteLine("iTunes Plugin successfully connected to iTunes COM library.");
                            }
                            catch (Exception)
                            {
                                // Ignore this, more than likely the application was in the process of closing while we tried to read from it
                            }
                        }
                        else if (iTunesProcesses.Length == 0 && _currentMediaItem != null)
                        {
                            _onTrackEnded(null);
                            _currentMediaItem = null;
                            Console.WriteLine("iTunes process not detected.  Waiting for iTunes process to start...");
                        }

                        if (iTunesApp != null)
                        {
                            Console.WriteLine("iTunes Plugin checking media state...");

                            if (_isEnabled)
                            {
                                MediaItem mediaDetail = await GetMediaDetail(iTunesApp);

                                if (mediaDetail != null && _mediaToScrobble.Count(mediaItem => mediaItem.TrackName == mediaDetail?.TrackName) == 0 && _currentMediaItem?.TrackName != mediaDetail?.TrackName && iTunesApp.PlayerState == ITPlayerState.ITPlayerStatePlaying)
                                {
                                    _currentMediaPlayTime = 1;

                                    if (_currentMediaItem != null)
                                    {
                                        _onTrackEnded?.Invoke(_currentMediaItem);
                                    }

                                    _currentMediaItem = mediaDetail;

                                    Console.WriteLine("Raising Track Change Method.");

                                    _onTrackStarted?.Invoke(mediaDetail);
                                    mediaDetail.StartedPlaying = DateTime.Now;
                                }
                                else if (iTunesApp.PlayerState != ITPlayerState.ITPlayerStatePlaying)
                                {
                                    if (_currentMediaPlayTime > 0)
                                    {
                                        _onTrackEnded?.Invoke(_currentMediaItem);
                                        _currentMediaItem = null;
                                    }
                                    _currentMediaPlayTime = 0;
                                }
                                else if (iTunesApp.PlayerState == ITPlayerState.ITPlayerStatePlaying && _currentMediaItem?.TrackName == mediaDetail?.TrackName)
                                {
                                    if (_currentMediaPlayTime == 0)
                                    {
                                        _onTrackStarted?.Invoke(_currentMediaItem);
                                    }
                                    _currentMediaPlayTime++;
                                }

                                if (_currentMediaItem != null)
                                {
                                    Console.WriteLine($"Current media playing time: {_currentMediaPlayTime} of {_currentMediaItem.TrackLength}.");

                                    if (mediaDetail != null && _mediaToScrobble.Count(item => item.TrackName == mediaDetail?.TrackName) == 0 && 
                                        _currentMediaPlayTime >= _minimumScrobbleSeconds && _currentMediaPlayTime >= _currentMediaItem.TrackLength / 2 &&
                                        mediaDetail?.TrackName != _lastQueuedItem?.TrackName)
                                    {
                                        _lastQueuedItem = mediaDetail;

                                        lock (_mediaLock)
                                        {
                                            _mediaToScrobble.Add(mediaDetail);
                                            Console.WriteLine($"Track {mediaDetail.TrackName} queued for Scrobbling.");
                                        }

                                        _onScrobbleTrack?.Invoke(mediaDetail);
                                    }
                                }
                            }

                            Console.WriteLine("iTunes Plugin checking media state complete.");
                        }
                        else if (_currentMediaItem != null)
                        {
                            _onTrackEnded?.Invoke(_currentMediaItem);
                            _currentMediaItem = null;
                        }

                        if (iTunesApp != null)
                        {
                            Marshal.ReleaseComObject(iTunesApp);
                        }

                        _scrobbleTimer.Start();
                    };                
            }
            catch (Exception ex)
            {
                _scrobbleTimer.Start();
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
