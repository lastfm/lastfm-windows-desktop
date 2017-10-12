
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
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading.Tasks;
using LastFM.Common.Localization;

namespace LastFM.Common.Classes
{
    /// <summary>
    /// The base class for the User Interface, which provides all the methods
    /// to react to the Scrobble Factory
    /// </summary>
    public partial class NotificationThread : Form
    {
        // Whether the reason the form is closing, is because the user has exited
        private bool _userExiting = false;

        // An instance of the settings Ui, no longer in use since
        // the settings were merged into the Scrobbler Ui (ScrobblerUi.cs)
        [Obsolete]
        private SettingsUi _settingsUI = null;

        // Details of the currently signed in user
        private UserInfo _currentUser = null;

        // A dedicated client for communicating with the Last.fm API
        private ApiClient.LastFMClient _apiClient = null;

        // The current media item being tracked (irrespective of source)
        private MediaItem _currentMediaItem = null;

        // The default Last.fm Tray Icon (colour version)
        private Icon _normalTrayIcon = null;

        // The greyscale version of the Tray Icon (when scrobbling is disabled)
        private Icon _greyScaleIcon = null;

        // A list of the notifications currently being displayed
        private List<PopupNotificationUi> _notifications = new List<PopupNotificationUi>();

        // Method definition for when the Scrobbling state is changed
        public delegate void ScrobbleStateChanged(bool scrobblingEnabled);

        // An instance of the Scrobbling state changing method
        public ScrobbleStateChanged OnScrobbleStateChanged { get; set; }

        // The height of the status bar (obsolete since the Last.fm designers implemented Ui changes)
        [Obsolete]
        protected internal int StatusBarHeight => Convert.ToInt32(statusStrip1?.Height);

