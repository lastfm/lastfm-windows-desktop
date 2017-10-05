using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LastFM.Common.Helpers
{
    public static class ProcessHelper
    {
        public static async Task LaunchUrl(string launchUrl)
        {
            var launchedProcess = Process.Start(launchUrl);
        }

        internal static async Task LaunchProcess(string processName, string arguments = null)
        {
            var launchedProcess = Process.Start(processName, arguments);
        }
    }
}
