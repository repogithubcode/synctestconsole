using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ProEstimator.Business.Logic
{
    public class ImageUtilities
    {
        public static Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            var resizedImg = new Bitmap(width, height);
            double ratioX = (double)resizedImg.Width / (double)image.Width;
            double ratioY = (double)resizedImg.Height / (double)image.Height;
            double ratio = ratioX < ratioY ? ratioX : ratioY;

            int newHeight = Convert.ToInt32(image.Height * ratio);
            int newWidth = Convert.ToInt32(image.Width * ratio);

            using (Graphics g = Graphics.FromImage(resizedImg))
            {
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return resizedImg;
        }

        public static string GetImageAsBase64String(Bitmap image, bool resize = false, int resizeWidth = 500, int resizeHeight = 500)
        {
            var result = "";
            if (image != null)
            {
                using (var stream = new MemoryStream())
                {
                    if (resize)
                        ResizeImage(image, resizeWidth, resizeHeight).Save(stream, ImageFormat.Png);
                    else
                        image.Save(stream, ImageFormat.Png);

                    stream.Close();
                    result = Convert.ToBase64String(stream.ToArray());
                }
            }

            return result;
        }
    }
}
