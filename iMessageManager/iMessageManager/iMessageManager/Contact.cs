using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace iMessageManager
{
    public class Contact
    {
        public string displayName;
        public string firstName;
        public string lastName;
        public byte[] photo;

        public Contact(string firstName, string lastName, byte[] photo)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.displayName = firstName + " " + lastName;
            this.photo = photo;
        }

        public Contact(string displayName)
        {
            this.displayName = displayName;
        }

        public Contact(string firstName, string lastName) : this(firstName, lastName, new byte[0]) { }

        public BitmapImage getPhoto()
        {
            if (photo == null || photo.Length == 0) 
            {
                Bitmap bmp = new(500,500);

                RectangleF rectf = new RectangleF(0, 0, bmp.Width, bmp.Height);

                Graphics g = Graphics.FromImage(bmp);

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                StringFormat format = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                string initials = ((firstName != null && firstName.Length > 0) ? firstName.Substring(0, 1) : "") +
                    ((lastName != null && lastName.Length > 0) ? lastName.Substring(0, 1) : "");
                if (initials.Length == 0)
                {
                    initials = "?";
                }
                // Draw the text onto the image
                g.DrawString(initials
                    , new Font("Tahoma", 350 / initials.Length), Brushes.Black, rectf, format);

                g.Flush();

                MemoryStream stream = new MemoryStream();
                bmp.Save(stream, ImageFormat.Png);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();

                return bitmapImage;
            } else
            {
                return Util.ToImage(photo);
            }
        }
    }
}
