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
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblSignInName = new System.Windows.Forms.Label();
            this.linkProfile = new System.Windows.Forms.LinkLabel();
            this.linkSettings = new System.Windows.Forms.LinkLabel();
            this.linkLogOut = new System.Windows.Forms.LinkLabel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.linkLogIn = new System.Windows.Forms.LinkLabel();
            this.lblTrackName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbLogo
            // 
            this.pbLogo.Image = ((System.Drawing.Image)(resources.GetObject("pbLogo.Image")));
            this.pbLogo.Location = new System.Drawing.Point(12, 12);
            this.pbLogo.Name = "pbLogo";
            this.pbLogo.Size = new System.Drawing.Size(84, 84);
            this.pbLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbLogo.TabIndex = 1;
            this.pbLogo.TabStop = false;
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
            this.lblSignInName.Location = new System.Drawing.Point(115, 12);
            this.lblSignInName.Name = "lblSignInName";
            this.lblSignInName.Size = new System.Drawing.Size(332, 23);
            this.lblSignInName.TabIndex = 3;
            this.lblSignInName.Text = "Last.fm Desktop Scrobbler";
            this.lblSignInName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkProfile
            // 
            this.linkProfile.Location = new System.Drawing.Point(112, 35);
            this.linkProfile.Name = "linkProfile";
            this.linkProfile.Size = new System.Drawing.Size(335, 23);
            this.linkProfile.TabIndex = 4;
            this.linkProfile.TabStop = true;
            this.linkProfile.Text = "Click here to view your profile...";
            this.linkProfile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkProfile.Visible = false;
            // 
            // linkSettings
            // 
            this.linkSettings.Location = new System.Drawing.Point(3, 46);
            this.linkSettings.Name = "linkSettings";
            this.linkSettings.Size = new System.Drawing.Size(62, 23);
            this.linkSettings.TabIndex = 5;
            this.linkSettings.TabStop = true;
            this.linkSettings.Text = "Settings...";
            this.linkSettings.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkLogOut
            // 
            this.linkLogOut.Location = new System.Drawing.Point(3, 23);
            this.linkLogOut.Name = "linkLogOut";
            this.linkLogOut.Size = new System.Drawing.Size(62, 23);
            this.linkLogOut.TabIndex = 6;
            this.linkLogOut.TabStop = true;
            this.linkLogOut.Text = "Log Out...";
            this.linkLogOut.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLogOut.Visible = false;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.linkLogIn);
            this.flowLayoutPanel1.Controls.Add(this.linkLogOut);
            this.flowLayoutPanel1.Controls.Add(this.linkSettings);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(462, 12);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(72, 84);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // linkLogIn
            // 
            this.linkLogIn.Location = new System.Drawing.Point(3, 0);
            this.linkLogIn.Name = "linkLogIn";
            this.linkLogIn.Size = new System.Drawing.Size(62, 23);
            this.linkLogIn.TabIndex = 7;
            this.linkLogIn.TabStop = true;
            this.linkLogIn.Text = "Log In...";
            this.linkLogIn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTrackName
            // 
            this.lblTrackName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTrackName.Location = new System.Drawing.Point(112, 58);
            this.lblTrackName.Name = "lblTrackName";
            this.lblTrackName.Size = new System.Drawing.Size(335, 39);
            this.lblTrackName.TabIndex = 8;
            this.lblTrackName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTrackName.UseMnemonic = false;
            // 
            // ScrobblerUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 131);
            this.Controls.Add(this.lblTrackName);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.linkProfile);
            this.Controls.Add(this.lblSignInName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pbLogo);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ScrobblerUi";
            this.Text = "Last.fm Desktop Scrobbler";
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSignInName;
        private System.Windows.Forms.LinkLabel linkProfile;
        private System.Windows.Forms.LinkLabel linkSettings;
        private System.Windows.Forms.LinkLabel linkLogOut;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.LinkLabel linkLogIn;
        private System.Windows.Forms.Label lblTrackName;
    }
}

