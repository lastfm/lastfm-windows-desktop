using LastFM.Common.Helpers;
using LastFM.Common.Static_Classes;
using System;
using System.Windows.Forms;
using LastFM.ApiClient;
using LastFM.ApiClient.Models;
using LastFM.Common;

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

            if (await this.ApiClient.GetAuthorisationToken().ConfigureAwait(false))
            {
                await ProcessHelper.LaunchUrl(string.Format(APIDetails.UserAuthorizationEndPointUrl, APIDetails.Key, this.ApiClient.AuthenticationToken)).ConfigureAwait(false);
                StartAuthCheckTimer();
            }
            else
            {
                MessageBox.Show(this, "Failed to retrieve an authentication token from Last.fm, so authorization cannot take place.", $"{Core.APPLICATION_TITLE} Failed to Authenticate", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            var sessionToken = await this.ApiClient.GetSessionToken().ConfigureAwait(false);

            if (sessionToken != null)
            {
                this.ApiSessionToken = sessionToken;

                this.DialogResult = DialogResult.OK;
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
