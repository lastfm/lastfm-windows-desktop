using LastFM.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using LastFM.Common.Localization;

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
            // Check that there isn't already an instance of this application running
            if (Process.GetProcesses().Count(p => p.ProcessName == Process.GetCurrentProcess().ProcessName) == 1)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ScrobblerUi());
            }
            else
            {
                MessageBox.Show(LocalizationStrings.Application_InstanceAlreadyRunning, Core.APPLICATION_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
