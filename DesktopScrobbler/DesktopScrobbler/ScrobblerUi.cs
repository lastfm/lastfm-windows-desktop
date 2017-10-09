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
using LastFM.Common.Localization;


namespace DesktopScrobbler
{
    // The Scrobbler Ui originally inherited the NotificationUi from the 'Common' project, to extend it and add
    // additional features.  Most of those features have now been 'removed' from view, but left here in case
    // there's a backtrack at any point soon

    // NOTE: YOU MUST HAVE BUILT THE PROJECT AT LEAST ONCE TO BE ABLE TO LOAD THIS FORM IN DESIGNER VIEW
    // AFTER MAKING THE CHANGES RECOMMENDED BELOW

    // Comment in this line of code when you need to view / make changes using the UI designer
    // Comment it out when you are done
    // public partial class ScrobblerUi : Form

    // Comment out this line of code when you need to view / make changes using the UI designer
    // Comment it in when you are done
    public partial class ScrobblerUi : NotificationThread
    {
        // Internal instance of the Authentication user interface
        private AuthenticationUi _authUi = null;

        // Internal instance of the Windows Media Player host
        private WindowsMediaPlayer _playerForm = null;

        // The Last.fm logo in its normal state (full colour)
        private Image _normalStateLogo = null;

        // The Last.fm logo in its disabled state (greyscale)
        private Image _greyStateLogo = null;

        // Constructor for the user interface
        public ScrobblerUi()
        {
            InitializeComponent();

            // Always display the form in the middle of the current screen
            this.StartPosition = FormStartPosition.CenterScreen;

            // Delegate to fire when the User inteface loads
            this.Load += ScrobblerUi_Load;

            // Convert all text on the form to the localized versions
            Localize();
        }

        // Method called by the Scrobbler when track monitoring is started
        public override void TrackMonitoringStarted(MediaItem mediaItem, bool wasResumed)
        {
            // If we have been passed a media item
            if (mediaItem != null)
            {
                // Display the name of the track, in human-readable localized form
                this.Invoke(new MethodInvoker(() =>
                {
                    lblTrackName.Text = string.Format(LocalizationStrings.ScrobblerUi_CurrentTrack, MediaHelper.GetTrackDescription(mediaItem));
                }));
            }
            else
            {
                // Otherwise clear the display of the track name
                this.Invoke(new MethodInvoker(() =>
                {
                    lblTrackName.Text = string.Empty;
                }));
            }

            // Raise the event on the inherited form
            base.TrackMonitoringStarted(mediaItem, wasResumed);
        }

