namespace nokako
{
    partial class FormSetting
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSetting));
            textBoxNsec = new TextBox();
            trackBarOpacity = new TrackBar();
            checkBoxTopMost = new CheckBox();
            label1 = new Label();
            checkBoxAddClient = new CheckBox();
            label4 = new Label();
            linkLabelIcons8 = new LinkLabel();
            labelOpacity = new Label();
            checkBoxShowOnlyFollowees = new CheckBox();
            label3 = new Label();
            linkLabelVersion = new LinkLabel();
            checkBoxMinimizeToTray = new CheckBox();
            label2 = new Label();
            textBoxNpub = new TextBox();
            buttonLogOut = new Button();
            toolTipLogOut = new ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)trackBarOpacity).BeginInit();
            SuspendLayout();
            // 
            // textBoxNsec
            // 
            textBoxNsec.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxNsec.BorderStyle = BorderStyle.FixedSingle;
            textBoxNsec.ImeMode = ImeMode.Disable;
            textBoxNsec.Location = new Point(82, 132);
            textBoxNsec.MaxLength = 256;
            textBoxNsec.Name = "textBoxNsec";
            textBoxNsec.PasswordChar = '*';
            textBoxNsec.PlaceholderText = "nsec1...";
            textBoxNsec.Size = new Size(164, 23);
            textBoxNsec.TabIndex = 10;
            textBoxNsec.Leave += TextBoxNsec_Leave;
            // 
            // trackBarOpacity
            // 
            trackBarOpacity.Location = new Point(152, 31);
            trackBarOpacity.Maximum = 100;
            trackBarOpacity.Minimum = 20;
            trackBarOpacity.Name = "trackBarOpacity";
            trackBarOpacity.Size = new Size(120, 45);
            trackBarOpacity.TabIndex = 2;
            trackBarOpacity.TickFrequency = 20;
            trackBarOpacity.Value = 100;
            trackBarOpacity.Scroll += TrackBarOpacity_Scroll;
            // 
            // checkBoxTopMost
            // 
            checkBoxTopMost.AutoSize = true;
            checkBoxTopMost.Location = new Point(12, 12);
            checkBoxTopMost.Name = "checkBoxTopMost";
            checkBoxTopMost.Size = new Size(101, 19);
            checkBoxTopMost.TabIndex = 1;
            checkBoxTopMost.Text = "Always on top";
            checkBoxTopMost.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(152, 13);
            label1.Name = "label1";
            label1.Size = new Size(48, 15);
            label1.TabIndex = 0;
            label1.Text = "Opacity";
            // 
            // checkBoxAddClient
            // 
            checkBoxAddClient.AutoSize = true;
            checkBoxAddClient.Checked = true;
            checkBoxAddClient.CheckState = CheckState.Checked;
            checkBoxAddClient.Location = new Point(12, 107);
            checkBoxAddClient.Name = "checkBoxAddClient";
            checkBoxAddClient.Size = new Size(100, 19);
            checkBoxAddClient.TabIndex = 9;
            checkBoxAddClient.Text = "Add client tag";
            checkBoxAddClient.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label4.AutoSize = true;
            label4.ForeColor = SystemColors.GrayText;
            label4.Location = new Point(99, 237);
            label4.Name = "label4";
            label4.Size = new Size(126, 15);
            label4.TabIndex = 0;
            label4.Text = "Monochrome icons by";
            // 
            // linkLabelIcons8
            // 
            linkLabelIcons8.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkLabelIcons8.AutoSize = true;
            linkLabelIcons8.Location = new Point(231, 237);
            linkLabelIcons8.Name = "linkLabelIcons8";
            linkLabelIcons8.Size = new Size(41, 15);
            linkLabelIcons8.TabIndex = 11;
            linkLabelIcons8.TabStop = true;
            linkLabelIcons8.Text = "Icons8";
            linkLabelIcons8.LinkClicked += LinkLabelIcons8_LinkClicked;
            // 
            // labelOpacity
            // 
            labelOpacity.Location = new Point(231, 13);
            labelOpacity.Name = "labelOpacity";
            labelOpacity.Size = new Size(41, 15);
            labelOpacity.TabIndex = 0;
            labelOpacity.Text = "100%";
            labelOpacity.TextAlign = ContentAlignment.TopRight;
            // 
            // checkBoxShowOnlyFollowees
            // 
            checkBoxShowOnlyFollowees.AutoSize = true;
            checkBoxShowOnlyFollowees.Location = new Point(12, 37);
            checkBoxShowOnlyFollowees.Name = "checkBoxShowOnlyFollowees";
            checkBoxShowOnlyFollowees.Size = new Size(134, 19);
            checkBoxShowOnlyFollowees.TabIndex = 4;
            checkBoxShowOnlyFollowees.Text = "Show only followees";
            checkBoxShowOnlyFollowees.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 134);
            label3.Name = "label3";
            label3.Size = new Size(64, 15);
            label3.TabIndex = 0;
            label3.Text = "Private key";
            // 
            // linkLabelVersion
            // 
            linkLabelVersion.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkLabelVersion.AutoSize = true;
            linkLabelVersion.Location = new Point(12, 237);
            linkLabelVersion.Name = "linkLabelVersion";
            linkLabelVersion.Size = new Size(37, 15);
            linkLabelVersion.TabIndex = 10;
            linkLabelVersion.TabStop = true;
            linkLabelVersion.Text = "v0.2.2";
            linkLabelVersion.LinkClicked += LinkLabelVersion_LinkClicked;
            // 
            // checkBoxMinimizeToTray
            // 
            checkBoxMinimizeToTray.AutoSize = true;
            checkBoxMinimizeToTray.Location = new Point(12, 82);
            checkBoxMinimizeToTray.Name = "checkBoxMinimizeToTray";
            checkBoxMinimizeToTray.Size = new Size(150, 19);
            checkBoxMinimizeToTray.TabIndex = 8;
            checkBoxMinimizeToTray.Text = "Minimize to system tray";
            checkBoxMinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 163);
            label2.Name = "label2";
            label2.Size = new Size(61, 15);
            label2.TabIndex = 15;
            label2.Text = "Public key";
            // 
            // textBoxNpub
            // 
            textBoxNpub.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxNpub.BorderStyle = BorderStyle.FixedSingle;
            textBoxNpub.Location = new Point(82, 161);
            textBoxNpub.Name = "textBoxNpub";
            textBoxNpub.PlaceholderText = "npub1...";
            textBoxNpub.ReadOnly = true;
            textBoxNpub.Size = new Size(190, 23);
            textBoxNpub.TabIndex = 12;
            // 
            // buttonLogOut
            // 
            buttonLogOut.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonLogOut.Image = Properties.Resources.icons8_log_out_16;
            buttonLogOut.Location = new Point(249, 132);
            buttonLogOut.Name = "buttonLogOut";
            buttonLogOut.Size = new Size(23, 23);
            buttonLogOut.TabIndex = 11;
            toolTipLogOut.SetToolTip(buttonLogOut, "Log out");
            buttonLogOut.UseVisualStyleBackColor = true;
            buttonLogOut.Click += ButtonLogOut_Click;
            // 
            // FormSetting
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(284, 261);
            Controls.Add(buttonLogOut);
            Controls.Add(textBoxNpub);
            Controls.Add(label2);
            Controls.Add(checkBoxMinimizeToTray);
            Controls.Add(linkLabelVersion);
            Controls.Add(label3);
            Controls.Add(checkBoxShowOnlyFollowees);
            Controls.Add(labelOpacity);
            Controls.Add(linkLabelIcons8);
            Controls.Add(label4);
            Controls.Add(checkBoxAddClient);
            Controls.Add(label1);
            Controls.Add(checkBoxTopMost);
            Controls.Add(trackBarOpacity);
            Controls.Add(textBoxNsec);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(300, 300);
            Name = "FormSetting";
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Show;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Setting";
            TopMost = true;
            Load += FormSetting_Load;
            Shown += FormSetting_Shown;
            KeyDown += FormSetting_KeyDown;
            ((System.ComponentModel.ISupportInitialize)trackBarOpacity).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        internal TextBox textBoxNsec;
        internal TrackBar trackBarOpacity;
        internal CheckBox checkBoxTopMost;
        private Label label1;
        internal CheckBox checkBoxAddClient;
        private Label label4;
        private LinkLabel linkLabelIcons8;
        private Label labelOpacity;
        internal CheckBox checkBoxShowOnlyFollowees;
        private Label label3;
        private LinkLabel linkLabelVersion;
        internal CheckBox checkBoxMinimizeToTray;
        private Label label2;
        internal TextBox textBoxNpub;
        private Button buttonLogOut;
        private ToolTip toolTipLogOut;
    }
}