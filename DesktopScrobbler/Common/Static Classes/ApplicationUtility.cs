using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LastFM.Common.Static_Classes
{
    public static class ApplicationUtility
    {
        private static readonly Assembly _applicationAssembly = ApplicationUtility.GetApplicationAssembly();
        private static readonly string _applicationBinFolder = ApplicationUtility.GetApplicationBinFolder();

        public static Assembly ApplicationAssembly
        {
            get
            {
                return ApplicationUtility._applicationAssembly;
            }
        }

        public static string ApplicationBinFolder
        {
            get
            {
                return ApplicationUtility._applicationBinFolder;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        public static bool ApplicationIsActivated()
        {
            IntPtr foregroundWindow = ApplicationUtility.GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
            {
                return false;
            }

            int id = Process.GetCurrentProcess().Id;
            int processId;

            ApplicationUtility.GetWindowThreadProcessId(foregroundWindow, out processId);

            return processId == id;
        }

        public static string ApplicationPath()
        {
            return new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
        }

        public static ApplicationUtility.VersionState CompareVersions(string applicationVersion, string websiteVersion)
        {
            ApplicationUtility.VersionState versionState = ApplicationUtility.VersionState.Same;
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

        private static List<string> GetAssemblyFiles(string sourceFolder, string[] exts, SearchOption searchOption)
        {
            return Directory.EnumerateFiles(sourceFolder, "*.*", SearchOption.AllDirectories).Where<string>((Func<string, bool>)(file =>
            {
                if (!file.EndsWith(".exe"))
                    return file.EndsWith(".dll");
                return true;
            })).ToList<string>();
        }

        public static List<string> GetAssemblyFiles()
        {
            string[] exts = new string[2] { "*.exe", "*.dll" };
            return ApplicationUtility.GetAssemblyFiles(ApplicationUtility._applicationBinFolder, exts, SearchOption.AllDirectories);
        }

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

        public static string GetApplicationVersionNumber()
        {
            return ApplicationUtility.ApplicationAssembly.GetName().Version.ToString(3);
        }

        public static string GetApplicationVersionNumber(Assembly sourceAssembly)
        {
            return sourceAssembly.GetName().Version.ToString(3);
        }

        public static string GetAssemblyVersionFromType(Type type)
        {
            return type.Assembly.GetName().Version.ToString(3);
        }

        public static bool ApplicationIsDebugBuild()
        {
            return ApplicationUtility.AssemblyIsDebugBuild(ApplicationUtility.ApplicationAssembly);
        }

        public static bool AssemblyIsDebugBuild(Assembly assembly)
        {
            return assembly.GetCustomAttributes(false).OfType<DebuggableAttribute>().Select<DebuggableAttribute, bool>((Func<DebuggableAttribute, bool>)(attr => attr.IsJITTrackingEnabled)).FirstOrDefault<bool>();
        }

        private static string GetApplicationBinFolder()
        {
            return Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(ApplicationUtility.ApplicationAssembly.CodeBase).Path));
        }

        private static Assembly GetApplicationAssembly()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            if ((object) assembly == null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }
            return assembly;
        }

        public static string BuildVersion()
        {
            return ApplicationUtility.GetApplicationVersionNumber();
        }

        public static string BuildVersion(Assembly sourceAssembly)
        {
            return ApplicationUtility.GetApplicationVersionNumber(sourceAssembly);
        }

        public static object GetExecutingAssemblyPath()
        {
            return (object)new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName;
        }

        public enum VersionState
        {
            WebIsNewer,
            Same,
            WebIsOlder,
        }
    }
}
