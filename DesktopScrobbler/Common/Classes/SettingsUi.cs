using LastFM.Common;
using LastFM.Common.Factories;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Common.Classes
{
    public partial class SettingsUi : Form
    {
        public SettingsUi()
        {
            InitializeComponent();

            this.Load += SettingsUi_Load;
        }

        private void SettingsUi_Load(object sender, EventArgs e)
        {
            chkMinimizeToTray.Checked = Core.Settings.CloseToTray;
            chkStartMinimized.Checked = Core.Settings.StartMinimized;
            chkShowScrobbleNotifications.Checked = Convert.ToBoolean(Core.Settings.ShowScrobbleNotifications);
            chkShowtrackChanges.Checked = Convert.ToBoolean(Core.Settings.ShowTrackChanges);

            foreach(IScrobbleSource plugin in ScrobbleFactory.ScrobblePlugins)
            {
                checkedPluginList.Items.Add(plugin.SourceDescription, Core.Settings.ScrobblerStatus.Count(pluginItem => pluginItem.Identifier == plugin.SourceIdentifier && Convert.ToBoolean(pluginItem.IsEnabled == true)) > 0);
            }

            chkShowScrobbleNotifications.CheckedChanged += (o, ev) => { SettingItem_Changed(); };
            chkShowtrackChanges.CheckedChanged += (o, ev) => { SettingItem_Changed(); };
            chkMinimizeToTray.CheckedChanged += (o, ev) => { SettingItem_Changed(); };
            chkStartMinimized.CheckedChanged += (o, ev) => { SettingItem_Changed(); };
            checkedPluginList.ItemCheck += (o, ev) => { SettingItem_Changed(); };
        }

        private void SettingItem_Changed()
        {
            btnSave.Enabled = true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Core.Settings.CloseToTray = chkMinimizeToTray.Checked;
            Core.Settings.StartMinimized = chkStartMinimized.Checked;
            Core.Settings.ShowScrobbleNotifications = chkShowScrobbleNotifications.Checked;
            Core.Settings.ShowTrackChanges = chkShowtrackChanges.Checked;

            Core.Settings.ScrobblerStatus.Clear();

            foreach (var checkedItem in checkedPluginList.Items)
            {
                IScrobbleSource plugin = ScrobbleFactory.ScrobblePlugins?.FirstOrDefault(item => item.SourceDescription == checkedItem);

                Core.Settings.ScrobblerStatus.Add(new LastFM.Common.Classes.ScrobblerSourceStatus() { Identifier = plugin.SourceIdentifier, IsEnabled = checkedPluginList.CheckedItems.Contains(checkedItem) });
            }

            Core.SaveSettings();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
