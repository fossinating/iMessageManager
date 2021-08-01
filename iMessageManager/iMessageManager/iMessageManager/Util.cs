using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace iMessageManager
{
    abstract class Util
    {
        public static string cocoaToReadable(long timestamp)
        {
            DateTime dateTime = new DateTime(2001, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dateTime.AddTicks(timestamp/100).ToLocalTime().ToString();
        }
        public static BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
    }
}