        // Method called when the Ui loads
        private async void ScrobblerUi_Load(object sender, System.EventArgs e)
        {
            // Display the current version number
            lblVersion.Text = $"v{ApplicationUtility.GetApplicationFullVersionNumber()}";

            // Check for a new version from the website
            CheckForNewVersion();

            // Create a new instance of the Windows Media Player host form, and hide it
            _playerForm = new WindowsMediaPlayer();
            _playerForm.ShowInTaskbar = false;
            _playerForm.WindowState = FormWindowState.Minimized;

            try
            {
                // Load the form which houses the Windows Media Player OCX interop control so we can
                // start communicating with Windows Media Player
                _playerForm.Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            _playerForm.Hide();

            // Assign a delegate to the (hidden) 'View your profile' link
            linkProfile.Click += linkProfile_Click;

            // Assign a delegate to the 'Terms of use' link
            linkTerms.Click += (o, ev) => 
            {
                // Display the Last.fm terms of use web page using the user's default browser
                ProcessHelper.LaunchUrl(Core.TermsUrl);
            };
            linkTerms.PreviewKeyDown += (o, ev) =>
            {
                if (ev.KeyCode == Keys.Space || ev.KeyCode == Keys.Enter || ev.KeyCode == Keys.Return)
                {
                    // Display the Last.fm terms of use web page using the user's default browser
                    ProcessHelper.LaunchUrl(Core.TermsUrl);
                }
            };


            // Assign a delegate to the user 'Log Out' link
            linkLogOut.Click += LogoutUser;
            linkLogOut.PreviewKeyDown += (o, ev) =>
            {
                if (ev.KeyCode == Keys.Space || ev.KeyCode == Keys.Enter || ev.KeyCode == Keys.Return)
                {
                    LogoutUser(o, null);
                }
            };

            // Assign a delegate to the user 'Log In' link
            // (which might be redundant in both the old and new UI)
            linkLogIn.Click += LogInUser;
            linkLogIn.PreviewKeyDown += (o, ev) =>
            {
                if (ev.KeyCode == Keys.Space || ev.KeyCode == Keys.Enter || ev.KeyCode == Keys.Return)
                {
                    LogInUser(o, null);
                }
            };

            // Assign a delegate to shadow the base forms 'OnScrobbleStateChanged' event so that we can display
            // the state on this form
            base.OnScrobbleStateChanged += UpdateScrobbleState;

            // Remove the annoying 'have to click twice, once to select the control, again to modify the checkbox'
            // issue that the default behaviour of the checked listbox has
            checkedPluginList.CheckOnClick = true;

            // Make sure there's never a 'selected' row, even though it will briefly appear on the check
            //checkedPluginList.SelectedIndexChanged += PluginList_SelectedIndexChanged;

            checkedPluginList.PreviewKeyDown += (o, ev) =>
            {
                //checkedPluginList.SelectedIndexChanged -= PluginList_SelectedIndexChanged;

                if (ev.KeyCode == Keys.Down && checkedPluginList.SelectedIndex < checkedPluginList.Items.Count - 1)
                {
                    checkedPluginList.SelectedIndex++;
                }
                else if (ev.KeyCode == Keys.Up && checkedPluginList.SelectedIndex > 0)
                {
                    checkedPluginList.SelectedIndex--;
                }

                //checkedPluginList.SelectedIndexChanged += PluginList_SelectedIndexChanged;
            };

            // Fix descenders being cut off in the list
            checkedPluginList.ItemHeight += 4;

            // Once upon a time there was a logo on the form that could be used to enable / disable the scrobble state
            // but it was deemed redundant.  If it ever makes a return, re-establish this code
            //_normalStateLogo = pbLogo.Image;
            //_greyStateLogo = await ImageHelper.GreyScaleImage(_normalStateLogo).ConfigureAwait(false);

            //pbLogo.MouseEnter += (o, ev) =>
            //{
            //    if (!string.IsNullOrEmpty(base.APIClient.SessionToken?.Key))
            //    {
            //        pbLogo.Cursor = Cursors.Hand;
            //    }
            //    else
            //    {
            //        pbLogo.Cursor = DefaultCursor;
            //    }
            //};

            //pbLogo.Click += (o, ev) =>
            //{
            //    if (!string.IsNullOrEmpty(base.APIClient.SessionToken?.Key))
            //    {
            //        base.ScrobbleStateChanging(!ScrobbleFactory.ScrobblingEnabled);
            //    }
            //};

            // Call the startup routine to check authorization and start the scrobbling process
            Startup();
        }

        // Obsolete method that used to be called to allow the last.fm Logo to change state based on the 
        // current state of the scrobbler
        private void UpdateScrobbleState(bool scrobblingEnabled)
        {
            //pbLogo.Image = (scrobblingEnabled) ? _normalStateLogo : _greyStateLogo;
        }

        // Obsolete? method used to restart the authentication loop
        private void LogInUser(object sender, EventArgs e)
        {
            Startup(true);
        }

        // Method used to confirm the user wants to log out, and de-authorize the application
        private void LogoutUser(object sender, EventArgs e)
        {
            // Confirm with the user that they want to log out
            if (MessageBox.Show(this, LocalizationStrings.ScrobblerUi_LogoutUser_Message, Core.APPLICATION_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // They answered 'Yes'

                // Disable scrobbling
                ScrobbleFactory.ScrobblingEnabled = false;

                // Modify the settings and de-authorize the application
                Core.Settings.UserHasAuthorizedApp = false;

                // Clear the stored session token
                Core.Settings.SessionToken = string.Empty;

                // Clear the stored username
                Core.Settings.Username = string.Empty;

                // Save the current settings file
                Core.SaveSettings();

                // Clear the token state of the API Client
                base.APIClient.LoggedOut();

                // Refresh the user interface to reflect the current logged in state
                RefreshOnlineStatus(OnlineState.Offline);

                // Run the authentication loop again (in case they want to switch users)
                Startup(true);
            }
        }

        // Method used to display the users profile when they click on the 'View your profile link' (now redundant)
        private void linkProfile_Click(object sender, EventArgs e)
        {
            // Pass the call to the inherited form
            base.ViewUserProfile();
        }

        // The authentication loop method
        private async void Startup(bool afterLogOut = false)
        {
            // If we're here because the user just launched the application and hasn't authorized the application
            if (!afterLogOut)
            {
                // Set the appropriate status on the (hidden) status bar
                SetStatus(LocalizationStrings.NotificationThread_Status_StartingUp);

                // Initialize the application
                Core.InitializeApplication();

                // Hide this user interface 
                if (Core.Settings.StartMinimized)
                {
                    this.WindowState = FormWindowState.Minimized;
                }

                // Set the appropriate status on the (hidden) status bar
                SetStatus(LocalizationStrings.ScrobblerUi_Status_LoadingPlugins);

                // Get the available external scrobble plugins
                await GetPlugins().ConfigureAwait(false);
            }
            else
            {
                // Remove the online status tracking delegate
                ScrobbleFactory.OnlineStatusUpdated -= OnlineStatusUpdated;
            }

            // Set the appropriate status on the (hidden) status bar
            SetStatus(LocalizationStrings.ScrobblerUi_Status_ConnectingToLastfm);

            // Use the Last.fm API client to check the status of authorization
            await ConnectToLastFM(afterLogOut).ConfigureAwait(false);

            // Initialize the Scrobbling handler (only gets here when the client is authorized)
            await ScrobbleFactory.Initialize(base.APIClient, this).ConfigureAwait(false);

            // Make sure we have a default status available for all the loaded plugins
            ApplicationConfiguration.CheckPluginDefaultStatus();

            // If we got here because the application is starting
            if (!afterLogOut)
            {
                // Update the user interface to show the current settings
                DisplaySettings();
            }

            // Enable scrobbling only if there are any plugins enabled
            ScrobbleFactory.ScrobblingEnabled = Core.Settings.ScrobblerStatus.Count(plugin => plugin.IsEnabled) > 0;

            // Assign a delegate to update the Ui when the user goes on/offline with the Last.fm API
            ScrobbleFactory.OnlineStatusUpdated += OnlineStatusUpdated;
        }

        // Delegate called when the user goes on/offline with the Last.fm API
        private void OnlineStatusUpdated(OnlineState currentState, UserInfo latestUserInfo)
        {
            base.CurrentUser = latestUserInfo;
            RefreshOnlineStatus(currentState);
        }

        // Method used to get the available external plugins
        private async Task GetPlugins()
        {
            // Query the plugin factory to get a list of all the assemblies that contain an 'IScrobbleSource' class
            List<Plugin> typedPlugins = await PluginFactory.GetPluginsOfType(ApplicationUtility.ApplicationPath(), typeof(IScrobbleSource)).ConfigureAwait(false);

            // Local list of all the plugins
            ScrobbleFactory.ScrobblePlugins = new List<IScrobbleSource>();

            // Track whether or not new plugins have been discovered and don't have associated enabled/disabled state in the
            // current user settings
            bool requiresSettingsToBeSaved = false;

            // Iterate each of the available plugins
            foreach (Plugin pluginItem in typedPlugins)
            {
                // Conver the plugin into the base type that we know about
                var pluginInstance = pluginItem.PluginInstance as IScrobbleSource;

                // Add this plugin to internal list
                ScrobbleFactory.ScrobblePlugins.Add(pluginInstance);

                // Check if we have any settings for this plugin
                if (Core.Settings.ScrobblerStatus.Count(item => item.Identifier == pluginInstance.SourceIdentifier) == 0)
                {
                    // No?  Add a default instance of the settings for this plugin, defaulting it to 'off'
                    Core.Settings.ScrobblerStatus.Add(new LastFM.Common.Classes.ScrobblerSourceStatus() { Identifier = pluginInstance.SourceIdentifier, IsEnabled = false });

                    // Mark the fact that we need to update the users settings file
                    requiresSettingsToBeSaved = true;
                }
            }

            // Create an instance of the iTunes Scrobble plugin and add it to the list
            ScrobbleFactory.ScrobblePlugins.Add(new iTunesScrobblePlugin());

            // Create an instance of the Windows Media Player Scrobble plugin and add it to the list
            // and assign the host form for the Media Player interop control
            ScrobbleFactory.ScrobblePlugins.Add(new WindowsMediaScrobbleSource(_playerForm));

            // If we need to update the users settings file
            if (requiresSettingsToBeSaved)
            {
                // Automatically save the settings file
                Core.SaveSettings();
            }
        }

        // Method used to connect to, and verify the state of authentication of the Last.fm API
        private async Task ConnectToLastFM(bool afterLogout)
        {
            // If we've not already created an instance of the Last.fm API client
            if (!afterLogout)
            {
                // Create a new instance of the Last.fm API client
                base.APIClient = new LastFMClient(APIDetails.EndPointUrl, APIDetails.Key, APIDetails.SharedSecret);
            }

            // If the user hasn't authorised the application
            if (!Core.Settings.UserHasAuthorizedApp)
            {
                // Run the authentication loop, and determine if it succeeds
                if (await VerifyAuthorization(LocalizationStrings.AuthenticationUi_AuthorizationRequired_WindowTitle).ConfigureAwait(false))
                {
                    // It has succeeded

                    // Store the session key returned by the API
                    Core.Settings.SessionToken = base.APIClient.SessionToken.Key;

                    // Update the settings to state the user has authorized the application
                    Core.Settings.UserHasAuthorizedApp = true;

                    // Keep a copy of the username that authorized the application
                    Core.Settings.Username = base.APIClient.SessionToken.Name;

                    // Save the current user settings
                    Core.SaveSettings();
                }
                else
                {
                    // Update the (hidden) status bar to show the user is not logged in
                    SetStatus(LocalizationStrings.ScrobblerUi_Status_NotLoggedIn);
                }
            }
            else
            {
                // Update the API client with the last known session token to use with API methods that
                // require authentication
                base.APIClient.SessionToken = new SessionToken() { Key = Core.Settings.SessionToken };
            }

            // If the user has authorized the application
            if (Core.Settings.UserHasAuthorizedApp)
            {
                // Show the Last.fm scrobbler icon in the system tray
                base.ShowTrayIcon();

                // Make an initial connection to get the user profile (to validate there is a connection)
                DisplayCurrentUser();
            }
        }

        // Delegate method used to update the User Interface with the current state of the user session
        public void RefreshOnlineStatus(OnlineState currentState)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                // Hide the (obsolete) 'Log Lin' link
                linkLogIn.Visible = false;

                // If the current state shows that the user has a connection to the Last.fm API
                if (currentState == OnlineState.Online)
                {
                    // Display the current sign in name
                    if (!string.IsNullOrEmpty(base.CurrentUser?.Name))
                    {
                        lblSignInName.Text = string.Format(LocalizationStrings.ScrobblerUi_UserLoggedInText, base.CurrentUser?.Name);
                    }
                }
                // Or, if the user has previously authorised the application
                else if (!string.IsNullOrEmpty(Core.Settings.Username))
                {
                    // Display the last known username, but indicating that the session is 'Offline'
                    lblSignInName.Text = String.Format(LocalizationStrings.ScrobblerUi_UserOffline, base.CurrentUser?.Name);
                }
                else
                {
                    // Otherwise just display the name of the application
                    lblSignInName.Text = $"{Core.APPLICATION_TITLE}";
                    linkLogIn.Visible = true;
                    SetStatus(LocalizationStrings.ScrobblerUi_Status_NotLoggedIn);
                }

                // Show or hide various Ui elements based on the online status
                linkProfile.Visible = currentState == OnlineState.Online;
                linkLogOut.Visible = currentState == OnlineState.Online;

                // Reset the 'Love Track' Ui status
                base.RefreshLoveTrackState();
            }));
        }

        // Method used to get details of the current logged in user, and reset the Ui display as appropriate
        private async void DisplayCurrentUser()
        {
            try
            {
                // Use the Last.fm API client to request details of the currently logged in user
                base.CurrentUser = await base.APIClient.GetUserInfo(Core.Settings.Username).ConfigureAwait(false);

                // If we get some details back, the user is online
                if (!string.IsNullOrEmpty(base.CurrentUser?.Name))
                {
                    RefreshOnlineStatus(OnlineState.Online);
                }
                // Otherwise the user is offline
                else if (!string.IsNullOrEmpty(Core.Settings.Username))
                {
                    RefreshOnlineStatus(OnlineState.Offline);
                }
            }
            catch (Exception)
            {
                // There was probably a problem communicating with the Last.fm API, most likely because there was no connection
                RefreshOnlineStatus(OnlineState.Offline);
                SetStatus(LocalizationStrings.ScrobblerUi_Status_ConnectionToLastfmNotAvailable);
            }
            finally
            {
                ShowIdleStatus();
            }
        }

        // Method used to update the now obsolete status bar with the current state of the scrobbler
        private void ShowIdleStatus()
        {
            base.ShowScrobbleState();

            // If there are any plugins enabled, and scrobbling is enabled... set the status to 'Waiting to scrobble'
            if (Core.Settings.ScrobblerStatus.Count(item => item.IsEnabled) > 0 && ScrobbleFactory.ScrobblingEnabled)
            {
                SetStatus(LocalizationStrings.NotificationThread_Status_WaitingToScrobble);
            }
            // Otherwise if there are no plugins enabled and the scrobbler is enabled.. set the status to 'No plugins available'
            else if (Core.Settings.ScrobblerStatus.Count(item => item.IsEnabled) == 0 && ScrobbleFactory.ScrobblingEnabled)
            {
                SetStatus(LocalizationStrings.ScrobblerUi_Status_NoPluginsAvailable);
            }
            // Otherwise if the user has not authenticated the application, set the status to 'Not logged in'
            else if (!Core.Settings.UserHasAuthorizedApp)
            {
                SetStatus(LocalizationStrings.ScrobblerUi_Status_NotLoggedIn);
            }
            else
            {
                // Otherwise the scrobbler must be paused, set the status to 'Scrobbling is paused'
                SetStatus(LocalizationStrings.NotificationThread_Status_ScrobblingPaused);
            }
        }

        // Method for displaying the 'Authenticate this application' user interface
        private async Task<bool> VerifyAuthorization(string authorizationReason)
        {
            // Hide the tray icon, as the application is not in a valid 'running' state
            base.HideTrayIcon();

            // Default value for the current state of application authorization
            bool _isApplicationAuthed = false;

            // If we've not pre-loaded the authentication Ui
            if (_authUi == null)
            {
                // Create a new instance of the Ui, and pass it the API client
                _authUi = new AuthenticationUi();
                _authUi.StartPosition = FormStartPosition.CenterScreen;
                _authUi.ApiClient = base.APIClient;
            }

            // Reset the loaded authentication Ui
            _authUi.Reset();

            // Set the title based on the reason the Ui is being displayed
            _authUi.Text = authorizationReason;

            // Don't allow this window to be interacted with anymore (in case it's visible)
            this.Enabled = false;

            // Update the (hidden) status bar to show we're waiting for the application to be authorized
            SetStatus(LocalizationStrings.ScrobblerUi_Status_WaitingForAuthorization);

            // Display the authorization Ui as a dialog, and return whether the authentication succeeded or if the user
            // simply closed it
            var authenticationCloseResult = _authUi.ShowDialog(this);

            // Re-enable this form
            this.Enabled = true;

            // If the reason the form closed was because the authentication had succeeded
            if (_authUi.DialogResult == DialogResult.OK)
            {
                // Set the (hidden) status bar to tell the user that we're connecting to Last.fm
                SetStatus(LocalizationStrings.ScrobblerUi_Status_ConnectingToLastfm);

                // Set the local state based on whethere we have a valid Session token
                _isApplicationAuthed = !string.IsNullOrEmpty(_authUi?.ApiSessionToken.Key);

                // If the session token is valid
                if (_isApplicationAuthed)
                {
                    // Store the session token in the settings
                    Core.Settings.SessionToken = base.APIClient.SessionToken?.Key;

                    // Automatically save the settings
                    Core.SaveSettings();
                }
            }
            // Or if the user closed the authorization Ui without completing the process
            else if (_authUi.DialogResult == DialogResult.Cancel)
            {
                // Show a message stating that the application can't run any more
                MessageBox.Show(this, LocalizationStrings.ScrobblerUi_ValidUserAccountRequiredMessage, string.Format(LocalizationStrings.AuthenticationUI_FailedToAuthorize_Message, Core.APPLICATION_TITLE), MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Cleanly close the application
                base.ExitApplication();
            }

            return _isApplicationAuthed;
        }

        // Method used to check the website for any indication that there is a new version available
        private async Task CheckForNewVersion()
        {
            // Aynchronously check for a new version
            VersionChecker.VersionState result = await Task.Run(() => VersionChecker.CheckVersion(Core.UpdateUrl)).ConfigureAwait(false);

            // If the check shows there's a new version available
            if (result.IsNewVersion)
            {
                // Pass this result back to the inherited form (to show this in the context menu)
                base.HasNewVersion(result);
            }
        }

        // Method used to populate the Ui with the current user settings
        private void DisplaySettings()
        {
            // Get rid of the un-neccesary bored around the list control that the available plugins gets shown in
            checkedPluginList.BorderStyle = BorderStyle.None;
            checkedPluginList.BackColor = this.BackColor;

            // Make sure that list is clear before we populate it
            checkedPluginList.Items.Clear();

            // Obsolete setting for whether the application should minimize / close to the tray (hide the window from the taskbar)
            // (Defaulted to true now, but can be overridden by manually changing the settings file)
            chkMinimizeToTray.Checked = Core.Settings.CloseToTray;

            // Obsolete setting for starting the application in a minimized state
            // (Defaulted to true now, but can be overridden by manually changing the settings file)
            chkStartMinimized.Checked = Core.Settings.StartMinimized;

            // Setting denote if notifications should be displayed (at all)
            chkShowNotifications.Checked = Core.Settings.ShowNotifications;

            // Obsolete setting for determining if Scrobble notifications should be displayed
            // (Defaulted to true now, but can be overridden by manually changing the settings file)
            chkShowScrobbleNotifications.Checked = Convert.ToBoolean(Core.Settings.ShowScrobbleNotifications);

            // Obsolete setting for determining if track change notifications should be displayed
            // (Defaulted to true now, but can be overridden by manually changing the settings file)
            chkShowtrackChanges.Checked = Convert.ToBoolean(Core.Settings.ShowTrackChanges);

            // Display the list of available plugins and their enabled state
            foreach (IScrobbleSource plugin in ScrobbleFactory.ScrobblePlugins)
            {
                checkedPluginList.Items.Add(plugin.SourceDescription, Core.Settings.ScrobblerStatus.Count(pluginItem => pluginItem.Identifier == plugin.SourceIdentifier && Convert.ToBoolean(pluginItem.IsEnabled == true)) > 0);
            }

            // Define handlers for when the user modifies any of the check boxes associated with the settings
            chkShowScrobbleNotifications.CheckedChanged += (o, ev) => { SettingItem_Changed(); };
            chkShowtrackChanges.CheckedChanged += (o, ev) => { SettingItem_Changed(); };
            chkMinimizeToTray.CheckedChanged += (o, ev) => { SettingItem_Changed(); };
            chkStartMinimized.CheckedChanged += (o, ev) => { SettingItem_Changed(); };
            chkShowNotifications.CheckedChanged += (o, ev) => { SettingItem_Changed(); };
            checkedPluginList.ItemCheck += (o, ev) => { this.BeginInvoke(new MethodInvoker(SettingItem_Changed)); };
        }

        // Method used when the user changes any of the checkboxes on the settings ui
        private void SettingItem_Changed()
        {
            // Temporarily pause scrobbling
            ScrobbleFactory.ScrobblingEnabled = false;

            // Modify the settings based on what the current state of the Ui elements are
            Core.Settings.CloseToTray = chkMinimizeToTray.Checked;
            Core.Settings.StartMinimized = chkStartMinimized.Checked;
            Core.Settings.ShowScrobbleNotifications = chkShowScrobbleNotifications.Checked;
            Core.Settings.ShowTrackChanges = chkShowtrackChanges.Checked;
            Core.Settings.ShowNotifications = chkShowNotifications.Checked;

            // Clear the settings of the current scrobbler plugin status'
            Core.Settings.ScrobblerStatus.Clear();

            // Create a new list of scrobbler plugin status'
            foreach (var checkedItem in checkedPluginList.Items)
            {
                IScrobbleSource plugin = ScrobbleFactory.ScrobblePlugins?.FirstOrDefault(item => item.SourceDescription == checkedItem);

                ScrobblerSourceStatus newStatus = new ScrobblerSourceStatus() { Identifier = plugin.SourceIdentifier, IsEnabled = checkedPluginList.CheckedItems.Contains(checkedItem) };
                Core.Settings.ScrobblerStatus.Add(newStatus);
            }

            // Re-enable scrobbling based on the mew plugin status'
            bool allowScrobbling = Core.Settings.ScrobblerStatus.Count(plugin => plugin.IsEnabled) > 0;

            base.ScrobbleStateChanging(allowScrobbling);


            // Automatically update the settings file
            Core.SaveSettings();

            // Refresh the scrobble state on the Ui
            ShowIdleStatus();
        }

        // Method used to localize each of the Ui elements that can be localized
        private void Localize()
        {
            this.lblSignInName.Text = LocalizationStrings.General_ApplicationTitle;
            this.linkProfile.Text = LocalizationStrings.NotificationThread_TrayMenu_ViewYourProfile;
            //this.linkSettings.Text = LocalizationStrings.ScrobblerUi_LinkSettings_Closed;
            this.linkLogOut.Text = LocalizationStrings.ScrobblerUi_UserLogout;
            this.linkLogIn.Text = LocalizationStrings.ScrobblerUi_UserLogin;
            this.chkShowtrackChanges.Text = LocalizationStrings.ScrobblerUi_Settings_ShowTrackChanges;
            this.chkShowScrobbleNotifications.Text = LocalizationStrings.ScrobblerUi_Settings_ShowScrobbles;
            this.label2.Text = LocalizationStrings.ScrobblerUi_Settings_ScrobblePlugins_Title;
            this.chkMinimizeToTray.Text = LocalizationStrings.ScrobblerUi_Settings_CloseMinimizeToTray;
            this.chkStartMinimized.Text = LocalizationStrings.ScrobblerUi_Settings_StartMinimized;
            this.lblGeneralSettingsTitle.Text = LocalizationStrings.ScrobblerUi_Settings_GeneralSettingsTitle;
            this.lblPlugins.Text = LocalizationStrings.ScrobblerUi_Settings_ScrobblePluginsEnableMessage;
            this.linkTerms.Text = LocalizationStrings.ScrobblerUi_TermsOfUse;
            this.Text = LocalizationStrings.General_ApplicationTitle;
            this.linkLogIn.Text = LocalizationStrings.ScrobblerUi_UserLogin;
        }
    }
}
