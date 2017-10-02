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
                await ProcessHelper.LaunchUrl(string.Format(APIDetails.UserAuthorizationEndPointUrl, APIDetails.Key, this.ApiClient.AuthenticationToken));
                btnVerify.Enabled = true;
            }
            else
            {
                MessageBox.Show(this, "Failed to retrieve an authentication token from LastFM, so authorization cannot take place.", $"{Core.APPLICATION_TITLE} Failed to Authenticate", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }

        internal void Reset()
        {
            this.Text = string.Empty;

            btnAuthorize.Enabled = true;
            btnVerify.Enabled = false;

        }

        private void btnVerify_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
