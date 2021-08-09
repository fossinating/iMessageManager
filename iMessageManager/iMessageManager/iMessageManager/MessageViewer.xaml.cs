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
        public static DependencyProperty MessageIDProperty;
        private Message messageReference;

        public int MessageID
        {
            get => (int)GetValue(MessageIDProperty);
            set => SetValue(MessageIDProperty, value);
        }

        static MessageViewer()
        {
            MessageIDProperty = DependencyProperty.Register("MessageID", typeof(int), typeof(MessageViewer));
        }

        public MessageViewer() {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (messageReference == null)
            {
                messageReference = MessageManager.GetMessage(MessageID);
            }
            textLabel.Text = messageReference.Text;
            timeLabel.Text = Util.cocoaToReadable(messageReference.Date);
            if (messageReference.FromMe)
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
                contactImage.Source = messageReference.Contact.getPhoto();
            }
        }
    }
}
