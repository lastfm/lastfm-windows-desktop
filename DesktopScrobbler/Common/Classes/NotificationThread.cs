
using Common.Classes;
using LastFM.ApiClient.Models;
using LastFM.Common.Factories;
using LastFM.Common.Helpers;
using LastFM.Common.Static_Classes;
using System;
using System.Windows.Forms;

namespace LastFM.Common.Classes
{
    public class NotificationThread: Form
    {
        #region Form Designer Code

        private ContextMenuStrip trayMenu;
        private System.ComponentModel.IContainer components;
        private ToolStripMenuItem mnuShow;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem mnuPauseScrobbling;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem mnuExit;
        private NotifyIcon trayIcon;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel stripStatus;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem mnuShowSettings;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem mnuViewUserProfile;
        private ToolStripStatusLabel stripVersionLabel;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotificationThread));
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuShow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuPauseScrobbling = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuShowSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuViewUserProfile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.stripStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.stripVersionLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.trayMenu.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // trayMenu
            // 
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuShow,
            this.toolStripSeparator1,
            this.mnuPauseScrobbling,
            this.toolStripSeparator3,
            this.mnuShowSettings,
            this.toolStripSeparator4,
            this.mnuViewUserProfile,
            this.toolStripSeparator2,
            this.mnuExit});
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.Size = new System.Drawing.Size(205, 138);
            // 
            // mnuShow
            // 
            this.mnuShow.Name = "mnuShow";
            this.mnuShow.Size = new System.Drawing.Size(204, 22);
            this.mnuShow.Text = "&Show";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(201, 6);
            // 
            // mnuPauseScrobbling
            // 
            this.mnuPauseScrobbling.Name = "mnuPauseScrobbling";
            this.mnuPauseScrobbling.Size = new System.Drawing.Size(204, 22);
            this.mnuPauseScrobbling.Text = "&Pause";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(201, 6);
            // 
            // mnuShowSettings
            // 
            this.mnuShowSettings.Name = "mnuShowSettings";
            this.mnuShowSettings.Size = new System.Drawing.Size(204, 22);
            this.mnuShowSettings.Text = "Show Se&ttings";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(201, 6);
            // 
            // mnuViewUserProfile
            // 
            this.mnuViewUserProfile.Name = "mnuViewUserProfile";
            this.mnuViewUserProfile.Size = new System.Drawing.Size(204, 22);
            this.mnuViewUserProfile.Text = "&View your LastFM Profile";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(201, 6);
            // 
            // mnuExit
            // 
            this.mnuExit.Name = "mnuExit";
            this.mnuExit.Size = new System.Drawing.Size(204, 22);
            this.mnuExit.Text = "E&xit";
            // 
            // trayIcon
            // 
            this.trayIcon.ContextMenuStrip = this.trayMenu;
            this.trayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("trayIcon.Icon")));
            this.trayIcon.Text = "Starting up...";
            this.trayIcon.Visible = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stripStatus,
            this.stripVersionLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 143);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(444, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // stripStatus
            // 
            this.stripStatus.Name = "stripStatus";
            this.stripStatus.Size = new System.Drawing.Size(77, 17);
            this.stripStatus.Text = "Starting up....";
            // 
            // stripVersionLabel
            // 
            this.stripVersionLabel.Name = "stripVersionLabel";
            this.stripVersionLabel.Size = new System.Drawing.Size(352, 17);
            this.stripVersionLabel.Spring = true;
            this.stripVersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // NotificationThread
            // 
            this.ClientSize = new System.Drawing.Size(444, 165);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "NotificationThread";
            this.trayMenu.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion 

        private bool _userExiting = false;
        private SettingsUi _settingsUI = null;
        private UserInfo _currentUser = null;

        public UserInfo CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; }
        }

        public NotificationThread()
        {
            InitializeComponent();

            this.Load += NotificationThread_Load;
        }

        private void NotificationThread_Load(object sender, System.EventArgs e)
        {
            trayIcon.Visible = true;
            trayIcon.DoubleClick += TrayIcon_DoubleClick;
            stripVersionLabel.Text = $"v{ApplicationUtility.GetApplicationVersionNumber()}";

            this.FormClosing += NotificationThread_FormClosing;
            this.Resize += NotificationThread_Resize;

            trayMenu.Opening += TrayMenu_Opening;

            mnuShow.Click += (o, ev) => { ShowForm(); };
            mnuPauseScrobbling.Click += (o, ev) => {

                mnuPauseScrobbling.Checked = !mnuPauseScrobbling.Checked;
                ScrobbleFactory.ScrobblingEnabled = !mnuPauseScrobbling.Checked;

            };

            mnuShowSettings.Click += (o, ev) =>
            {
                ShowSettings();
            };

            mnuViewUserProfile.Click += (o, ev) =>
            {
                ViewUserProfile();
            };

            mnuExit.Click += (o, ev) =>
            {
                ScrobbleFactory.ScrobblingEnabled = false;
                ScrobbleFactory.Dispose();

                _userExiting = true;
                this.Close();

            };
        }

        private void TrayMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mnuShow.Enabled = this.Visible;
            mnuPauseScrobbling.Checked = !ScrobbleFactory.ScrobblingEnabled;
            mnuViewUserProfile.Enabled = !string.IsNullOrEmpty(_currentUser?.Url);
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

        private void NotificationThread_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((e.CloseReason == CloseReason.UserClosing && !Core.Settings.CloseToTray) || _userExiting)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
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

        public void SetStatus(string newStatus)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                if (stripStatus != null)
                {
                    stripStatus.Text = newStatus;
                }

                trayIcon.Text = newStatus;
            }));
        }

        protected void ShowSettings()
        {
            if (_settingsUI == null)
            {
                _settingsUI = new SettingsUi();
                _settingsUI.FormClosing += SettingsUi_FormClosing;
                _settingsUI.StartPosition = (this.WindowState == FormWindowState.Normal) ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;
            }

            bool previousScrobbleState = ScrobbleFactory.ScrobblingEnabled;

            ScrobbleFactory.ScrobblingEnabled = false;

            _settingsUI.ShowDialog(this);

            ScrobbleFactory.ScrobblingEnabled = previousScrobbleState;
        }

        private void SettingsUi_FormClosing(object sender, FormClosingEventArgs e)
        {
            _settingsUI = null;
        }

        protected void ViewUserProfile()
        {
            ProcessHelper.LaunchUrl(_currentUser.Url);
        }
    }
}
