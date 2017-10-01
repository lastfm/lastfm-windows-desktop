
using Common.Classes;
using LastFM.ApiClient.Models;
using LastFM.Common.Factories;
using LastFM.Common.Helpers;
using LastFM.Common.Static_Classes;
using System;
using System.Windows.Forms;

namespace LastFM.Common.Classes
{
    public partial class NotificationThread: Form
    {
        private bool _userExiting = false;
        private SettingsUi _settingsUI = null;
        private UserInfo _currentUser = null;

        public UserInfo CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; }
        }

        public NotificationThread()
        {
            InitializeComponent();

            this.Load += NotificationThread_Load;
        }

        private void NotificationThread_Load(object sender, System.EventArgs e)
        {
            trayIcon.Visible = true;
            trayIcon.DoubleClick += TrayIcon_DoubleClick;
            trayIcon.BalloonTipClosed += ClearBallonTip;

            stripVersionLabel.Text = $"v{ApplicationUtility.GetApplicationVersionNumber()}";

            this.FormClosing += NotificationThread_FormClosing;
            this.Resize += NotificationThread_Resize;

            trayMenu.Opening += TrayMenu_Opening;

            mnuShow.Click += (o, ev) => { ShowForm(); };
            mnuPauseScrobbling.Click += (o, ev) => {

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
        }

        private void ClearBallonTip(object sender, EventArgs e)
        {
            trayIcon.BalloonTipText = null;
            trayIcon.BalloonTipTitle = null;
            trayIcon.BalloonTipIcon = ToolTipIcon.None;
        }

        private void TrayMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mnuShow.Enabled = this.Visible;
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
            this.Invoke(new MethodInvoker(() =>
            {
                if (stripStatus != null)
                {
                    stripStatus.Text = newStatus;
                }

                trayIcon.Text = newStatus;
            }));
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

        public void DoBallonTip(ToolTipIcon icon, string title, string text)
        {            
            trayIcon.BalloonTipText = text;
            trayIcon.BalloonTipTitle = title;
            trayIcon.BalloonTipIcon = icon;
            trayIcon.ShowBalloonTip(3000);
        }
    }
}
