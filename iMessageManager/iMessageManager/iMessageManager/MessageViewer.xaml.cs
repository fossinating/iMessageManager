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
        public static DependencyProperty TextProperty;
        public static DependencyProperty PhotoPathProperty;
        public static DependencyProperty FromMeProperty;
        public static DependencyProperty DateProperty;

        public String text
        {
            get { return (String)base.GetValue(TextProperty); }
            set { base.SetValue(TextProperty, value); }
        }

        public String photoPath
        {
            get { return (String)base.GetValue(PhotoPathProperty); }
            set { base.SetValue(PhotoPathProperty, value); }
        }

        public Boolean fromMe
        {
            get { return (Boolean)base.GetValue(FromMeProperty); }
            set { base.SetValue(FromMeProperty, value); }
        }

        public String date
        {
            get { return (String)base.GetValue(DateProperty); }
            set { base.SetValue(DateProperty, value); }
        }

        static MessageViewer()
        {
            TextProperty = DependencyProperty.Register("Text", typeof(String), typeof(MessageViewer));
            PhotoPathProperty = DependencyProperty.Register("PhotoPath", typeof(String), typeof(MessageViewer));
            FromMeProperty = DependencyProperty.Register("FromMe", typeof(Boolean), typeof(MessageViewer));
            DateProperty = DependencyProperty.Register("PhotoPath", typeof(String), typeof(MessageViewer));
        }

        public MessageViewer() {
            InitializeComponent();
        }
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
            var command = MessageManager.connection.CreateCommand();
            command.CommandText = "SL";

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

        private double height = -1;

        internal void nominateForNormalization(double v)
        {
            height = v;
        }

        private void ColumnDefinition_Initialized(object sender, EventArgs e)
        {
            if (height != -1)
            {
                ((ScrollViewer)((StackPanel)this.Parent).Parent).ScrollToVerticalOffset(height);
            }
        }
    }
}
