using System;
using System.Windows.Forms;
using WindowsMediaPlayerScrobblePlugin;

namespace DesktopScrobbler
{
    // This is a host form for the AXImport'ed Windows Media Library (wmp.dll)
    // It's sole purpose is to give us somewhere to render the shared control, and track the status of the media player

    public partial class WindowsMediaPlayer : Form
    {
        // Localised instance of the media player control
        private RemotedWindowsMediaPlayer _mediaPlayer = null;

        // Exposed property to allow anything to interact with the hosted media player
        public RemotedWindowsMediaPlayer Player { get { return _mediaPlayer; } }

        // Constructor for the host form
        public WindowsMediaPlayer()
        {
            InitializeComponent();

            try
            {
                // Create a new instance of the Windows Media Player Remoted control
                _mediaPlayer = new RemotedWindowsMediaPlayer();

                // Set it to fill the available space on the container it gets added to
                _mediaPlayer.Dock = System.Windows.Forms.DockStyle.Fill;

                // Add it to the panel that consumes this form
                panel1.Controls.Add(_mediaPlayer);
            }
            catch (Exception e)
            {
                // Can occur if you close the application whilst it's starting up...
            }
        }
    }
}
