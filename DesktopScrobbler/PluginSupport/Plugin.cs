using System;

namespace PluginSupport
{
    public class Plugin
    {
        public string Identifier { get; set; }
        public string Location { get; set; }
        public object PluginInstance { get; set; }

        public Plugin(string identifier, string location, Type pluginType)
        {
            this.Identifier = identifier;
            this.Location = location;

            try
            {
                PluginInstance = Activator.CreateInstance(pluginType);
            }
            catch (Exception)
            {
            }
        }
    }
}
