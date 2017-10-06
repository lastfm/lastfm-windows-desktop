using System.Windows.Forms;
using LastFM.Common.Localization;

namespace LastFM.Common.Classes
{
    public partial class NotificationThread
    {
        #region Form Designer Code

        private ContextMenuStrip trayMenu;
        private System.ComponentModel.IContainer components;
        private ToolStripMenuItem mnuShow;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem mnuEnableScrobbling;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem mnuExit;
        private NotifyIcon trayIcon;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel stripStatus;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem mnuViewUserProfile;
        private ToolStripStatusLabel stripVersionLabel;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotificationThread));
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuNewVersion = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuNewVersionSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.mnuLoveThisTrack = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuShow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuEnableScrobbling = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuViewUserProfile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.stripStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.stripLoveTrack = new System.Windows.Forms.ToolStripStatusLabel();
            this.stripVersionLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.stripNewVersion = new System.Windows.Forms.ToolStripStatusLabel();
            this.stripUpdateProgress = new System.Windows.Forms.ToolStripStatusLabel();
            this.trayMenu.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // trayMenu
            // 
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuNewVersion,
            this.mnuNewVersionSeparator,
            this.mnuLoveThisTrack,
            this.toolStripSeparator5,
            this.mnuShow,
            this.toolStripSeparator1,
            this.mnuEnableScrobbling,
            this.toolStripSeparator3,
            this.mnuViewUserProfile,
            this.toolStripSeparator2,
            this.mnuExit});
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.Size = new System.Drawing.Size(206, 166);
            // 
            // mnuNewVersion
            // 
            this.mnuNewVersion.Name = "mnuNewVersion";
            this.mnuNewVersion.Size = new System.Drawing.Size(205, 22);
            this.mnuNewVersion.Text = LocalizationStrings.NotificationThread_TrayMenu_NewVersionAvailableDefault;
            this.mnuNewVersion.Visible = false;
            // 
            // mnuNewVersionSeparator
            // 
            this.mnuNewVersionSeparator.Name = "mnuNewVersionSeparator";
            this.mnuNewVersionSeparator.Size = new System.Drawing.Size(202, 6);
            this.mnuNewVersionSeparator.Visible = false;
            // 
            // mnuLoveThisTrack
            // 
            this.mnuLoveThisTrack.Name = "mnuLoveThisTrack";
            this.mnuLoveThisTrack.Size = new System.Drawing.Size(205, 22);
            this.mnuLoveThisTrack.Text = LocalizationStrings.NotificationThread_TrayMenu_Love_this_Track;
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(202, 6);
            // 
            // mnuShow
            // 
            this.mnuShow.Name = "mnuShow";
            this.mnuShow.Size = new System.Drawing.Size(205, 22);
            this.mnuShow.Text = LocalizationStrings.NotificationThread_TrayMenu_Show;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(202, 6);
            // 
            // mnuEnableScrobbling
            // 
            this.mnuEnableScrobbling.Checked = true;
            this.mnuEnableScrobbling.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuEnableScrobbling.Name = "mnuEnableScrobbling";
            this.mnuEnableScrobbling.Size = new System.Drawing.Size(205, 22);
            this.mnuEnableScrobbling.Text = LocalizationStrings.NotificationThread_TrayMenu_EnableScrobbling;
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(202, 6);
            // 
            // mnuViewUserProfile
            // 
            this.mnuViewUserProfile.Name = "mnuViewUserProfile";
            this.mnuViewUserProfile.Size = new System.Drawing.Size(205, 22);
            this.mnuViewUserProfile.Text = LocalizationStrings.NotificationThread_TrayMenu_ViewYourProfile;
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(202, 6);
            // 
            // mnuExit
            // 
            this.mnuExit.Name = "mnuExit";
            this.mnuExit.Size = new System.Drawing.Size(205, 22);
            this.mnuExit.Text = LocalizationStrings.NotificationThread_TrayMEnu_Exit;
            // 
            // trayIcon
            // 
            this.trayIcon.ContextMenuStrip = this.trayMenu;
            this.trayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("trayIcon.Icon")));
            this.trayIcon.Text = LocalizationStrings.NotificationThread_Status_StartingUp;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stripStatus,
            this.toolStripStatusLabel1,
            this.stripLoveTrack,
            this.stripVersionLabel,
            this.stripNewVersion,
            this.stripUpdateProgress});
            this.statusStrip1.Location = new System.Drawing.Point(0, 105);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(444, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // stripStatus
            // 
            this.stripStatus.Name = "stripStatus";
            this.stripStatus.Size = new System.Drawing.Size(77, 17);
            this.stripStatus.Text = LocalizationStrings.NotificationThread_Status_StartingUp;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(253, 17);
            this.toolStripStatusLabel1.Spring = true;
            // 
            // stripLoveTrack
            // 
            this.stripLoveTrack.AutoSize = false;
            this.stripLoveTrack.Enabled = false;
            this.stripLoveTrack.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.stripLoveTrack.Name = "stripLoveTrack";
            this.stripLoveTrack.Size = new System.Drawing.Size(24, 17);
            this.stripLoveTrack.Text = "<3";
            // 
            // stripVersionLabel
            // 
            this.stripVersionLabel.AutoSize = false;
            this.stripVersionLabel.Name = "stripVersionLabel";
            this.stripVersionLabel.Size = new System.Drawing.Size(75, 17);
            this.stripVersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // stripNewVersion
            // 
            this.stripNewVersion.AutoSize = false;
            this.stripNewVersion.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stripNewVersion.Image = ((System.Drawing.Image)(resources.GetObject("stripNewVersion.Image")));
            this.stripNewVersion.Name = "stripNewVersion";
            this.stripNewVersion.Size = new System.Drawing.Size(24, 17);
            this.stripNewVersion.TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay;
            this.stripNewVersion.Visible = false;
            // 
            // stripUpdateProgress
            // 
            this.stripUpdateProgress.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.stripUpdateProgress.Name = "stripUpdateProgress";
            this.stripUpdateProgress.Size = new System.Drawing.Size(78, 17);
            this.stripUpdateProgress.Text = LocalizationStrings.NotificationThread_StatusBar_Connecting;
            this.stripUpdateProgress.Visible = false;
            // 
            // NotificationThread
            // 
            this.ClientSize = new System.Drawing.Size(444, 127);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "NotificationThread";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.trayMenu.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ToolStripStatusLabel stripLoveTrack;
        private ToolStripMenuItem mnuLoveThisTrack;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel stripNewVersion;
        private ToolStripMenuItem mnuNewVersion;
        private ToolStripSeparator mnuNewVersionSeparator;
        private ToolStripStatusLabel stripUpdateProgress;
    }
}