        // Exposure of the user exposed to anything that might need to know about it
        protected UserInfo CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; }
        }

        // Exposure of the API Client to anything that might need to know about it
        protected ApiClient.LastFMClient APIClient
        {
            get { return _apiClient; }
            set { _apiClient = value; }
        }

        // Default constructor for the Ui
        public NotificationThread()
        {
            InitializeComponent();

            this.Load += NotificationThread_Load;
        }

        // Clean out the downloads folder (in case an install was recently done, or ignored.
        // Configure the appropriate Ui elements
        private async void NotificationThread_Load(object sender, System.EventArgs e)
        {
            VersionChecker.CleanUpDownloads();

            trayIcon.DoubleClick += TrayIcon_DoubleClick;
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

        // A common handler for various Ui elements when the mouse cursor enters them
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

        // A common handler for various Ui elements when the mouse cursor leaves them
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

        internal void TrackMonitoringEnded(MediaItem mediaItem)
        {
            Console.WriteLine($"Stopped monitoring: {mediaItem?.TrackName}");

            _currentMediaItem = null;

            ShowScrobbleState();

            RefreshMenuState(null);
        }

        // Method raised when the Scrobble State changes
        protected void ScrobbleStateChanging(bool scrobblingEnabled)
        {
            bool actualStateAllowed = scrobblingEnabled;

            // If scrobbling is being enabled, validate that it can be enabled
            if (scrobblingEnabled && Core.Settings.ScrobblerStatus.Count(plugin => plugin.IsEnabled) == 0)
            {
                actualStateAllowed = false;
            }

            ScrobbleFactory.ScrobblingEnabled = actualStateAllowed;

            if (!actualStateAllowed)
            {
                TrackMonitoringStarted(null, false);
            }

            ShowScrobbleState();

            this.OnScrobbleStateChanged?.BeginInvoke(scrobblingEnabled, null, null);
        }

        // Method raised when the Ui is notified of 'Love Track' changes
        public void RefreshLoveTrackState()
        {
            LoveStatus currentState = (LoveStatus)stripLoveTrack.Tag;
            ResetLoveTrackState(currentState);
        }

        // Method to set the 'Love track' Ui state to a specific state
        internal void ResetLoveTrackState(LoveStatus newState)
        {
            this.BeginInvoke(new MethodInvoker(async () => { 

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

            RefreshMenuState(_currentMediaItem);
        }

        // Handler for the status strip's 'Love track' icon (obsolete)
        [Obsolete]
        private async void stripLoveTrack_Click(object sender, EventArgs e)
        {
            LoveorUnloveCurrentMedia();
        }

        // Method for sending the Love or Unlove status to the Last.fm API
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

        // Method for handling when the user clicks on the Tray Icon to open the context menu
        // used to set the state of the relevant menu items
        private void TrayMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mnuShow.Enabled = this.WindowState == FormWindowState.Minimized || this.Visible == false;
            mnuEnableScrobbling.Checked = ScrobbleFactory.ScrobblingEnabled;
            mnuViewUserProfile.Enabled = !string.IsNullOrEmpty(_currentUser?.Url);            
        }

        // Displays the tray icon
        public void ShowTrayIcon()
        {
            trayIcon.Visible = true;
        }

        // Handler for when the user double clicks on the tray icon
        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowForm();
        }

        // Handler form when the Window state changes, to hide it from the Task bar and keep things tidy
        private void NotificationThread_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                MinimizeToTray();
            }
        }

        // Handler for when the form is being closed
        private async void NotificationThread_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If the (obsolete) setting Close to tray is turned off, or something other than the user
            // requested that this Ui should close, then allow it to close and terminate the application
            bool canCloseApp = (e.CloseReason == CloseReason.UserClosing && !Core.Settings.CloseToTray) || _userExiting || e.CloseReason != CloseReason.UserClosing;

            if (canCloseApp)
            {
                // Remove the icon from the tray
                trayIcon.Visible = false;
                trayIcon.Dispose();

                // Unload any notifications that might be displayed
                NotificationHelper.ClearNotifications();

                // Stop any Scrobble related functions
                ScrobbleFactory.ScrobblingEnabled = false;

                // Give the scrobblers a chance to stop running cleanly
                await Task.Delay(2000).ConfigureAwait(false);

                ScrobbleFactory.Dispose();

                // Close any instance of the (obsolete) settings form
                _settingsUI?.Close();
            }
            else
            {
                // Close / Minimize to tray is turned on (default)
                // so cancel the disposal of the Ui, and hide it from the taskbar
                e.Cancel = true;
                MinimizeToTray();
            }
        }

        // Hides the form from the taskbar, and removes it from view
        private void MinimizeToTray()
        {
            this.ShowInTaskbar = false;
            this.Hide();
        }

        // Displays the Ui, making sure it temporarily sits on top
        // resetting the state back to normal if the user last minimized it
        private void ShowForm()
        {
            this.TopMost = true;
            this.ShowInTaskbar = true;
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.TopMost = false;
        }

        // Update the Ui to show the current Scrobbling state
        public void ShowScrobbleState()
        {
            if (ScrobbleFactory.ScrobblingEnabled)
            {
                SetStatus(LocalizationStrings.NotificationThread_Status_WaitingToScrobble);
                trayIcon.Icon = _normalTrayIcon;
            }
            else
            {
                SetStatus(LocalizationStrings.NotificationThread_Status_ScrobblingPaused);
                trayIcon.Icon = _greyScaleIcon;
            }
        }

        // Update the (obsolete) status bar, and menu with the current Scrobble state
        public void SetStatus(string newStatus, bool setTrayText=true)
        {
            if (!this.IsDisposed && !this.Disposing)
            {
                this.BeginInvoke(new MethodInvoker(() =>
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

        // Removes the tray icon from the task bar
        public void HideTrayIcon()
        {
            trayIcon.Visible = false;
        }

        // Handler for when the user clicks to view their profile, either from the tray menu,
        // or from the obsolete LinkLabel on the Scrobbler Ui
        protected void ViewUserProfile()
        {
            ProcessHelper.LaunchUrl(_currentUser.Url);
        }

        // Method for displaying a notification on the screen
        internal void ShowNotification(string title, string text)
        {
            NotificationHelper.ShowNotification(this, title, text);
        }

        // Base method for handling when a Scrobbler source (plugin) starts monitoring (new) media
        public virtual void TrackMonitoringStarted(MediaItem mediaItem, bool wasResumed)
        {
            Console.WriteLine($"Started monitoring: {mediaItem?.TrackName}");

            bool raiseNotification = mediaItem?.TrackName != _currentMediaItem?.TrackName || _currentMediaItem == null || !wasResumed;

            ShowScrobbleState();

            RefreshMenuState(mediaItem);

            if (mediaItem != null && raiseNotification)
            {
                _currentMediaItem = mediaItem;

                if (Core.Settings.ShowNotifications && (Core.Settings.ShowTrackChanges == null || Convert.ToBoolean(Core.Settings.ShowTrackChanges)))
                {
                    string notificationText = string.Format(LocalizationStrings.PopupNotifications_TrackChanged, MediaHelper.GetTrackDescription(_currentMediaItem));

                    ShowNotification(Core.APPLICATION_TITLE, notificationText);
                }
            }

            ResetLoveTrackState(LoveStatus.Love);
        }

        private void RefreshMenuState(MediaItem mediaItem)
        {
            if (mediaItem != null)
            {
                this.BeginInvoke(new MethodInvoker(() =>
                {
                    mnuNowPlaying.Text = string.Format(LocalizationStrings.ScrobblerUi_CurrentTrack, MediaHelper.GetTrackDescription(mediaItem));
                    if(mnuNowPlaying.Text.Length >= 64)
                    {
                        trayIcon.Text = mnuNowPlaying.Text.Substring(0, 62) + '…';
                    } else
                    {
                        trayIcon.Text = mnuNowPlaying.Text;
                    }

                    if ((LoveStatus)stripLoveTrack.Tag == LoveStatus.Love)
                    {
                        mnuLoveThisTrack.Text = string.Format(LocalizationStrings.NotificationThread_TrayMenu_LoveTrack, MediaHelper.GetTrackDescription(mediaItem));
                    }
                    else if ((LoveStatus)stripLoveTrack.Tag == LoveStatus.Unlove)
                    {
                        mnuLoveThisTrack.Text = string.Format(LocalizationStrings.NotificationThread_TrayMenu_Un_Love, MediaHelper.GetTrackDescription(mediaItem));
                    }
                }));
            }
            else
            {
                this.BeginInvoke(new MethodInvoker(() =>
                {
                    mnuNowPlaying.Text = LocalizationStrings.NotificationThread_NowPlayingDefault;
                    mnuLoveThisTrack.Text = LocalizationStrings.NotificationThread_TrayMenu_Love_this_Track;
                    ShowScrobbleState(); // updates tray icon to default
                }));
            }

            this.BeginInvoke(new MethodInvoker(() =>
            {
                mnuLoveThisTrack.Enabled = mediaItem != null && _currentUser != null;
            }));

        }

        // Base method for handling the continued monitoring a media item
        public virtual void TrackMonitoringProgress(MediaItem mediaItem, int playerPosition)
        {
            if (mediaItem != null)
            {
                this.BeginInvoke(new MethodInvoker(() =>
                {
                    mnuNowPlaying.Text = string.Format(LocalizationStrings.ScrobblerUi_CurrentTrack, MediaHelper.GetTrackDescription(mediaItem));
                    if (mnuNowPlaying.Text.Length >= 64)
                    {
                        trayIcon.Text = mnuNowPlaying.Text.Substring(0, 62) + '…';
                    }
                    else
                    {
                        trayIcon.Text = mnuNowPlaying.Text;
                    }
                }));
            }
            else
            {
                this.BeginInvoke(new MethodInvoker(() =>
                {
                    mnuNowPlaying.Text = LocalizationStrings.NotificationThread_NowPlayingDefault;
                    ShowScrobbleState(); // updates tray icon to default
                }));
            }
        }

        // Method for handling whether the Version Checker has found an update, enabling Ui elements accordingly
        public void HasNewVersion(VersionChecker.VersionState versionDetail)
        {
            this.BeginInvoke(new MethodInvoker(() => {

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

        // Clears all Ui handlers related to the update process (so they can be re-instated if a new version is present)
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

        // Method invoked when the user requests to download the latest update
        public async void DownloadNewVersion(object sender, EventArgs e)
        {
            mnuNewVersion.Enabled = false;
            stripUpdateProgress.Visible = true;
            stripUpdateProgress.AutoSize = true;

            await VersionChecker.DownloadUpdate(stripNewVersion.Tag as VersionChecker.VersionState, Core.UserDownloadsPath, DownloadProgressUpdated, DownloadComplete).ConfigureAwait(false);
        }

        // Method invoked by the update process when a download has completed.
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

        // Method invoked to install a downloaded update
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

        // Method invoked to show the current progress of an update being downloaded
        private void DownloadProgressUpdated(object sender, DownloadProgressChangedEventArgs e)
        {
            mnuNewVersion.Text = string.Format(LocalizationStrings.NotificationThread_DownloadProgressUpdated, e.ProgressPercentage);
            stripUpdateProgress.Text = string.Format(LocalizationStrings.NotificationThread_DownloadProgressUpdated, e.ProgressPercentage);
        }

        // Central method invoked to deal with closing the application when it has been requested to close
        public async Task ExitApplication()
        {
            _userExiting = true;
            this.Close();
        }
       
    }
}
