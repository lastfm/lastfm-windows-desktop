using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using LastFM.ApiClient;
using LastFM.ApiClient.Models;
using LastFM.Common;
using LastFM.Common.Helpers;
using LastFM.Common.Static_Classes;
using LastFM.Common.Factories;
using PluginSupport;
using LastFM.Common.Classes;

namespace DesktopScrobbler
{
    public partial class ScrobblerUi : NotificationThread
    {
        private LastFM.ApiClient.LastFMClient _apiClient = null;
        private AuthenticationUi _authUi = null;
        private SettingsUi _settingsUI = null;

        public ScrobblerUi()
        {
            InitializeComponent();

            this.Load += ScrobblerUi_Load;
        }

        private void ScrobblerUi_Load(object sender, System.EventArgs e)
        {
            linkSettings.Click += (o, ev) => 
            {
                if(_settingsUI == null)
                {
                    _settingsUI = new SettingsUi();
                    _settingsUI.FormClosing += SettingsUi_FormClosing;
                    _settingsUI.StartPosition = FormStartPosition.CenterParent;
                }

                bool previousScrobbleState = ScrobbleFactory.ScrobblingEnabled;

                ScrobbleFactory.ScrobblingEnabled = false;

                _settingsUI.ShowDialog(this);

                ScrobbleFactory.ScrobblingEnabled = previousScrobbleState;
            };

            Startup();
        }

        private void SettingsUi_FormClosing(object sender, FormClosingEventArgs e)
        {
            _settingsUI = null;

            ShowIdleStatus();
        }

        private async void Startup()
        {
            SetStatus("Starting up...");
            Core.InitializeSettings();

            if (Core.Settings.StartMinimized)
            {
                this.WindowState = FormWindowState.Minimized;
            }

            SetStatus("Loading plugins...");
            await GetPlugins();

            SetStatus("Starting up... checking connection to LastFM...");
            ConnectToLastFM();

            ScrobbleFactory.Initialize(_apiClient, this);
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

        private async Task ConnectToLastFM()
        {
            _apiClient = new LastFMClient(APIDetails.EndPointUrl, APIDetails.Key, APIDetails.SharedSecret);

            if (!Core.Settings.UserHasAuthorizedApp)
            {
                if (await VerifyAuthorization("Authentication Required"))
                {
                    Core.Settings.SessionToken = _apiClient.SessionToken.Key;
                    Core.Settings.UserHasAuthorizedApp = true;
                    Core.Settings.Username = _apiClient.SessionToken.Name;

                    Core.SaveSettings();
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
            }
        }

        private async void DisplayCurrentUser()
        {
            try
            {
                var userInfo = await _apiClient.GetUserInfo(Core.Settings.Username);

                if(!string.IsNullOrEmpty(userInfo?.Name))
                {
                    lblSignInName.Text = $"Welcome {userInfo.Name}";
                    linkProfile.Visible = true;
                    linkLogOut.Visible = true;
                    linkSettings.Visible = true;

                    linkProfile.Click += (o, e) => 
                    {
                        ProcessHelper.LaunchUrl(userInfo.Url);
                    };

                    linkLogOut.Click += (o, e) =>
                    {
                        // Not sure what this is meant to do - clarify with LastFM
                    };
                }

                ScrobbleFactory.ScrobblingEnabled = Core.Settings.ScrobblerStatus.Count(plugin => plugin.IsEnabled) > 0;
                ShowIdleStatus();

            }
            catch (Exception)
            {
                SetStatus("A connection to LastFM is not available.");
            }
        }

        private void ShowIdleStatus()
        {
            if(Core.Settings.ScrobblerStatus.Count(item => item.IsEnabled) > 0 && ScrobbleFactory.ScrobblingEnabled)
            {
                SetStatus("Waiting to Scrobble...");
            }
            else if (Core.Settings.ScrobblerStatus.Count(item => item.IsEnabled) == 0 && ScrobbleFactory.ScrobblingEnabled)
            {
                SetStatus("Scrobbling Disabled (No Plugins Available / Enabled)...");
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

            var authenticationCloseResult = _authUi.ShowDialog();

            if (_authUi.DialogResult == DialogResult.OK)
            {
                SetStatus("Starting up... checking connection to LastFM...");

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
