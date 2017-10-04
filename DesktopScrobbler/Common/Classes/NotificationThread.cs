
using Common.Classes;
using LastFM.ApiClient.Models;
using LastFM.Common.Factories;
using LastFM.Common.Helpers;
using LastFM.Common.Static_Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Reflection;
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

        private Icon _normalTrayIcon = null;
        private Icon _greyScaleIcon = null;

        private List<PopupNotificationUi> _notifications = new List<PopupNotificationUi>();

        public delegate void ScrobbleStateChanged(bool scrobblingEnabled);

        public ScrobbleStateChanged OnScrobbleStateChanged { get; set; }

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
            trayIcon.DoubleClick += TrayIcon_DoubleClick;
            trayIcon.BalloonTipClosed += ClearBallonTip;
            trayIcon.MouseClick += (o, ev) =>
            {
                if ((ev.Button & MouseButtons.Left) != 0)
                {
                    MethodInfo methodInfo = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                    methodInfo?.Invoke(trayIcon, null);
                }
            };

            stripVersionLabel.Text = $"v{ApplicationUtility.GetApplicationVersionNumber()}";

            this.FormClosing += NotificationThread_FormClosing;
            this.Resize += NotificationThread_Resize;

            trayMenu.Opening += TrayMenu_Opening;

            mnuShow.Click += (o, ev) => 
            {
                ShowForm();
            };

            mnuEnableScrobbling.Click += (o, ev) =>
            {
                mnuEnableScrobbling.Checked = !mnuEnableScrobbling.Checked;                

                ScrobbleStateChanging(mnuEnableScrobbling.Checked);
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
                ExitApplication();
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

            _normalTrayIcon = trayIcon.Icon;
            _greyScaleIcon = await ImageHelper.GreyScaleIcon(_normalTrayIcon);

        }

        protected void ScrobbleStateChanging(bool scrobblingEnabled)
        {
            ScrobbleFactory.ScrobblingEnabled = scrobblingEnabled;

            if (!scrobblingEnabled)
            {
                trayIcon.Icon = _greyScaleIcon;
            }
            else
            {
                trayIcon.Icon = _normalTrayIcon;
            }

            if (!scrobblingEnabled)
            {
                TrackChanged(null, false);
            }

            ShowScrobbleState();

            this.OnScrobbleStateChanged?.Invoke(scrobblingEnabled);
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

                stripLoveTrack.Enabled = _currentUser != null && _currentMediaItem != null;

                switch(newState)
                {
                    case LoveStatus.Love:
                    {
                        stripLoveTrack.Image = await ImageHelper.LoadImage("Resources\\love_off_64.png");
                        break;
                    }
                    case LoveStatus.Unlove:
                    {
                        stripLoveTrack.Image = await ImageHelper.LoadImage("Resources\\love_on_64.png");
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

            mnuShow.Enabled = this.WindowState == FormWindowState.Minimized || this.Visible == false;

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

            mnuEnableScrobbling.Checked = ScrobbleFactory.ScrobblingEnabled;
            mnuViewUserProfile.Enabled = !string.IsNullOrEmpty(_currentUser?.Url);            
        }

        public void ShowTrayIcon()
        {
            trayIcon.Visible = true;
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
                ScrobbleFactory.ScrobblingEnabled = false;
                ScrobbleFactory.Dispose();

                _settingsUI?.Close();

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

        internal void ShowScrobbleState()
        {
            if (ScrobbleFactory.ScrobblingEnabled)
            {
                SetStatus("Waiting to Scrobble...");
            }
            else
            {
                SetStatus("Scrobbling is paused.");
            }
        }

        public void SetStatus(string newStatus, bool setTrayText=true)
        {
            if (!this.IsDisposed && !this.Disposing)
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    if (stripStatus != null)
                    {
                        stripStatus.Text = newStatus;
                    }

                    if (setTrayText)
                    {
                        trayIcon.Text = newStatus;
                    }
                }));
            }
        }

        protected void ShowSettings()
        {
            bool isAlreadyLoaded = _settingsUI != null;

            if (!isAlreadyLoaded)
            {
                _settingsUI = new SettingsUi();
                _settingsUI.FormClosing += SettingsUi_FormClosing;
                _settingsUI.StartPosition = (this.WindowState == FormWindowState.Normal) ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;


                bool previousScrobbleState = ScrobbleFactory.ScrobblingEnabled;

                ScrobbleFactory.ScrobblingEnabled = false;

                _settingsUI.ShowDialog(this);

                ScrobbleFactory.ScrobblingEnabled = previousScrobbleState;
            }
            else
            {
                _settingsUI.BringToFront();
            }
        }

        public void HideTrayIcon()
        {
            trayIcon.Visible = false;
        }

        private void SettingsUi_FormClosing(object sender, FormClosingEventArgs e)
        {
            _settingsUI = null;
        }

        protected void ViewUserProfile()
        {
            ProcessHelper.LaunchUrl(_currentUser.Url);
        }

        internal void ShowNotification(string title, string text)
        {
            NotificationHelper.ShowNotification(this, title, text);
        }

        public virtual void TrackChanged(MediaItem mediaItem, bool wasResumed)
        {
            _currentMediaItem = mediaItem;

            ShowScrobbleState();

            if (_currentMediaItem != null && !wasResumed)
            {
                string trackName = _currentMediaItem?.TrackName ?? "<unknown>";
                string artistName = _currentMediaItem?.ArtistName ?? "<unknown>";

                if (Core.Settings.ShowTrackChanges)
                {
                    string initialText = $"Now playing: '{trackName}' by '{artistName}'";

                    if (initialText.Length > 120)
                    {
                        initialText = $"Now playing: '{trackName}'";
                    }

                    if (initialText.Length > 120)
                    {
                        initialText = $"{initialText.Substring(0, 117)} ...";
                    }

                    ShowNotification(Core.APPLICATION_TITLE, initialText);
                }
            }

            ResetLoveTrackState(LoveStatus.Love);
        }

        public void ExitApplication()
        {
            _userExiting = true;
            this.Close();
        }

    }
}
