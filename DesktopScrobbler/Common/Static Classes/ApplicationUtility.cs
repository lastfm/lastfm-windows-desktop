using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LastFM.Common.Static_Classes
{
    // General utility class specifically geared to versioning and application instancing
    public static class ApplicationUtility
    {
        // Human-readable definition of the result of a version check
        public enum VersionState
        {
            WebIsNewer,
            Same,
            WebIsOlder,
        }

        // Win32 API definition for finding the window the user current has active
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetForegroundWindow();

        // Win32 API delcaration for getting the Process Id of a specified window handle (which could also be a control)
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        // Internal instance of the application instance (EXE) that is currently running and using this DLL
        private static readonly Assembly _applicationAssembly = ApplicationUtility.GetApplicationAssembly();

        // Internal instance of the path associated with the application instance (EXE) that is currently running and using this DLL
        private static readonly string _applicationBinFolder = ApplicationUtility.GetApplicationBinFolder();

        // Helper function to get the application instance (EXE) that is currently running and using this DLL
        public static Assembly ApplicationAssembly
        {
            get
            {
                return ApplicationUtility._applicationAssembly;
            }
        }

        // Helper function to get the path associated with the application instance (EXE) that is currently running and using this DLL
        public static string ApplicationBinFolder
        {
            get
            {
                return ApplicationUtility._applicationBinFolder;
            }
        }

        // Helper function to determine if the current EXE has the focus
        public static bool ApplicationIsActivated()
        {
            bool isApplicationActive = false;

            // Find the window handle of the window the user currently has activated
            IntPtr foregroundWindow = ApplicationUtility.GetForegroundWindow();

            // If the user has a window active
            if (foregroundWindow != IntPtr.Zero)
            {
                // Get the process Id of the application currently referencing this DLL
                int id = Process.GetCurrentProcess().Id;
                int processId;

                // Get the process Id of the application that the user is currently interacting with
                ApplicationUtility.GetWindowThreadProcessId(foregroundWindow, out processId);

                // If Windows says the user is using our application, then it's active
                isApplicationActive = processId == id;
            }

            return isApplicationActive;
        }

        // Helper function for getting the current executable path (which directory it was launched from)
        public static string ApplicationPath()
        {
            return new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
        }

        // Helper function for comparing the currently running application version with the lastest version number
        // retrieved, say from a website
        public static ApplicationUtility.VersionState CompareVersions(string applicationVersion, string websiteVersion)
        {
            // Default to the version numbers being the same
            ApplicationUtility.VersionState versionState = ApplicationUtility.VersionState.Same;

            // Strip out the version number 'dots' to create a single integer number and compare the two
            int num = new Version(applicationVersion.Replace(",", ".")).CompareTo(new Version(websiteVersion.Replace(",", ".")));
            if (num < 0)
            {
                versionState = ApplicationUtility.VersionState.WebIsNewer;
            }
            else if (num > 0)
            {
                versionState = ApplicationUtility.VersionState.WebIsOlder;
            }
            return versionState;
        }

        // Helper function for getting a list of satellite DLLs (useful for such things as dependency injection)
        // from the specified source folder
        private static List<string> GetAssemblyFiles(string sourceFolder, string[] exts, SearchOption searchOption)
        {
            return Directory.EnumerateFiles(sourceFolder, "*.*", SearchOption.AllDirectories).Where<string>((Func<string, bool>)(file =>
            {
                if (!file.EndsWith(".exe"))
                    return file.EndsWith(".dll");
                return true;
            })).ToList<string>();
        }

        // Helper function for getting a list of all EXEs and DLLs in the current application folder
        public static List<string> GetAssemblyFiles()
        {
            string[] exts = new string[2] { "*.exe", "*.dll" };
            return ApplicationUtility.GetAssemblyFiles(ApplicationUtility._applicationBinFolder, exts, SearchOption.AllDirectories);
        }

        // Helper function for getting a list of filename and version numbers of all assembly files (EXEs and DLLs) in the
        // current application folder
        public static List<string> GetApplicationComponentVersions()
        {
            List<string> stringList = new List<string>();
            string[] exts = new string[2] { "*.exe", "*.dll" };
            foreach (string assemblyFile in ApplicationUtility.GetAssemblyFiles(ApplicationUtility._applicationBinFolder, exts, SearchOption.AllDirectories))
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assemblyFile);
                if (!string.IsNullOrEmpty(versionInfo.Comments))
                {
                    string str = string.Format("{0} ({1}) - version: {2}", (object)versionInfo.Comments, (object)versionInfo.InternalName, (object)versionInfo.FileVersion);
                    stringList.Add(str);
                }
            }
            return stringList;
        }

        // A helper function to return the FULL current version of the running executable (including revision number)
        public static string GetApplicationFullVersionNumber()
        {
            return ApplicationUtility.ApplicationAssembly.GetName().Version.ToString(4);
        }

        // Helper function to return Major, Minor and Build version of the currently running executable
        public static string GetApplicationVersionNumber()
        {
            return ApplicationUtility.ApplicationAssembly.GetName().Version.ToString(3);
        }

        // Helper function to return Major, Minor and Build version of the specified assembly
        public static string GetApplicationVersionNumber(Assembly sourceAssembly)
        {
            return sourceAssembly.GetName().Version.ToString(3);
        }

        // Helper function (another one) for getting the application startup path
        // TODO: determine if this is really needed, and replace as appropriate with its potential duplicate
        private static string GetApplicationBinFolder()
        {
            return Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(ApplicationUtility.ApplicationAssembly.CodeBase).Path));
        }

        // Internal function used to get the currently running executable
        private static Assembly GetApplicationAssembly()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            if ((object) assembly == null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }
            return assembly;
        }

        // Helper function to get the current version number (used by the version checker system)
        // TODO: determine if this is un-necessarily duplicated
        public static string BuildVersion()
        {
            return ApplicationUtility.GetApplicationVersionNumber();
        }
    }
}
