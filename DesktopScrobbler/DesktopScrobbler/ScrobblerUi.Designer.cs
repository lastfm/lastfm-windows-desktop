using LastFM.Common.Localization;

namespace DesktopScrobbler
{
    partial class ScrobblerUi
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScrobblerUi));
            this.label1 = new System.Windows.Forms.Label();
            this.lblSignInName = new System.Windows.Forms.Label();
            this.linkProfile = new System.Windows.Forms.LinkLabel();
            this.linkLogOut = new System.Windows.Forms.LinkLabel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.linkLogIn = new System.Windows.Forms.LinkLabel();
            this.lblTrackName = new System.Windows.Forms.Label();
            this.chkShowtrackChanges = new System.Windows.Forms.CheckBox();
            this.chkShowScrobbleNotifications = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkMinimizeToTray = new System.Windows.Forms.CheckBox();
            this.chkStartMinimized = new System.Windows.Forms.CheckBox();
            this.lblGeneralSettingsTitle = new System.Windows.Forms.Label();
            this.lblPlugins = new System.Windows.Forms.Label();
            this.checkedPluginList = new DesktopScrobbler.Controls.LastfmCheckedListBox();
            this.linkTerms = new System.Windows.Forms.LinkLabel();
            this.chkShowNotifications = new System.Windows.Forms.CheckBox();
            this.lblVersion = new System.Windows.Forms.Label();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(121, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 2;
            // 
            // lblSignInName
            // 
            this.lblSignInName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSignInName.Location = new System.Drawing.Point(9, 12);
            this.lblSignInName.Name = "lblSignInName";
            this.lblSignInName.Size = new System.Drawing.Size(216, 42);
            this.lblSignInName.TabIndex = 3;
            this.lblSignInName.Text = "Connected as WWWWWWWWWWWWWWW";
            // 
            // linkProfile
            // 
            this.linkProfile.Location = new System.Drawing.Point(417, 12);
            this.linkProfile.Name = "linkProfile";
            this.linkProfile.Size = new System.Drawing.Size(102, 23);
            this.linkProfile.TabIndex = 4;
            this.linkProfile.TabStop = true;
            this.linkProfile.Text = "View Your Profile";
            this.linkProfile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkProfile.Visible = false;
            // 
            // linkLogOut
            // 
            this.linkLogOut.Location = new System.Drawing.Point(3, 0);
            this.linkLogOut.Name = "linkLogOut";
            this.linkLogOut.Size = new System.Drawing.Size(62, 23);
            this.linkLogOut.TabIndex = 0;
            this.linkLogOut.TabStop = true;
            this.linkLogOut.Text = "Log Out";
            this.linkLogOut.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.linkLogOut.Visible = false;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.linkLogOut);
            this.flowLayoutPanel1.Controls.Add(this.linkLogIn);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(200, 8);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(65, 66);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // linkLogIn
            // 
            this.linkLogIn.Location = new System.Drawing.Point(3, 23);
            this.linkLogIn.Name = "linkLogIn";
            this.linkLogIn.Size = new System.Drawing.Size(62, 23);
            this.linkLogIn.TabIndex = 1;
            this.linkLogIn.TabStop = true;
            this.linkLogIn.Text = "Log In";
            this.linkLogIn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTrackName
            // 
            this.lblTrackName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTrackName.Location = new System.Drawing.Point(416, 177);
            this.lblTrackName.Name = "lblTrackName";
            this.lblTrackName.Size = new System.Drawing.Size(201, 39);
            this.lblTrackName.TabIndex = 8;
            this.lblTrackName.Text = "Now Playing:";
            this.lblTrackName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTrackName.UseMnemonic = false;
            this.lblTrackName.Visible = false;
            // 
            // chkShowtrackChanges
            // 
            this.chkShowtrackChanges.AutoSize = true;
            this.chkShowtrackChanges.Location = new System.Drawing.Point(420, 125);
            this.chkShowtrackChanges.Name = "chkShowtrackChanges";
            this.chkShowtrackChanges.Size = new System.Drawing.Size(129, 17);
            this.chkShowtrackChanges.TabIndex = 12;
            this.chkShowtrackChanges.Text = "Show Track Changes";
            this.chkShowtrackChanges.UseVisualStyleBackColor = true;
            this.chkShowtrackChanges.Visible = false;
            // 
            // chkShowScrobbleNotifications
            // 
            this.chkShowScrobbleNotifications.AutoSize = true;
            this.chkShowScrobbleNotifications.Location = new System.Drawing.Point(420, 148);
            this.chkShowScrobbleNotifications.Name = "chkShowScrobbleNotifications";
            this.chkShowScrobbleNotifications.Size = new System.Drawing.Size(159, 17);
            this.chkShowScrobbleNotifications.TabIndex = 14;
            this.chkShowScrobbleNotifications.Text = "Show Scrobble Notifications";
            this.chkShowScrobbleNotifications.UseVisualStyleBackColor = true;
            this.chkShowScrobbleNotifications.Visible = false;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(9, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(200, 15);
            this.label2.TabIndex = 16;
            this.label2.Text = "Scrobbler Plugins";
            // 
            // chkMinimizeToTray
            // 
            this.chkMinimizeToTray.AutoSize = true;
            this.chkMinimizeToTray.Location = new System.Drawing.Point(420, 79);
            this.chkMinimizeToTray.Name = "chkMinimizeToTray";
            this.chkMinimizeToTray.Size = new System.Drawing.Size(98, 17);
            this.chkMinimizeToTray.TabIndex = 9;
            this.chkMinimizeToTray.Text = "Minimize to tray";
            this.chkMinimizeToTray.UseVisualStyleBackColor = true;
            this.chkMinimizeToTray.Visible = false;
            // 
            // chkStartMinimized
            // 
            this.chkStartMinimized.AutoSize = true;
            this.chkStartMinimized.Location = new System.Drawing.Point(420, 102);
            this.chkStartMinimized.Name = "chkStartMinimized";
            this.chkStartMinimized.Size = new System.Drawing.Size(97, 17);
            this.chkStartMinimized.TabIndex = 10;
            this.chkStartMinimized.Text = "Start Minimized";
            this.chkStartMinimized.UseVisualStyleBackColor = true;
            this.chkStartMinimized.Visible = false;
            // 
            // lblGeneralSettingsTitle
            // 
            this.lblGeneralSettingsTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGeneralSettingsTitle.Location = new System.Drawing.Point(417, 46);
            this.lblGeneralSettingsTitle.Name = "lblGeneralSettingsTitle";
            this.lblGeneralSettingsTitle.Size = new System.Drawing.Size(200, 17);
            this.lblGeneralSettingsTitle.TabIndex = 13;
            this.lblGeneralSettingsTitle.Text = "General Settings";
            this.lblGeneralSettingsTitle.Visible = false;
            // 
            // lblPlugins
            // 
            this.lblPlugins.Location = new System.Drawing.Point(9, 110);
            this.lblPlugins.Name = "lblPlugins";
            this.lblPlugins.Size = new System.Drawing.Size(253, 20);
            this.lblPlugins.TabIndex = 11;
            this.lblPlugins.Text = "Instructions:";
            this.lblPlugins.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // checkedPluginList
            // 
            this.checkedPluginList.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkedPluginList.FormattingEnabled = true;
            this.checkedPluginList.Location = new System.Drawing.Point(12, 133);
            this.checkedPluginList.Name = "checkedPluginList";
            this.checkedPluginList.Size = new System.Drawing.Size(250, 49);
            this.checkedPluginList.TabIndex = 3;
            // 
            // linkTerms
            // 
            this.linkTerms.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkTerms.Location = new System.Drawing.Point(9, 184);
            this.linkTerms.Name = "linkTerms";
            this.linkTerms.Size = new System.Drawing.Size(167, 23);
            this.linkTerms.TabIndex = 4;
            this.linkTerms.TabStop = true;
            this.linkTerms.Text = "Terms";
            this.linkTerms.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkShowNotifications
            // 
            this.chkShowNotifications.AutoSize = true;
            this.chkShowNotifications.Location = new System.Drawing.Point(12, 47);
            this.chkShowNotifications.Name = "chkShowNotifications";
            this.chkShowNotifications.Size = new System.Drawing.Size(114, 17);
            this.chkShowNotifications.TabIndex = 2;
            this.chkShowNotifications.Text = "Show Notifications";
            this.chkShowNotifications.UseVisualStyleBackColor = true;
            // 
            // lblVersion
            // 
            this.lblVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVersion.Location = new System.Drawing.Point(182, 184);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(83, 23);
            this.lblVersion.TabIndex = 19;
            this.lblVersion.Text = "version";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ScrobblerUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(269, 208);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.chkShowNotifications);
            this.Controls.Add(this.linkTerms);
            this.Controls.Add(this.chkShowtrackChanges);
            this.Controls.Add(this.chkShowScrobbleNotifications);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.chkMinimizeToTray);
            this.Controls.Add(this.chkStartMinimized);
            this.Controls.Add(this.lblGeneralSettingsTitle);
            this.Controls.Add(this.lblPlugins);
            this.Controls.Add(this.checkedPluginList);
            this.Controls.Add(this.lblTrackName);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.linkProfile);
            this.Controls.Add(this.lblSignInName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScrobblerUi";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSignInName;
        private System.Windows.Forms.LinkLabel linkProfile;
        private System.Windows.Forms.LinkLabel linkLogOut;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.LinkLabel linkLogIn;
        private System.Windows.Forms.Label lblTrackName;
        private System.Windows.Forms.CheckBox chkShowtrackChanges;
        private System.Windows.Forms.CheckBox chkShowScrobbleNotifications;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkMinimizeToTray;
        private System.Windows.Forms.CheckBox chkStartMinimized;
        private System.Windows.Forms.Label lblGeneralSettingsTitle;
        private System.Windows.Forms.Label lblPlugins;
        private System.Windows.Forms.LinkLabel linkTerms;
        private System.Windows.Forms.CheckBox chkShowNotifications;
        private System.Windows.Forms.Label lblVersion;
        private Controls.LastfmCheckedListBox checkedPluginList;
    }
}

