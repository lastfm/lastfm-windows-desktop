using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PluginSupport
{
	public static class PluginFactory
	{
		[DebuggerStepThrough]
		public static async Task<List<Plugin>> GetPluginsOfType(string directoryLocation, Type targetType, bool includeTypeItself = false)
        {
			List<Plugin> AvailablePlugins = new List<Plugin>();
			string searchPattern = "*.dll";

            string[] pluginFiles = Directory.GetFiles(directoryLocation, searchPattern, SearchOption.AllDirectories);

			foreach (string fileLocation in pluginFiles)
            try
            {
				Assembly pluginAssembly = Assembly.LoadFrom(fileLocation);

                string pluginIdentifier = pluginAssembly.FullName.Substring(0, pluginAssembly.FullName.IndexOf(","));

                foreach (Type reflectedType in pluginAssembly.GetTypes().OrderBy(item => item.Name).ToList())
                {
                    if (reflectedType.IsClass)
                    {
	                    bool containsType = reflectedType.GetInterfaces().Count(item => item.Name == targetType.Name) > 0;

						if ((reflectedType.BaseType != null && reflectedType.BaseType == targetType) || targetType.IsAssignableFrom(reflectedType) || containsType)
						{
							bool canAdd = reflectedType != targetType || includeTypeItself;

							if (canAdd)
							{
								AvailablePlugins.Add(new Plugin(pluginIdentifier, fileLocation, reflectedType));
							}
						}
                    }
                }
            }
            catch (Exception ex)
            {
            }

			return AvailablePlugins;
        }
	}
}
