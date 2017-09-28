using System;
using System.Collections.Generic;
using System.IO;

namespace LastFM.Common.Helpers
{
    public static class PathHelper
    {
        public static List<Exception> CheckPaths(params string[] directoryPaths)
        {
            List<Exception> pathCheckExceptions = null;

            foreach (string directoryPath in directoryPaths)
            {
                if (!Directory.Exists(directoryPath))
                {
                    try
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    catch (Exception e)
                    {
                        if (pathCheckExceptions == null)
                        {
                            pathCheckExceptions = new List<Exception>();
                        }

                        pathCheckExceptions.Add(new Exception($"Failed to create the path {directoryPath}"));
                    }
                }
            }
            return pathCheckExceptions;
        }
    }
}
