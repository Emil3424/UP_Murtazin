using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace UP_Murtazin.Helpers
{
    public static class ImageHelper
    {
        public static BitmapImage ConvertBase64ToImage(string base64String)
        {
            try
            {
                string base64Data = base64String;
                if (base64String.Contains(","))
                {
                    base64Data = base64String.Substring(base64String.IndexOf(",") + 1);
                }

                byte[] imageBytes = Convert.FromBase64String(base64Data);

                using (var stream = new MemoryStream(imageBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
