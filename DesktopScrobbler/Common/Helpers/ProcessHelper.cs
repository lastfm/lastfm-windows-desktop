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
    }
}
