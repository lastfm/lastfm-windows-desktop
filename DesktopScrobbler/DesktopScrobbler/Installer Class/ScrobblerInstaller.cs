using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace DesktopScrobbler.Installer_Class
{
    [RunInstaller(true)]
    public partial class ScrobblerInstaller : System.Configuration.Install.Installer
    {
        public ScrobblerInstaller()
        {
            InitializeComponent();
        }

        public override void Commit(IDictionary savedState)
        {
            System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(this.Context.Parameters["AssemblyPath"]) + @"\DesktopScrobbler.exe");
        }
    }
}
