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
        public static DependencyProperty ContactProperty;
        public static DependencyProperty DateProperty;
        public static DependencyProperty FromMeProperty;

        public string Text
        {
            get { return (string)base.GetValue(TextProperty); }
            set { base.SetValue(TextProperty, value); }
        }

        public Contact Contact
        {
            get => (Contact)base.GetValue(ContactProperty);
            set { base.SetValue(ContactProperty, value); }
        }

        public long Date
        {
            get { return (long)base.GetValue(DateProperty); }
            set { base.SetValue(DateProperty, value); }
        }

        public bool FromMe
        {
            get { return (bool)base.GetValue(FromMeProperty); }
            set { base.SetValue(FromMeProperty, value); }
        }

        static MessageViewer()
        {
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(MessageViewer));
            ContactProperty = DependencyProperty.Register("Contact", typeof(Contact), typeof(MessageViewer));
            DateProperty = DependencyProperty.Register("Date", typeof(long), typeof(MessageViewer));
            FromMeProperty = DependencyProperty.Register("FromMe", typeof(bool), typeof(MessageViewer));
        }

        public MessageViewer() {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            textLabel.Text = Text;
            timeLabel.Text = Util.cocoaToReadable(Date);
            if (FromMe)
            {
                // if is from me
                SolidColorBrush myBrush = new SolidColorBrush();
                myBrush.Color = Color.FromRgb(28, 77, 255);
                textBackground.Background = myBrush;
                textLabel.TextAlignment = TextAlignment.Right;
                textDropShadow.Direction = 315;
            }
            else
            {
                // if is not from me
                SolidColorBrush myBrush = new SolidColorBrush();
                myBrush.Color = Color.FromRgb(46, 46, 46);
                textBackground.Background = myBrush;
                textLabel.TextAlignment = TextAlignment.Left;
                textDropShadow.Direction = 225;
                if (Contact.photo != null && Contact.photo.Length > 0)
                {
                    contactImage.Source = Util.ToImage(Contact.photo);
                }
            }
        }
    }
}
