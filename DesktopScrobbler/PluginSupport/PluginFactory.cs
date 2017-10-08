using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PluginSupport
{
    // A factory dedicated to the identifying, and instantiation of external plugins from a starting directory
	public static class PluginFactory
	{
        // Get a list of plugins matching, or derived from, a base class type 
		[DebuggerStepThrough]
		public static async Task<List<Plugin>> GetPluginsOfType(string directoryLocation, Type targetType, bool includeTypeItself = false)
        {
            // Create a list to return
			List<Plugin> AvailablePlugins = new List<Plugin>();

            // Set the searching patter to ONLY include DLLs
			string searchPattern = "*.dll";

            // Get a list of files matching the search pattern from the starting directory (including all sub-drectories)
            string[] pluginFiles = Directory.GetFiles(directoryLocation, searchPattern, SearchOption.AllDirectories);

            // Iterate each of the matching files
			foreach (string fileLocation in pluginFiles)
            try
            {
                // Assume that the file is a .NET assembly and attempt to load it from the file
				Assembly pluginAssembly = Assembly.LoadFrom(fileLocation);

                // Build an identifier based on the full name of the assembly
                string pluginIdentifier = pluginAssembly.FullName.Substring(0, pluginAssembly.FullName.IndexOf(","));

                // Iterate all of the public types housed in the assembly
                foreach (Type reflectedType in pluginAssembly.GetTypes().OrderBy(item => item.Name).ToList())
                {
                    // We're only interested in anything that's a class (yes, you could filter it in the lambda above)
                    if (reflectedType.IsClass)
                    {
                        // Determine if the class we're looking at, contains the type name we're looking for
	                    bool containsType = reflectedType.GetInterfaces().Count(item => item.Name == targetType.Name) > 0;
                        
                        // If an explicit instance is there, or if there's an instance that is derived from the type we're looking for
						if ((reflectedType.BaseType != null && reflectedType.BaseType == targetType) || targetType.IsAssignableFrom(reflectedType) || containsType)
						{
                            // Add it if it's the type (and we're looking for that type) or if it's derived from the type that we are looking for
							bool canAdd = reflectedType != targetType || includeTypeItself;

							if (canAdd)
							{
                                // Add the instance to the plugin list
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
