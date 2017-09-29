using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopScrobbler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (Process.GetProcesses().Count(p => p.ProcessName == Process.GetCurrentProcess().ProcessName) == 1)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ScrobblerUi());
            }
            else
            {
                MessageBox.Show("You already have an instance of the Desktop Scrobbler running.", "LastFM Desktop Scrobbler", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }


        }
    }
}
