using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace iMessageManager
{
    /// <summary>
    /// Interaction logic for MessageViewer.xaml
    /// </summary>
    public partial class MessageViewer : UserControl
    {
        private string text;
        private string photoPath;
        private bool fromMe;
        private string date;
        public MessageViewer(string text, string photoPath, bool fromMe, string date)
        {
            this.text = text;
            this.photoPath = photoPath;
            this.fromMe = fromMe;
            this.date = date;
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            textLabel.Text = text;
            timeLabel.Text = date;
            if (this.fromMe)
            {
                SolidColorBrush myBrush = new SolidColorBrush();
                myBrush.Color = Color.FromRgb(28, 77, 255);
                textBackground.Background = myBrush;
                textLabel.TextAlignment = TextAlignment.Right;
                textDropShadow.Direction = 315;
            }
            else
            {
                SolidColorBrush myBrush = new SolidColorBrush();
                myBrush.Color = Color.FromRgb(46, 46, 46);
                textBackground.Background = myBrush;
                textLabel.TextAlignment = TextAlignment.Left;
                textDropShadow.Direction = 225;
            }
            if (photoPath != null && photoPath != "")
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(photoPath, UriKind.Absolute);
                image.EndInit();
                contactImage.Source = image;
            }
        }
    }
}
