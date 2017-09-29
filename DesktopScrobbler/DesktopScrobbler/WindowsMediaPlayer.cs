using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
