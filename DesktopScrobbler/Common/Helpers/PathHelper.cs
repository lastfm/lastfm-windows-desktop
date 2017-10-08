using System;
using System.Collections.Generic;
using System.IO;

namespace LastFM.Common.Helpers
{
    // General helper for dealing with IO paths
    public static class PathHelper
    {
        // Utility functions for checking that the specfied directories have been created.
        // If not, then attempt to create them
        public static List<Exception> CheckPaths(params string[] directoryPaths)
        {
            // A stack of errors that have occured when trying to create paths that do not yet exist
            List<Exception> pathCheckExceptions = null;

            // Iterate each of the specified paths
            foreach (string directoryPath in directoryPaths)
            {
                // Check to see if the path exists
                if (!Directory.Exists(directoryPath))
                {
                    try
                    {                        
                        // Try to create the directory
                        Directory.CreateDirectory(directoryPath);
                    }
                    catch (Exception e)
                    {
                        // Creation failed, make sure we initialise the exception stack
                        if (pathCheckExceptions == null)
                        {
                            pathCheckExceptions = new List<Exception>();
                        }

                        // Add the current failure reason to the stack, inidicating which path failed
                        pathCheckExceptions.Add(new Exception($"Failed to create the path {directoryPath}"));
                    }
                }
            }

            // Return the stack of exceptions (if any)
            return pathCheckExceptions;
        }
    }
}
