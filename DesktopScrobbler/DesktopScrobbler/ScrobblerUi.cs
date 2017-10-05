using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ITunesScrobblePlugin;
using LastFM.ApiClient;
using LastFM.ApiClient.Models;
using LastFM.Common;
using LastFM.Common.Static_Classes;
using LastFM.Common.Factories;
using PluginSupport;
using LastFM.Common.Classes;
using LastFM.Common.Helpers;
using static LastFM.Common.Factories.ScrobbleFactory;
using System.IO;


namespace DesktopScrobbler
{
    // Comment in this line of code when you need to view / make changes using the UI designer
    // Comment it out when you are done
    // public partial class ScrobblerUi : Form

    // Comment out this line of code when you need to view / make changes using the UI designer
    // Comment it in when you are done
    public partial class ScrobblerUi : NotificationThread
    {
        private AuthenticationUi _authUi = null;
        private WindowsMediaPlayer _playerForm = null;
        private Image _normalStateLogo = null;
        private Image _greyStateLogo = null;

        public ScrobblerUi()
        {
            InitializeComponent();

            this.Load += ScrobblerUi_Load;
        }

        public override void TrackChanged(MediaItem mediaItem, bool wasResumed)
        {
            if (mediaItem != null)
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    lblTrackName.Text = $"Current track: {MediaHelper.GetTrackDescription(mediaItem)}";
                }));
            }
            else
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    lblTrackName.Text = string.Empty;
                }));
            }

            base.TrackChanged(mediaItem, wasResumed);
        }

        private async void ScrobblerUi_Load(object sender, System.EventArgs e)
        {
            CheckForNewVersion();

            _playerForm = new WindowsMediaPlayer();
            _playerForm.ShowInTaskbar = false;
            _playerForm.WindowState = FormWindowState.Minimized;

            try
            {
                _playerForm.Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            _playerForm.Hide();

            linkSettings.Click += (o, ev) =>
            {
                base.ShowSettings();

                if (this.Height == 164)
                {
                    linkSettings.Text = "Settings...";
                }
                else
                {
                    linkSettings.Text = "Settings <<";
                }
            };

            linkProfile.Click += linkProfile_Click;
            linkTerms.Click += (o, ev) => { ProcessHelper.LaunchUrl(Core.TermsUrl); };

            linkLogOut.Click += LogoutUser;
            linkLogIn.Click += LogInUser;

            base.OnScrobbleStateChanged += UpdateScrobbleState;

            _normalStateLogo = pbLogo.Image;
            _greyStateLogo = await ImageHelper.GreyScaleImage(_normalStateLogo).ConfigureAwait(false);

            pbLogo.MouseEnter += (o, ev) =>
            {
                if (!string.IsNullOrEmpty(base.APIClient.SessionToken?.Key))
                {
                    pbLogo.Cursor = Cursors.Hand;
                }
                else
                {
                    pbLogo.Cursor = DefaultCursor;
                }
            };

            pbLogo.Click += (o, ev) =>
            {
                if (!string.IsNullOrEmpty(base.APIClient.SessionToken?.Key))
                {
                    base.ScrobbleStateChanging(!ScrobbleFactory.ScrobblingEnabled);
                }
            };

            Startup();
        }

        private void UpdateScrobbleState(bool scrobblingEnabled)
        {
            pbLogo.Image = (scrobblingEnabled) ? _normalStateLogo : _greyStateLogo;
        }

        private void LogInUser(object sender, EventArgs e)
        {
            Startup(true);
        }

        private void LogoutUser(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to log out?", Core.APPLICATION_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ScrobbleFactory.ScrobblingEnabled = false;

                Core.Settings.UserHasAuthorizedApp = false;
                Core.Settings.SessionToken = string.Empty;
                Core.Settings.Username = string.Empty;

                Core.SaveSettings();

                base.APIClient.LoggedOut();

                RefreshOnlineStatus(OnlineState.Offline);

                VerifyAuthorization("Authentication Required");
            }
        }

        private void linkProfile_Click(object sender, EventArgs e)
        {
            base.ViewUserProfile();
        }

        private async void Startup(bool afterLogOut = false)
        {
            if (!afterLogOut)
            {
                SetStatus("Starting up...");
                Core.InitializeSettings();

                if (Core.Settings.StartMinimized)
                {
                    this.WindowState = FormWindowState.Minimized;
                }

                SetStatus("Loading plugins...");
                await GetPlugins().ConfigureAwait(false);
            }
            else
            {
                ScrobbleFactory.OnlineStatusUpdated -= OnlineStatusUpdated;
            }

            SetStatus("Checking connection to Last.fm...");
            await ConnectToLastFM(afterLogOut).ConfigureAwait(false);

            await ScrobbleFactory.Initialize(base.APIClient, this).ConfigureAwait(false);

            ApplicationConfiguration.CheckPluginDefaultStatus();

            DisplaySettings();

            ScrobbleFactory.ScrobblingEnabled = Core.Settings.ScrobblerStatus.Count(plugin => plugin.IsEnabled) > 0;

            ScrobbleFactory.OnlineStatusUpdated += OnlineStatusUpdated;

            linkSettings.Visible = true;
        }

        private void OnlineStatusUpdated(OnlineState currentState, UserInfo latestUserInfo)
        {
            base.CurrentUser = latestUserInfo;
            RefreshOnlineStatus(currentState);
        }

        private async Task GetPlugins()
        {
            List<Plugin> typedPlugins = await PluginFactory.GetPluginsOfType(ApplicationUtility.ApplicationPath(), typeof(IScrobbleSource)).ConfigureAwait(false);
            ScrobbleFactory.ScrobblePlugins = new List<IScrobbleSource>();
            bool requiresSettingsToBeSaved = false;

            foreach (Plugin pluginItem in typedPlugins)
            {
                var pluginInstance = pluginItem.PluginInstance as IScrobbleSource;

                ScrobbleFactory.ScrobblePlugins.Add(pluginInstance);

                if (Core.Settings.ScrobblerStatus.Count(item => item.Identifier == pluginInstance.SourceIdentifier) == 0)
                {
                    Core.Settings.ScrobblerStatus.Add(new LastFM.Common.Classes.ScrobblerSourceStatus() { Identifier = pluginInstance.SourceIdentifier, IsEnabled = true });
                    requiresSettingsToBeSaved = true;
                }
            }

            // Manually add the embedded version of the Windows Media Plugin #Issue01
            ScrobbleFactory.ScrobblePlugins.Add(new iTunesScrobblePlugin());
            ScrobbleFactory.ScrobblePlugins.Add(new WindowsMediaScrobbleSource(_playerForm));

            if (requiresSettingsToBeSaved)
            {
                Core.SaveSettings();
            }
        }

        private async Task ConnectToLastFM(bool afterLogout)
        {
            if (!afterLogout)
            {
                base.APIClient = new LastFMClient(APIDetails.EndPointUrl, APIDetails.Key, APIDetails.SharedSecret);
            }

            if (!Core.Settings.UserHasAuthorizedApp)
            {
                if (await VerifyAuthorization("Authentication Required").ConfigureAwait(false))
                {
                    Core.Settings.SessionToken = base.APIClient.SessionToken.Key;
                    Core.Settings.UserHasAuthorizedApp = true;
                    Core.Settings.Username = base.APIClient.SessionToken.Name;

                    Core.SaveSettings();
                }
                else
                {
                    SetStatus("Not Logged In.");
                }
            }
            else
            {
                base.APIClient.SessionToken = new SessionToken() { Key = Core.Settings.SessionToken };
            }

            if (Core.Settings.UserHasAuthorizedApp)
            {
                base.ShowTrayIcon();

                // Make an initial connection to get the user profile (to validate there is a connection)
                DisplayCurrentUser();
            }
        }

        public void RefreshOnlineStatus(OnlineState currentState)
        {
            this.Invoke(new MethodInvoker(() =>
            {

                linkLogIn.Visible = false;

                if (currentState == OnlineState.Online)
                {
                    if (!string.IsNullOrEmpty(base.CurrentUser?.Name))
                    {
                        lblSignInName.Text = $"Welcome {base.CurrentUser?.Name}";
                    }
                }
                else if (!string.IsNullOrEmpty(Core.Settings.Username))
                {
                    lblSignInName.Text = $"(Offline) {Core.Settings.Username}";
                }
                else
                {
                    lblSignInName.Text = $"{Core.APPLICATION_TITLE}";
                    linkLogIn.Visible = true;
                    SetStatus("Not logged in.");
                }

                linkProfile.Visible = currentState == OnlineState.Online;
                linkLogOut.Visible = currentState == OnlineState.Online;

                base.RefreshLoveTrackState();
            }));
        }

        private async void DisplayCurrentUser()
        {
            try
            {
                base.CurrentUser = await base.APIClient.GetUserInfo(Core.Settings.Username).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(base.CurrentUser?.Name))
                {
                    RefreshOnlineStatus(OnlineState.Online);
                }
                else if (!string.IsNullOrEmpty(Core.Settings.Username))
                {
                    RefreshOnlineStatus(OnlineState.Offline);
                }
            }
            catch (Exception)
            {
                RefreshOnlineStatus(OnlineState.Offline);
                SetStatus("A connection to Last.fm is not available.");
            }
            finally
            {
                ShowIdleStatus();
            }
        }

        private void ShowIdleStatus()
        {
            if (Core.Settings.ScrobblerStatus.Count(item => item.IsEnabled) > 0 && ScrobbleFactory.ScrobblingEnabled)
            {
                SetStatus("Waiting to Scrobble...");
            }
            else if (Core.Settings.ScrobblerStatus.Count(item => item.IsEnabled) == 0 && ScrobbleFactory.ScrobblingEnabled)
            {
                SetStatus("Scrobbling Disabled (No Plugins Available / Enabled)...");
            }
            else if (!Core.Settings.UserHasAuthorizedApp)
            {
                SetStatus("Not logged in.");
            }
            else
            {
                SetStatus("Scrobbling Paused...");
            }
        }

        private async Task<bool> VerifyAuthorization(string authorizationReason)
        {
            base.HideTrayIcon();

            bool _isApplicationAuthed = false;

            if (_authUi == null)
            {
                _authUi = new AuthenticationUi();
                _authUi.StartPosition = FormStartPosition.CenterScreen;
                _authUi.ApiClient = base.APIClient;
            }

            _authUi.Reset();
            _authUi.Text = authorizationReason;

            this.Enabled = false;

            SetStatus("Waiting for you to authorize the application...");

            var authenticationCloseResult = _authUi.ShowDialog(this);

            this.Enabled = true;

            if (_authUi.DialogResult == DialogResult.OK)
            {
                SetStatus("Checking your connection to Last.fm...");

                // Verify the result by trying to get a SessionToken
                _isApplicationAuthed = !string.IsNullOrEmpty(_authUi?.ApiSessionToken.Key);

                if (_isApplicationAuthed)
                {
                    Core.Settings.SessionToken = base.APIClient.SessionToken?.Key;
                    Core.SaveSettings();
                }
            }
            else if (_authUi.DialogResult == DialogResult.Cancel)
            {
                MessageBox.Show(this, "A valid user account is required for the Desktop Scrobbler to operate correctly, so the application will now close.", $"{Core.APPLICATION_TITLE} Failed to Authenticate", MessageBoxButtons.OK, MessageBoxIcon.Error);
                base.ExitApplication();
            }

            return _isApplicationAuthed;
        }


        private async Task CheckForNewVersion()
        {
            string applicationVersion = ApplicationUtility.BuildVersion();
            string pathDownload = Core.UserDownloadsPath;

            VersionChecker.VersionState result = await Task.Run(() => VersionChecker.CheckVersion(Core.UpdateUrl)).ConfigureAwait(false);

            if (result.IsNewVersion)
            {
                base.HasNewVersion(result);
            }
        }

        private void DisplaySettings()
        {
            checkedPluginList.BorderStyle = BorderStyle.None;
            checkedPluginList.BackColor = this.BackColor;

            chkMinimizeToTray.Checked = Core.Settings.CloseToTray;
            chkStartMinimized.Checked = Core.Settings.StartMinimized;
            chkShowScrobbleNotifications.Checked = Core.Settings.ShowScrobbleNotifications;
            chkShowtrackChanges.Checked = Core.Settings.ShowTrackChanges;

            foreach (IScrobbleSource plugin in ScrobbleFactory.ScrobblePlugins)
            {
                checkedPluginList.Items.Add(plugin.SourceDescription, Core.Settings.ScrobblerStatus.Count(pluginItem => pluginItem.Identifier == plugin.SourceIdentifier && Convert.ToBoolean(pluginItem.IsEnabled == true)) > 0);
            }

            chkShowScrobbleNotifications.CheckedChanged += (o, ev) => { SettingItem_Changed(); };
            chkShowtrackChanges.CheckedChanged += (o, ev) => { SettingItem_Changed(); };
            chkMinimizeToTray.CheckedChanged += (o, ev) => { SettingItem_Changed(); };
            chkStartMinimized.CheckedChanged += (o, ev) => { SettingItem_Changed(); };
            checkedPluginList.ItemCheck += (o, ev) => { this.BeginInvoke(new MethodInvoker(SettingItem_Changed)); };
        }

        private void SettingItem_Changed()
        {
            Core.Settings.CloseToTray = chkMinimizeToTray.Checked;
            Core.Settings.StartMinimized = chkStartMinimized.Checked;
            Core.Settings.ShowScrobbleNotifications = chkShowScrobbleNotifications.Checked;
            Core.Settings.ShowTrackChanges = chkShowtrackChanges.Checked;

            Core.Settings.ScrobblerStatus.Clear();

            foreach (var checkedItem in checkedPluginList.Items)
            {
                IScrobbleSource plugin = ScrobbleFactory.ScrobblePlugins?.FirstOrDefault(item => item.SourceDescription == checkedItem);

                ScrobblerSourceStatus newStatus = new ScrobblerSourceStatus() { Identifier = plugin.SourceIdentifier, IsEnabled = checkedPluginList.CheckedItems.Contains(checkedItem) };
                Core.Settings.ScrobblerStatus.Add(newStatus);
            }

            ScrobbleFactory.ScrobblingEnabled = Core.Settings.ScrobblerStatus.Count(plugin => plugin.IsEnabled) > 0;

            Core.SaveSettings();

            ShowIdleStatus();
        }
    }
}
