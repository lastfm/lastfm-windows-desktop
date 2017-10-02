﻿using System.Windows.Forms;

namespace LastFM.Common.Classes
{
    public partial class NotificationThread
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
            this.mnuLoveThisTrack = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
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
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.stripLoveTrack = new System.Windows.Forms.ToolStripStatusLabel();
            this.stripVersionLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.trayMenu.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // trayMenu
            // 
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuLoveThisTrack,
            this.toolStripSeparator5,
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
            this.trayMenu.Size = new System.Drawing.Size(205, 166);
            // 
            // mnuLoveThisTrack
            // 
            this.mnuLoveThisTrack.Name = "mnuLoveThisTrack";
            this.mnuLoveThisTrack.Size = new System.Drawing.Size(204, 22);
            this.mnuLoveThisTrack.Text = "&Love this Track";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(201, 6);
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
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stripStatus,
            this.toolStripStatusLabel1,
            this.stripLoveTrack,
            this.stripVersionLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 143);
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
            this.stripStatus.Text = "Starting up....";
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
            // NotificationThread
            // 
            this.ClientSize = new System.Drawing.Size(444, 165);
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
    }
}
