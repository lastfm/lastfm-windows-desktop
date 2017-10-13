using LastFM.Common.Helpers;
using LastFM.Common.Static_Classes;
using System;
using System.Windows.Forms;
using LastFM.ApiClient;
using LastFM.ApiClient.Models;
using LastFM.Common;
using LastFM.Common.Localization;

namespace DesktopScrobbler
{
    // User interface for allowing the user to authenticate with Last.fm and authorize the application
    // to communicate with the Last.fm API as themselves
    public partial class AuthenticationUi : Form
    {
        // Internal instance of a Last.fm API client
        internal LastFMClient ApiClient = null;

        // The current session token used for the Last.fm API
        internal SessionToken ApiSessionToken = null;

        // An internal timer for checking the Last.fm API for when a user successfully authenticates and authorizes the application
        private Timer _authCheckTimer = null;

        // The form that created this one, so that we can put out notifications and align them
        // with the correct owner
        private Form _owner;

        public AuthenticationUi()
        {
            InitializeComponent();
        }

        // Constructor that specifies the form that created this one, so that we can put out notifications and align them
        // with the correct owner
        public AuthenticationUi(Form owner)
        {
            _owner = owner;

            InitializeComponent();
        }

        // Handler for the 'Authorize' button, which gets an authorisation token from Last.fm, and 
        // launches the user's default browser to get them to sign-in (if not already) and authorize the application
        private async void btnAuthorize_Click(object sender, EventArgs e)
        {
            // Don't allow the user to run this again, to prevent authorisation token clashes
            btnAuthorize.Enabled = false;

            // Get an authorization token from Last.fm
            if (await this.ApiClient.GetAuthenticationToken())
            {
                // Start the timer for tracking the authorization state
                StartAuthCheckTimer();

                // Open the authentication Url in the user's default browser
                ProcessHelper.LaunchUrl(string.Format(APIDetails.UserAuthorizationEndPointUrl, APIDetails.Key, this.ApiClient.AuthenticationToken));
            }
            else
            {
                // Display a message to the user because no connection to Last.fm was available
                MessageBox.Show(this, LocalizationStrings.AuthenticationUI_FailedToAuthorize_Message, string.Format(LocalizationStrings.AuthenticationUi_FailedToAuthorize_MessageTitle, Core.APPLICATION_TITLE), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Method used to create a timer to track the state of authorization
        private void StartAuthCheckTimer()
        {
            // Create a timer to run every second
            _authCheckTimer = new Timer();
            _authCheckTimer.Interval = 1000;

            // The event to fire every time the timer fires
            _authCheckTimer.Tick += _authCheckTimer_Tick;

            // Start the timer
            _authCheckTimer.Start();
        }

        // Delegate event that fires every time the authorization timer elapses
        private async void _authCheckTimer_Tick(object sender, EventArgs e)
        {
            // Stop the timer to prevent execution clashes
            _authCheckTimer.Stop();

            // Get the current session token from the Last.fm API associated with the authentication token
            var sessionToken = await this.ApiClient.GetSessionToken();

            // If a session token is returned, the user authenticated and authorized the application
            if (sessionToken != null)
            {
                // Store the token (to pass back to the Scrobbler Ui)
                this.ApiSessionToken = sessionToken;

                // Set the result of the form operation to 'success' so the calling form knows to continue
                this.DialogResult = DialogResult.OK;

                // Display a popup notification telling the user that we are running as them
                NotificationHelper.ShowNotificationSynch(_owner, Core.APPLICATION_TITLE, string.Format(LocalizationStrings.PopupNotifications_SuccessfullyAuthorized, sessionToken.Name));

                // Close this form
                this.Close();
            }
            else
            {
                // There's still no session token, restart the timer
                _authCheckTimer.Start();
            }
        }

        // Method to stop and clear the timer
        private  void ClearVerificationTimer()
        {
            _authCheckTimer?.Stop();
            _authCheckTimer = null;
        }

        // Handler for the 'Close' button
        private void btnClose_Click(object sender, EventArgs e)
        {
            // Return a result indicating the the user canclled the operation
            this.DialogResult = DialogResult.Cancel;

            // Close this form
            this.Close();
        }

        // Reset the state of the form, so the process of authorization can be (re)started
        internal void Reset()
        {
            this.Text = string.Empty;

            btnAuthorize.Enabled = true;
        }
    }
}
