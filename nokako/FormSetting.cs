using nokakoiCrypt;
using System.Diagnostics;

namespace nokako
{
    public partial class FormSetting : Form
    {
        public FormSetting()
        {
            InitializeComponent();
            textBoxNokakoiKey.PlaceholderText = NokakoiCrypt.NokakoiTag + " . . .";
        }

        private void FormSetting_Load(object sender, EventArgs e)
        {
            labelOpacity.Text = $"{trackBarOpacity.Value}%";
        }

        private void FormSetting_Shown(object sender, EventArgs e)
        {
            textBoxPassword.Focus();
        }

        private void FormSetting_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        private void TrackBarOpacity_Scroll(object sender, EventArgs e)
        {
            labelOpacity.Text = $"{trackBarOpacity.Value}%";
            if (Owner != null)
            {
                Owner.Opacity = trackBarOpacity.Value / 100.0;
            }
        }

        private void LinkLabelIcons8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabelIcons8.LinkVisited = true;
            var app = new ProcessStartInfo
            {
                FileName = "https://icons8.com",
                UseShellExecute = true
            };
            Process.Start(app);
        }

        private void LinkLabelVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabelVersion.LinkVisited = true;
            var app = new ProcessStartInfo
            {
                FileName = "https://github.com/betonetojp/nokako",
                UseShellExecute = true
            };
            Process.Start(app);
        }
    }
}
