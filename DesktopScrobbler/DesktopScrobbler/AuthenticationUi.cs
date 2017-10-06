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
    public partial class AuthenticationUi : Form
    {
        internal LastFMClient ApiClient = null;
        internal SessionToken ApiSessionToken = null;

        private Timer _authCheckTimer = null;

        public AuthenticationUi()
        {
            InitializeComponent();
        }

        private void Authentication_Load(object sender, EventArgs e)
        {
        }

        private async void btnAuthorize_Click(object sender, EventArgs e)
        {
            btnAuthorize.Enabled = false;

            if (await this.ApiClient.GetAuthorisationToken())
            {
                StartAuthCheckTimer();
                ProcessHelper.LaunchUrl(string.Format(APIDetails.UserAuthorizationEndPointUrl, APIDetails.Key, this.ApiClient.AuthenticationToken));
            }
            else
            {
                MessageBox.Show(this, LocalizationStrings.AuthenticationUI_FailedToAuthorize_Message, string.Format(LocalizationStrings.AuthenticationUi_FailedToAuthorize_MessageTitle, Core.APPLICATION_TITLE), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartAuthCheckTimer()
        {
            _authCheckTimer = new Timer();
            _authCheckTimer.Interval = 1000;
            _authCheckTimer.Tick += _authCheckTimer_Tick;
            _authCheckTimer.Start();
        }

        private async void _authCheckTimer_Tick(object sender, EventArgs e)
        {
            _authCheckTimer.Stop();

            var sessionToken = await this.ApiClient.GetSessionToken();

            if (sessionToken != null)
            {
                this.ApiSessionToken = sessionToken;

                this.DialogResult = DialogResult.OK;

                NotificationHelper.ShowNotification(this, Core.APPLICATION_TITLE, string.Format(LocalizationStrings.PopupNotifications_SuccessfullyAuthorized, sessionToken.Name));
                this.Close();
            }
            else
            {
                _authCheckTimer.Start();
            }
        }

        private  void ClearVerificationTimer()
        {
            _authCheckTimer?.Stop();
            _authCheckTimer = null;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        internal void Reset()
        {
            this.Text = string.Empty;

            btnAuthorize.Enabled = true;
        }

        private void btnVerify_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
