
using Common.Classes;
using LastFM.ApiClient.Models;
using LastFM.Common.Factories;
using LastFM.Common.Helpers;
using LastFM.Common.Static_Classes;
using System;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using static LastFM.ApiClient.LastFMClient;

namespace LastFM.Common.Classes
{
    public partial class NotificationThread : Form
    {
        private bool _userExiting = false;
        private SettingsUi _settingsUI = null;
        private UserInfo _currentUser = null;

        private ApiClient.LastFMClient _apiClient = null;
        private MediaItem _currentMediaItem = null;

        protected UserInfo CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; }
        }

        protected ApiClient.LastFMClient APIClient
        {
            get { return _apiClient; }
            set { _apiClient = value; }
        }

        public NotificationThread()
        {
            InitializeComponent();

            this.Load += NotificationThread_Load;
        }

        private async void NotificationThread_Load(object sender, System.EventArgs e)
        {
            trayIcon.Visible = true;
            trayIcon.DoubleClick += TrayIcon_DoubleClick;
            trayIcon.BalloonTipClosed += ClearBallonTip;

            stripVersionLabel.Text = $"v{ApplicationUtility.GetApplicationVersionNumber()}";

            this.FormClosing += NotificationThread_FormClosing;
            this.Resize += NotificationThread_Resize;

            trayMenu.Opening += TrayMenu_Opening;

            mnuShow.Click += (o, ev) => 
            {
                ShowForm();
            };

            mnuPauseScrobbling.Click += (o, ev) => 
            {
                mnuPauseScrobbling.Checked = !mnuPauseScrobbling.Checked;
                ScrobbleFactory.ScrobblingEnabled = !mnuPauseScrobbling.Checked;
            };

            mnuShowSettings.Click += (o, ev) =>
            {
                ShowSettings();
            };

            mnuViewUserProfile.Click += (o, ev) =>
            {
                ViewUserProfile();
            };
            
            mnuExit.Click += (o, ev) =>
            {
                ScrobbleFactory.ScrobblingEnabled = false;
                ScrobbleFactory.Dispose();

                _userExiting = true;
                this.Close();
            };

            mnuLoveThisTrack.Click += (o, ev) =>
            {
                LoveorUnloveCurrentMedia();
            };

            ResetLoveTrackState(LoveStatus.Love);

            stripLoveTrack.Click += stripLoveTrack_Click;

            stripLoveTrack.MouseEnter += (o, ev) =>
            {
                statusStrip1.Cursor = stripLoveTrack.Enabled ? Cursors.Hand : Cursors.No;
            };

            stripLoveTrack.MouseLeave += (o, ev) => 
            {
                statusStrip1.Cursor = Cursors.Default;
            };
        }

        public void RefreshLoveTrackState()
        {
            LoveStatus currentState = (LoveStatus)stripLoveTrack.Tag;
            ResetLoveTrackState(currentState);
        }

        internal void ResetLoveTrackState(LoveStatus newState)
        {
            this.Invoke(new MethodInvoker(async () => { 

                stripLoveTrack.Text = string.Empty;
                stripLoveTrack.Tag = newState;

                stripLoveTrack.Enabled = _currentUser != null;

                switch(newState)
                {
                    case LoveStatus.Love:
                    {
                        stripLoveTrack.Image = await ImageHelper.LoadImage("Resources\\heart-empty.png");
                        break;
                    }
                    case LoveStatus.Unlove:
                    {
                        stripLoveTrack.Image = await ImageHelper.LoadImage("Resources\\heart-full.png");
                        break;
                    }
                }
            }));
        }

        private async void stripLoveTrack_Click(object sender, EventArgs e)
        {
            LoveorUnloveCurrentMedia();
        }

        private async void LoveorUnloveCurrentMedia()
        {
            LoveStatus statusToSend = (LoveStatus)stripLoveTrack.Tag;

            try
            {
                await _apiClient.LoveTrack(statusToSend, _currentMediaItem);
                ResetLoveTrackState((statusToSend == LoveStatus.Love) ? LoveStatus.Unlove : LoveStatus.Love);
            }
            catch (Exception)
            {
                // As current user is generally handled outside of this routine, we'll set it here
                _currentUser = null;

                // No connection to the API is available not a lot we can do about it...
                // (well not that is in scope for this phase of the project)
                ResetLoveTrackState(statusToSend);                
            }
        }

        private void ClearBallonTip(object sender, EventArgs e)
        {
            trayIcon.BalloonTipText = null;
            trayIcon.BalloonTipTitle = null;
            trayIcon.BalloonTipIcon = ToolTipIcon.None;
        }

        private void TrayMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string trackName = _currentMediaItem?.TrackName ?? "<unknown>";

            mnuShow.Enabled = this.Visible;

            mnuLoveThisTrack.Enabled = _currentMediaItem != null && _currentUser != null;

            if (_currentMediaItem != null)
            {
                if ((LoveStatus)stripLoveTrack.Tag == LoveStatus.Love)
                {
                    mnuLoveThisTrack.Text = $"Love '{trackName}'";
                }
                else if ((LoveStatus)stripLoveTrack.Tag == LoveStatus.Unlove)
            {
                    mnuLoveThisTrack.Text = $"Un-Love '{trackName}'";
                }
            }
            else
            {
                mnuLoveThisTrack.Text = "&Love this Track";
            }

            mnuPauseScrobbling.Checked = !ScrobbleFactory.ScrobblingEnabled;
            mnuViewUserProfile.Enabled = !string.IsNullOrEmpty(_currentUser?.Url);
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void NotificationThread_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                MinimizeToTray();
            }
        }

        private void NotificationThread_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((e.CloseReason == CloseReason.UserClosing && !Core.Settings.CloseToTray) || _userExiting)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
            else
            {
                e.Cancel = true;
                MinimizeToTray();
            }
        }

        private void MinimizeToTray()
        {
            this.ShowInTaskbar = false;
            this.Hide();
        }

        private void ShowForm()
        {
            this.TopMost = true;
            this.ShowInTaskbar = true;
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.TopMost = false;
        }

        public void SetStatus(string newStatus)
        {
            if (!this.IsDisposed && !this.Disposing)
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    if (stripStatus != null)
                    {
                        stripStatus.Text = newStatus;
                    }

                    trayIcon.Text = newStatus;
                }));
            }
        }

        protected void ShowSettings()
        {
            if (_settingsUI == null)
            {
                _settingsUI = new SettingsUi();
                _settingsUI.FormClosing += SettingsUi_FormClosing;
                _settingsUI.StartPosition = (this.WindowState == FormWindowState.Normal) ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;
            }

            bool previousScrobbleState = ScrobbleFactory.ScrobblingEnabled;

            ScrobbleFactory.ScrobblingEnabled = false;

            _settingsUI.ShowDialog(this);

            ScrobbleFactory.ScrobblingEnabled = previousScrobbleState;
        }

        private void SettingsUi_FormClosing(object sender, FormClosingEventArgs e)
        {
            _settingsUI = null;
        }

        protected void ViewUserProfile()
        {
            ProcessHelper.LaunchUrl(_currentUser.Url);
        }

        internal void DoBallonTip(ToolTipIcon icon, string title, string text)
        {            
            trayIcon.BalloonTipText = text;
            trayIcon.BalloonTipTitle = title;
            trayIcon.BalloonTipIcon = icon;
            trayIcon.ShowBalloonTip(3000);
        }

        internal void TrackChanged(MediaItem mediaItem)
        {
            _currentMediaItem = mediaItem;

            string trackName = _currentMediaItem?.TrackName ?? "<unknown>";
            string artistName = _currentMediaItem?.ArtistName ?? "<unknown>";

            if (Core.Settings.ShowTrackChanges)
            {
                string balloonText = $"The track '{trackName}' by '{artistName}' just started playing...";
                DoBallonTip(ToolTipIcon.Info, Core.APPLICATION_TITLE, balloonText);
            }


            ResetLoveTrackState(LoveStatus.Love);

            this.Invoke(new MethodInvoker(() =>
            {
                stripLoveTrack.Enabled = true;
            }));
        }
    }
}
