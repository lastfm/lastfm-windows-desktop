using LastFM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private iTunesApp _iTunesApp = null;

        private MediaItem _currentMediaItem = null;
        private List<MediaItem> _mediaToScrobble = new List<MediaItem>();

        private object _mediaLock = new object();

        private int _minimumScrobbleSeconds = 30;
        private int _playerPosition = 0;

        private bool _isIntialized = false;
        private bool _isEnabled = false;
        private Timer _scrobbleTimer = null;

        private TrackStarted _onTrackStarted = null;
        private TrackEnded _onTrackEnded = null;

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
                    _scrobbleTimer.Start();
                }
                else
                {
                    _scrobbleTimer.Stop();
                }
            }
        }

        public List<MediaItem> MediaToScrobble
        {
            get
            {
                return _mediaToScrobble;
            }

            set
            {
                _mediaToScrobble = value;
            }
        }

        public void InitializeSource(int minimumScrobbleSeconds, TrackStarted onTrackStartedCallback, TrackEnded onTrackEndedCallback)
        {
            _minimumScrobbleSeconds = minimumScrobbleSeconds;

            _onTrackStarted = onTrackStartedCallback;
            _onTrackEnded = onTrackEndedCallback;
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

                        if (iTunesProcesses.Length > 0 && _iTunesApp == null)
                        {
                            _iTunesApp = new iTunesApp();
                            Console.WriteLine("iTunes Plugin successfully connected to iTunes COM library.");
                        }
                        else if (iTunesProcesses.Length == 0 && _iTunesApp != null)
                        {
                            _iTunesApp = null;
                            Console.WriteLine("iTunes process not detected.  Waiting for iTunes process to start...");
                        }

                        if (_iTunesApp != null)
                        {
                            Console.WriteLine("iTunes Plugin checking media state...");

                            if (_isEnabled)
                            {
                                MediaItem mediaDetail = await GetMediaDetail();

                                if (mediaDetail != null && _mediaToScrobble.Count(mediaItem => mediaItem.TrackName == mediaDetail?.TrackName) == 0 && _currentMediaItem?.TrackName != mediaDetail?.TrackName)
                                {
                                    if (_currentMediaItem != null)
                                    {
                                        _onTrackEnded?.Invoke(_currentMediaItem);
                                    }

                                    _currentMediaItem = mediaDetail;

                                    Console.WriteLine("Raising Track Change Method.");

                                    _onTrackStarted?.Invoke(mediaDetail);
                                }

                                if (_currentMediaItem != null)
                                {
                                    Console.WriteLine($"Player position {_playerPosition} of {_currentMediaItem.TrackLength}.");

                                    if (mediaDetail != null && _mediaToScrobble.Count(item => item.TrackName == mediaDetail?.TrackName) == 0 && _playerPosition >= _minimumScrobbleSeconds && _playerPosition >= _currentMediaItem.TrackLength / 2)
                                    {
                                        lock (_mediaLock)
                                        {
                                            _mediaToScrobble.Add(mediaDetail);
                                            Console.WriteLine($"Track {mediaDetail.TrackName} queued for Scrobbling.");
                                        }
                                    }
                                }
                            }

                            Console.WriteLine("iTunes Plugin checking media state complete.");
                        }

                        _scrobbleTimer.Start();
                    };                
            }
            catch (Exception ex)
            {
                _scrobbleTimer.Start();
            }
        }

        private async Task<MediaItem> GetMediaDetail()
        {
            MediaItem iTunesMediaDetail = null;

            try
            {
                iTunesMediaDetail = new MediaItem() { TrackName = _iTunesApp?.CurrentTrack?.Name, AlbumName = _iTunesApp?.CurrentTrack?.Album, ArtistName = _iTunesApp?.CurrentTrack?.Artist, TrackLength = Convert.ToDouble(_iTunesApp?.CurrentTrack?.Duration) };

                if (iTunesMediaDetail.TrackName != null)
                {
                    _playerPosition = Convert.ToInt32(_iTunesApp?.PlayerPosition);
                }
                else
                {
                    _playerPosition = 0;
                }
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

            if (_iTunesApp != null)
            {
                Marshal.ReleaseComObject(_iTunesApp);
            }
        }
    }
}
