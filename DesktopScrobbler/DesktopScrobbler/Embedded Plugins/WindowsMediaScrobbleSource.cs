using LastFM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime.InteropServices;
using static LastFM.Common.Factories.ScrobbleFactory;
using LastFM.ApiClient.Models;
using System.Diagnostics;
using DesktopScrobbler;
using System.Windows.Forms;

namespace DesktopScrobbler
{
    public class WindowsMediaScrobbleSource : IScrobbleSource
    {
        private MediaItem _currentMediaItem = null;
        private MediaItem _lastQueuedItem = null;

        private List<MediaItem> _mediaToScrobble = new List<MediaItem>();

        private object _mediaLock = new object();

        private int _minimumScrobbleSeconds = 30;
        private int _playerPosition = 0;

        private bool _isIntialized = false;
        private bool _isEnabled = false;
        private System.Timers.Timer _scrobbleTimer = null;

        private TrackStarted _onTrackStarted = null;
        private TrackEnded _onTrackEnded = null;

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
                    _scrobbleTimer.Start();
                }
                else
                {
                    _scrobbleTimer.Stop();
                }
            }
        }

        public void ClearQueuedMedia()
        {
            _scrobbleTimer.Stop();

            lock (_mediaLock)
            {
                _mediaToScrobble.Clear();
            }

            _scrobbleTimer.Start();
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

        public void InitializeSource(int minimumScrobbleSeconds, TrackStarted onTrackStartedCallback, TrackEnded onTrackEndedCallback)
        {
            _minimumScrobbleSeconds = minimumScrobbleSeconds;

            _onTrackStarted = onTrackStartedCallback;
            _onTrackEnded = onTrackEndedCallback;
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

                                if (mediaDetail != null && _mediaToScrobble.Count(mediaItem => mediaItem.TrackName == mediaDetail?.TrackName) == 0 && _currentMediaItem?.TrackName != mediaDetail?.TrackName)
                                {
                                    if (_currentMediaItem != null)
                                    {
                                        _onTrackEnded?.Invoke(_currentMediaItem);
                                    }

                                    _currentMediaItem = mediaDetail;

                                    Console.WriteLine("Raising Track Change Method.");

                                    _onTrackStarted?.Invoke(mediaDetail);
                                    mediaDetail.StartedPlaying = DateTime.Now;

                                }

                                if (_currentMediaItem != null)
                                {
                                    Console.WriteLine($"Player position {_playerPosition} of {_currentMediaItem.TrackLength}.");

                                    if (mediaDetail != null && _mediaToScrobble.Count(item => item.TrackName == mediaDetail?.TrackName) == 0 && 
                                        _playerPosition >= _minimumScrobbleSeconds && _playerPosition >= _currentMediaItem.TrackLength / 2 &&
                                        mediaDetail?.TrackName != _lastQueuedItem?.TrackName)
                                    {
                                        _lastQueuedItem = mediaDetail;

                                        lock (_mediaLock)
                                        {
                                            _mediaToScrobble.Add(mediaDetail);
                                            Console.WriteLine($"Track {mediaDetail.TrackName} queued for Scrobbling.");
                                        }
                                    }
                                }
                            }

                            Console.WriteLine("Windows Media Plugin checking media state complete.");
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

                if (currentMedia != null)
                {
                    playerMedia = new MediaItem() { TrackName = currentMedia?.getItemInfo("Title"), AlbumName = currentMedia?.getItemInfo("Album"), ArtistName = currentMedia?.getItemInfo("Artist"), TrackLength = Convert.ToDouble(currentMedia?.duration) };

                    if (playerMedia.TrackName != null)
                    {
                        _playerPosition = Convert.ToInt32(_mediaPlayer?.Player?.Ctlcontrols?.currentPosition);
                    }
                }
                else
                {
                    _playerPosition = 0;
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
                Marshal.ReleaseComObject(_mediaPlayer);
            }
        }
    }
}
