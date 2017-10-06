
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
using System.Net;
using System.ComponentModel;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading.Tasks;
using LastFM.Common.Localization;

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

            VersionChecker.CleanUpDownloads();

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

            stripVersionLabel.Text = $"v{ApplicationUtility.GetApplicationFullVersionNumber()}";

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

            stripLoveTrack.MouseEnter += Common_MouseEnter;
            stripLoveTrack.MouseLeave += Common_MouseLeave;

            stripNewVersion.MouseEnter += Common_MouseEnter;
            stripNewVersion.MouseLeave += Common_MouseLeave;

            _normalTrayIcon = trayIcon.Icon;
            _greyScaleIcon = await ImageHelper.GreyScaleIcon(_normalTrayIcon).ConfigureAwait(false);

        }

        private async void Common_MouseEnter(object sender, EventArgs e)
        {
            statusStrip1.Cursor = ((ToolStripStatusLabel)sender).Enabled ? Cursors.Hand : Cursors.No;

            ToolStripLabel sendingLabel = sender as ToolStripStatusLabel;
            if (sendingLabel != null)
            {
                if (sendingLabel == stripLoveTrack)
                {
                    LoveStatus currentStatus = (LoveStatus)sendingLabel.Tag;
                    switch (currentStatus)
                    {
                        case LoveStatus.Love:
                        {
                            stripLoveTrack.Image = await ImageHelper.LoadImage("Resources\\love_off_hover_64.png").ConfigureAwait(false);
                            break;
                        }
                        case LoveStatus.Unlove:
                        {
                            stripLoveTrack.Image = await ImageHelper.LoadImage("Resources\\love_on_hover_64.png").ConfigureAwait(false);
                            break;
                        }
                    }
                }
            }
        }

        private async void Common_MouseLeave(object sender, EventArgs e)
        {
            statusStrip1.Cursor = Cursors.Default;

            ToolStripLabel sendingLabel = sender as ToolStripStatusLabel;
            if (sendingLabel != null)
            {
                if (sendingLabel == stripLoveTrack)
                {
                    LoveStatus currentStatus = (LoveStatus)sendingLabel.Tag;
                    switch (currentStatus)
                    {
                        case LoveStatus.Love:
                            {
                                stripLoveTrack.Image = await ImageHelper.LoadImage("Resources\\love_off_64.png").ConfigureAwait(false);
                                break;
                            }
                        case LoveStatus.Unlove:
                            {
                                stripLoveTrack.Image = await ImageHelper.LoadImage("Resources\\love_on_64.png").ConfigureAwait(false);
                                break;
                            }
                    }
                }
            }
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

                stripLoveTrack.Enabled = _currentUser != null && _currentMediaItem != null && _currentMediaItem.TrackLength > 0;

                switch(newState)
                {
                    case LoveStatus.Love:
                    {
                        stripLoveTrack.Image = await ImageHelper.LoadImage("Resources\\love_off_64.png").ConfigureAwait(false);
                        break;
                    }
                    case LoveStatus.Unlove:
                    {
                        stripLoveTrack.Image = await ImageHelper.LoadImage("Resources\\love_on_64.png").ConfigureAwait(false);
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
                await _apiClient.LoveTrack(statusToSend, _currentMediaItem).ConfigureAwait(false);
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
                    mnuLoveThisTrack.Text = string.Format(LocalizationStrings.NotificationThread_TrayMenu_LoveTrack, trackName);
                }
                else if ((LoveStatus)stripLoveTrack.Tag == LoveStatus.Unlove)
            {
                    mnuLoveThisTrack.Text = string.Format(LocalizationStrings.NotificationThread_TrayMenu_Un_Love, trackName);
                }
            }
            else
            {
                mnuLoveThisTrack.Text = LocalizationStrings.NotificationThread_TrayMenu_Love_this_Track;
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

        private async void NotificationThread_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool canCloseApp = (e.CloseReason == CloseReason.UserClosing && !Core.Settings.CloseToTray) || _userExiting || e.CloseReason != CloseReason.UserClosing;

            if (canCloseApp)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();

                NotificationHelper.ClearNotifications();

                ScrobbleFactory.ScrobblingEnabled = false;

                // Give the scrobblers a chance to stop running cleanly
                await Task.Delay(2000).ConfigureAwait(false);

                ScrobbleFactory.Dispose();

                _settingsUI?.Close();
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
                SetStatus(LocalizationStrings.NotificationThread_Status_WaitingToScrobble);
            }
            else
            {
                SetStatus(LocalizationStrings.NotificationThread_Status_ScrobblingPaused);
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
            if (this.Height == 164)
            {
                this.Height = 316;
            }
            else
            {
                this.Height = 164;
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
                if (Core.Settings.ShowTrackChanges)
                {
                    string notificationText = string.Format(LocalizationStrings.PopupNotifications_TrackChanged, MediaHelper.GetTrackDescription(_currentMediaItem));

                    ShowNotification(Core.APPLICATION_TITLE, notificationText);
                }
            }

            ResetLoveTrackState(LoveStatus.Love);
        }

        public void HasNewVersion(VersionChecker.VersionState versionDetail)
        {
            this.Invoke(new MethodInvoker(() => {

                stripNewVersion.Tag = versionDetail;
                stripNewVersion.Visible = true;

                mnuNewVersion.Text = string.Format(LocalizationStrings.NotificationThread_TrayMenu_DownloadNewVersion, versionDetail.Version);
                mnuNewVersion.Visible = true;
                mnuNewVersionSeparator.Visible = true;

                ResetVersionMenuClickHandlers();

                mnuNewVersion.Click += DownloadNewVersion;
                stripNewVersion.Click += DownloadNewVersion;

                NotificationHelper.ShowNotification(this, Core.APPLICATION_TITLE, string.Format(LocalizationStrings.PopupNotifications_NewVersionAvailable, versionDetail.Version));
            }));
        }

        private void ResetVersionMenuClickHandlers()
        {
            mnuNewVersion.Click -= DownloadNewVersion;
            mnuNewVersion.Click -= InstallUpdate;
            stripNewVersion.Click -= DownloadNewVersion;
            stripNewVersion.Click -= InstallUpdate;
            stripUpdateProgress.Click -= InstallUpdate;

            stripUpdateProgress.MouseEnter -= Common_MouseEnter;
            stripUpdateProgress.MouseLeave -= Common_MouseLeave;

        }

        public async void DownloadNewVersion(object sender, EventArgs e)
        {
            mnuNewVersion.Enabled = false;
            stripUpdateProgress.Visible = true;
            stripUpdateProgress.AutoSize = true;

            await VersionChecker.DownloadUpdate(stripNewVersion.Tag as VersionChecker.VersionState, Core.UserDownloadsPath, DownloadProgressUpdated, DownloadComplete).ConfigureAwait(false);
        }

        private void DownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            if(e.Error != null)
            {
                mnuNewVersion.Enabled = true;
                MessageBox.Show(this, string.Format(LocalizationStrings.NotificationThread_FailedToDownload, e.Error.Message), $"{Core.APPLICATION_TITLE}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                VersionChecker.VersionState downloadedVersionInfo = e.UserState as VersionChecker.VersionState;

                if (downloadedVersionInfo != null)
                {
                    ResetVersionMenuClickHandlers();

                    stripUpdateProgress.Text = LocalizationStrings.NotificationThread_Status_ReadyToInstall;

                    mnuNewVersion.Text = string.Format(LocalizationStrings.NotificationThread_TrayMenu_InstallNewVersion, downloadedVersionInfo.Version);
                    mnuNewVersion.Enabled = true;
                    mnuNewVersion.Click += InstallUpdate;
                    stripNewVersion.Click += InstallUpdate;
                    stripUpdateProgress.Click += InstallUpdate;

                    stripUpdateProgress.MouseEnter += Common_MouseEnter;
                    stripUpdateProgress.MouseLeave += Common_MouseLeave;

                }
            }
        }

        private void InstallUpdate(object sender, EventArgs e)
        {
            VersionChecker.VersionState updateInfo = stripNewVersion.Tag as VersionChecker.VersionState;

            if (updateInfo != null)
            {
                string downloadOSFilename = new Uri(updateInfo.Url).PathAndQuery.Replace('/', Path.DirectorySeparatorChar);
                FileInfo downloadFileInfo = new FileInfo(downloadOSFilename);

                FileInfo downloadedFile = new FileInfo($"{Core.UserDownloadsPath}{downloadFileInfo.Name}");
                string extractionPath = downloadedFile.FullName.Substring(0, downloadedFile.FullName.IndexOf(downloadedFile.Extension));

                try
                {
                    if (downloadedFile.Exists)
                    {
                        using (ZipInputStream s = new ZipInputStream(File.OpenRead(downloadedFile.FullName)))
                        {
                            ZipEntry theEntry;
                            while ((theEntry = s.GetNextEntry()) != null)
                            {

                                string directoryName = extractionPath;
                                string fileName = Path.GetFileName(theEntry.Name);

                                // create directory 
                                if (directoryName.Length > 0)
                                {
                                    if (!Directory.Exists(directoryName))
                                    {
                                        Directory.CreateDirectory(directoryName);
                                    }
                                }

                                if (fileName != String.Empty)
                                {
                                    using (FileStream streamWriter = File.Create(string.Format("{0}\\{1}", directoryName, theEntry.Name)))
                                    {
                                        int size = 2048;
                                        byte[] data = new byte[2048];
                                        while (true)
                                        {
                                            size = s.Read(data, 0, data.Length);
                                            if (size > 0)
                                            {
                                                streamWriter.Write(data, 0, size);
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (File.Exists(extractionPath + "\\setup.exe"))
                        {
                            ProcessHelper.LaunchProcess($"{extractionPath}\\setup.exe");
                            ExitApplication();
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }        
        }

        private void DownloadProgressUpdated(object sender, DownloadProgressChangedEventArgs e)
        {
            mnuNewVersion.Text = string.Format(LocalizationStrings.NotificationThread_DownloadProgressUpdated, e.ProgressPercentage);
            stripUpdateProgress.Text = string.Format(LocalizationStrings.NotificationThread_DownloadProgressUpdated, e.ProgressPercentage);
        }

        public async Task ExitApplication()
        {
            _userExiting = true;
            this.Close();
        }
       
    }
}
