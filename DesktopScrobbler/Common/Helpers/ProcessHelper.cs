using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LastFM.Common.Helpers
{
    // General helper for launching external processes
    public static class ProcessHelper
    {
        // Utility function for launching a Url
        public static async Task LaunchUrl(string launchUrl)
        {
            // Send the Url directly to the O/S command line interpreter to deal with
            // resulting in the display of the Url in the user's default browser
            //(Note: this will NOT work for Yandex users, which might upset them)
            var launchedProcess = Process.Start(launchUrl);
        }

        // Utility for launching a process and passing command line arguments (if any)
        internal static async Task LaunchProcess(string processName, string arguments = null)
        {
            // Send the process name directly to the O/S command line interpreter to deal with, passing any arguments
            var launchedProcess = Process.Start(processName, arguments);
        }
    }
}
