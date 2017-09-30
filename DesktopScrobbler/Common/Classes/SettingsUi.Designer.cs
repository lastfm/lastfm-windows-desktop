namespace Common.Classes
{
    partial class SettingsUi
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsUi));
            this.checkedPluginList = new System.Windows.Forms.CheckedListBox();
            this.lblPlugins = new System.Windows.Forms.Label();
            this.lblGeneralSettingsTitle = new System.Windows.Forms.Label();
            this.chkStartMinimized = new System.Windows.Forms.CheckBox();
            this.chkMinimizeToTray = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.chkShowtrackChanges = new System.Windows.Forms.CheckBox();
            this.chkShowScrobbleNotifications = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // checkedPluginList
            // 
            this.checkedPluginList.FormattingEnabled = true;
            this.checkedPluginList.Location = new System.Drawing.Point(118, 168);
            this.checkedPluginList.Name = "checkedPluginList";
            this.checkedPluginList.Size = new System.Drawing.Size(257, 94);
            this.checkedPluginList.TabIndex = 4;
            // 
            // lblPlugins
            // 
            this.lblPlugins.Location = new System.Drawing.Point(118, 149);
            this.lblPlugins.Name = "lblPlugins";
            this.lblPlugins.Size = new System.Drawing.Size(200, 13);
            this.lblPlugins.TabIndex = 1;
            this.lblPlugins.Text = "Select the Scrobbler Plugins to Enable:";
            // 
            // lblGeneralSettingsTitle
            // 
            this.lblGeneralSettingsTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGeneralSettingsTitle.Location = new System.Drawing.Point(115, 14);
            this.lblGeneralSettingsTitle.Name = "lblGeneralSettingsTitle";
            this.lblGeneralSettingsTitle.Size = new System.Drawing.Size(200, 17);
            this.lblGeneralSettingsTitle.TabIndex = 2;
            this.lblGeneralSettingsTitle.Text = "General Settings";
            // 
            // chkStartMinimized
            // 
            this.chkStartMinimized.AutoSize = true;
            this.chkStartMinimized.Location = new System.Drawing.Point(263, 47);
            this.chkStartMinimized.Name = "chkStartMinimized";
            this.chkStartMinimized.Size = new System.Drawing.Size(97, 17);
            this.chkStartMinimized.TabIndex = 1;
            this.chkStartMinimized.Text = "Start Minimized";
            this.chkStartMinimized.UseVisualStyleBackColor = true;
            // 
            // chkMinimizeToTray
            // 
            this.chkMinimizeToTray.AutoSize = true;
            this.chkMinimizeToTray.Location = new System.Drawing.Point(118, 47);
            this.chkMinimizeToTray.Name = "chkMinimizeToTray";
            this.chkMinimizeToTray.Size = new System.Drawing.Size(139, 17);
            this.chkMinimizeToTray.TabIndex = 0;
            this.chkMinimizeToTray.Text = "Close / Minimize to Tray";
            this.chkMinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(115, 126);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(200, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "Scrobbler Plugins";
            // 
            // pbLogo
            // 
            this.pbLogo.Image = ((System.Drawing.Image)(resources.GetObject("pbLogo.Image")));
            this.pbLogo.Location = new System.Drawing.Point(12, 12);
            this.pbLogo.Name = "pbLogo";
            this.pbLogo.Size = new System.Drawing.Size(84, 84);
            this.pbLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbLogo.TabIndex = 6;
            this.pbLogo.TabStop = false;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(280, 276);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(96, 33);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(178, 276);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(96, 33);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // chkShowtrackChanges
            // 
            this.chkShowtrackChanges.AutoSize = true;
            this.chkShowtrackChanges.Location = new System.Drawing.Point(118, 70);
            this.chkShowtrackChanges.Name = "chkShowtrackChanges";
            this.chkShowtrackChanges.Size = new System.Drawing.Size(129, 17);
            this.chkShowtrackChanges.TabIndex = 2;
            this.chkShowtrackChanges.Text = "Show Track Changes";
            this.chkShowtrackChanges.UseVisualStyleBackColor = true;
            // 
            // chkShowScrobbleNotifications
            // 
            this.chkShowScrobbleNotifications.AutoSize = true;
            this.chkShowScrobbleNotifications.Location = new System.Drawing.Point(118, 93);
            this.chkShowScrobbleNotifications.Name = "chkShowScrobbleNotifications";
            this.chkShowScrobbleNotifications.Size = new System.Drawing.Size(159, 17);
            this.chkShowScrobbleNotifications.TabIndex = 3;
            this.chkShowScrobbleNotifications.Text = "Show Scrobble Notifications";
            this.chkShowScrobbleNotifications.UseVisualStyleBackColor = true;
            // 
            // SettingsUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(387, 321);
            this.Controls.Add(this.chkShowtrackChanges);
            this.Controls.Add(this.chkShowScrobbleNotifications);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.pbLogo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkMinimizeToTray);
            this.Controls.Add(this.chkStartMinimized);
            this.Controls.Add(this.lblGeneralSettingsTitle);
            this.Controls.Add(this.lblPlugins);
            this.Controls.Add(this.checkedPluginList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsUi";
            this.Text = "Settings";
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox checkedPluginList;
        private System.Windows.Forms.Label lblPlugins;
        private System.Windows.Forms.Label lblGeneralSettingsTitle;
        private System.Windows.Forms.CheckBox chkStartMinimized;
        private System.Windows.Forms.CheckBox chkMinimizeToTray;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.CheckBox chkShowtrackChanges;
        private System.Windows.Forms.CheckBox chkShowScrobbleNotifications;
    }
}