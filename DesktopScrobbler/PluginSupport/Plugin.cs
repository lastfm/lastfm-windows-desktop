using System;

namespace PluginSupport
{
    // Encapsulation of 'plugin' (instance of depending injection)
    public class Plugin
    {
        // The identifier of the plugin
        public string Identifier { get; set; }

        // The path to the file containing the plugin
        public string Location { get; set; }

        // The instantiation of the plugin
        public object PluginInstance { get; set; }

        // The contructor for housing a plugin
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
