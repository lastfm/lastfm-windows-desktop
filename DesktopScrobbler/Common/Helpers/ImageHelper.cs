using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace LastFM.Common.Helpers
{
    // Helper function for performing Image related function
    public class ImageHelper
    {
        // Method for loading an image either from the file system, or from the Web
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

        // Method for taking an image, and turning into greyscale version (e.g. for showing a disabled state)
        public static async Task<Image> GreyScaleImage(Image sourceImage)
        {
            System.Drawing.Image imageToReturn = null;

            try
            {

                Image imageToConvert = new Bitmap(sourceImage);

                using (Graphics g = Graphics.FromImage(imageToConvert))
                {
                    ImageAttributes imageAttr = new ImageAttributes();

                    int width = imageToConvert.Width;
                    int height = imageToConvert.Height;

                    float[][] greyShear = new float[][]
                    {
                        new float[5] {0.5f, 0.5f, 0.5f, 0, 0},
                        new float[5] {0.5f, 0.5f, 0.5f, 0, 0},
                        new float[5] {0.5f, 0.5f, 0.5f, 0, 0},
                        new float[5] {0, 0, 0, 1, 0},
                        new float[5] {0, 0, 0, 0, 1}
                    };

                    ColorMatrix colMatrix = new ColorMatrix(greyShear);
                    imageAttr.SetColorMatrix(colMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    g.DrawImage(imageToConvert, new Rectangle(0, 0, width, height), 0, 0, width, height, System.Drawing.GraphicsUnit.Pixel, imageAttr);

                    imageToReturn = imageToConvert;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return imageToReturn;

        }

        // Method for taking an icon, and turning into a greyscale version (e.g for showing a disabled state)
        public static async Task<Icon> GreyScaleIcon(Icon sourceIcon)
        {
            System.Drawing.Icon imageToReturn = null;

            try
            {

                Bitmap imageToConvert = new Bitmap(sourceIcon.ToBitmap());

                using (Graphics g = Graphics.FromImage(imageToConvert))
                {
                    ImageAttributes imageAttr = new ImageAttributes();

                    int width = imageToConvert.Width;
                    int height = imageToConvert.Height;

                    float[][] greyShear = new float[][]
                    {
                        new float[5] {0.5f, 0.5f, 0.5f, 0, 0},
                        new float[5] {0.5f, 0.5f, 0.5f, 0, 0},
                        new float[5] {0.5f, 0.5f, 0.5f, 0, 0},
                        new float[5] {0, 0, 0, 1, 0},
                        new float[5] {0, 0, 0, 0, 1}
                    };

                    ColorMatrix colMatrix = new ColorMatrix(greyShear);
                    imageAttr.SetColorMatrix(colMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    g.DrawImage(imageToConvert, new Rectangle(0, 0, width, height), 0, 0, width, height, System.Drawing.GraphicsUnit.Pixel, imageAttr);

                    IntPtr iconHandle = imageToConvert.GetHicon();

                    imageToReturn = Icon.FromHandle(iconHandle);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return imageToReturn;

        }
    }
}
