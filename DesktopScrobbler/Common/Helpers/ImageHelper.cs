using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace LastFM.Common.Helpers
{
    public class ImageHelper
    {
        public static async Task<Image> LoadImage(string imageLocation)
        {
            Image returnImage = null;
            bool isLocalPath = false;

            if (!String.IsNullOrEmpty(imageLocation))
            {
                isLocalPath = imageLocation.Substring(1, 1) == ":";

                // If there is drive information, we need to load it from the local file system
                if (!isLocalPath)
                {
                    try
                    {
                        WebClient Client = new WebClient();
                        Stream stream = Client.OpenRead(imageLocation);
                        returnImage = Image.FromStream(stream);
                    }
                    catch (Exception ex)
                    {
                        //throw ex;
                    }
                }
                else
                {
                    // Load from a local filestream
                    try
                    {
                        FileInfo currentPathFileInfo = new FileInfo(imageLocation);
                        returnImage = Image.FromFile(currentPathFileInfo.FullName);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            return returnImage;
        }

    }
}
