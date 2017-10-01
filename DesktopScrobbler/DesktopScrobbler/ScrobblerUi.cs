using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LastFM.ApiClient;
using LastFM.ApiClient.Models;
using LastFM.Common;
using LastFM.Common.Static_Classes;
using LastFM.Common.Factories;
using PluginSupport;
using LastFM.Common.Classes;
using static LastFM.Common.Factories.ScrobbleFactory;

namespace DesktopScrobbler
{
    public partial class ScrobblerUi : NotificationThread
    {
        private LastFM.ApiClient.LastFMClient _apiClient = null;
        private AuthenticationUi _authUi = null;
        private WindowsMediaPlayer _playerForm = null;

        public ScrobblerUi()
        {
            InitializeComponent();

            this.Load += ScrobblerUi_Load;

        }

        private void ScrobblerUi_Load(object sender, System.EventArgs e)
        {
            _playerForm = new WindowsMediaPlayer();
            _playerForm.ShowInTaskbar = false;
            _playerForm.WindowState = FormWindowState.Minimized;

            _playerForm.Show();
            _playerForm.Hide();

            linkSettings.Click += (o, ev) => 
            {
                base.ShowSettings();
                ShowIdleStatus();
            };

            linkProfile.Click += linkProfile_Click;
            linkLogOut.Click += LogoutUser;
            linkLogIn.Click += LogInUser;

            Startup();
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

                _apiClient.LoggedOut();

                RefreshOnlineStatus(OnlineState.Offline);

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
                await GetPlugins();
            }
            else
            {
                ScrobbleFactory.OnlineStatusUpdated -= OnlineStatusUpdated;
            }

            SetStatus("Checking connection to LastFM...");
            await ConnectToLastFM(afterLogOut);

            ScrobbleFactory.Initialize(_apiClient, this);

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
            List<Plugin> typedPlugins = await PluginFactory.GetPluginsOfType(ApplicationUtility.ApplicationPath(), typeof(IScrobbleSource));
            ScrobbleFactory.ScrobblePlugins = new List<IScrobbleSource>();
            bool requiresSettingsToBeSaved = false;

            foreach (Plugin pluginItem in typedPlugins)
            {
                var pluginInstance = pluginItem.PluginInstance as IScrobbleSource;

                ScrobbleFactory.ScrobblePlugins.Add(pluginInstance);
                ScrobbleFactory.ScrobblePlugins.Add(new WindowsMediaScrobbleSource(_playerForm));

                if (Core.Settings.ScrobblerStatus.Count(item => item.Identifier == pluginInstance.SourceIdentifier) == 0)
                {
                    Core.Settings.ScrobblerStatus.Add(new LastFM.Common.Classes.ScrobblerSourceStatus() { Identifier = pluginInstance.SourceIdentifier, IsEnabled = true });
                    requiresSettingsToBeSaved = true;
                }
            }

            if(requiresSettingsToBeSaved)
            {
                Core.SaveSettings();
            }
        }

        private async Task ConnectToLastFM(bool afterLogout)
        {
            if (!afterLogout)
            {
                _apiClient = new LastFMClient(APIDetails.EndPointUrl, APIDetails.Key, APIDetails.SharedSecret);
            }

            if (!Core.Settings.UserHasAuthorizedApp)
            {
                if (await VerifyAuthorization("Authentication Required"))
                {
                    Core.Settings.SessionToken = _apiClient.SessionToken.Key;
                    Core.Settings.UserHasAuthorizedApp = true;
                    Core.Settings.Username = _apiClient.SessionToken.Name;

                    Core.SaveSettings();
                }
                else
                {
                    SetStatus("Not Logged In.");
                }
            }
            else
            {
                _apiClient.SessionToken = new SessionToken() { Key = Core.Settings.SessionToken };
            }

            if(Core.Settings.UserHasAuthorizedApp)
            {
                // Make an initial connection to get the user profile (to validate there is a connection)
                DisplayCurrentUser();

                if (afterLogout)
                {
                    // Re-initialize the Scrobbling
                    ScrobbleFactory.ScrobblingEnabled = true;
                }
            }
        }

        public void RefreshOnlineStatus(OnlineState currentState)
        {
            this.Invoke(new MethodInvoker(()  => {

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
                    linkLogIn.Visible = true;
                    SetStatus("Not logged in.");
                }

                linkProfile.Visible = currentState == OnlineState.Online;
                linkLogOut.Visible = currentState == OnlineState.Online;
            }));
        }

        private async void DisplayCurrentUser()
        {
            try
            {
                base.CurrentUser = await _apiClient.GetUserInfo(Core.Settings.Username);

                if(!string.IsNullOrEmpty(base.CurrentUser?.Name))
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
                SetStatus("A connection to LastFM is not available.");
            }
            finally
            {
                ScrobbleFactory.ScrobblingEnabled = Core.Settings.ScrobblerStatus.Count(plugin => plugin.IsEnabled) > 0;
                ShowIdleStatus();
            }
        }

        private void ShowIdleStatus()
        {
            if(Core.Settings.ScrobblerStatus.Count(item => item.IsEnabled) > 0 && ScrobbleFactory.ScrobblingEnabled)
            {
                SetStatus("Scrobbler is starting up...");
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
            bool _isApplicationAuthed = false;

            if (_authUi == null)
            {
                _authUi = new AuthenticationUi();
                _authUi.StartPosition = FormStartPosition.CenterScreen;
                _authUi.ApiClient = _apiClient;
            }

            _authUi.Reset();
            _authUi.Text = authorizationReason;

            this.Enabled = false;

            SetStatus("Waiting for you to authorize the application...");

            var authenticationCloseResult = _authUi.ShowDialog(this);

            this.Enabled = true;

            if (_authUi.DialogResult == DialogResult.OK)
            {
                SetStatus("Checking your connection to LastFM...");

                // Verify the result by trying to get a SessionToken
                var returnedSessionToken = await _apiClient.GetSessionToken();

                _isApplicationAuthed = !string.IsNullOrEmpty(returnedSessionToken?.Key);

                if(_isApplicationAuthed)
                {
                    Core.Settings.SessionToken = _apiClient.SessionToken?.Key;
                    Core.SaveSettings();
                }
            }

            return _isApplicationAuthed;
        }
    }


}
