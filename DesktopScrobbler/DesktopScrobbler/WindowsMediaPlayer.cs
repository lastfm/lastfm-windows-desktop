using System.Windows.Forms;
using WindowsMediaPlayerScrobblePlugin;

namespace DesktopScrobbler
{
    public partial class WindowsMediaPlayer : Form
    {
        private RemotedWindowsMediaPlayer _mediaPlayer = null;

        public RemotedWindowsMediaPlayer Player { get { return _mediaPlayer; } }

        public WindowsMediaPlayer()
        {
            InitializeComponent();

            _mediaPlayer = new RemotedWindowsMediaPlayer();
            _mediaPlayer.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Controls.Add(_mediaPlayer);
        }
    }
}
