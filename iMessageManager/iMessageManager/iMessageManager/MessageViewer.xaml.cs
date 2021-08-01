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

        // im gonna want to move to loading all the data here instead
        public static DependencyProperty MessageIDProperty;

        public string MessageID
        {
            get { return (string)base.GetValue(MessageIDProperty); }
            set { base.SetValue(MessageIDProperty, value); }
        }

        static MessageViewer()
        {
            MessageIDProperty = DependencyProperty.Register("MessageID", typeof(String), typeof(MessageViewer));
        }

        public MessageViewer() {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var command = MessageManager.messagesConnection.CreateCommand();
            command.CommandText = 
                $@"SELECT text, handle_id, date, is_from_me FROM message
                    WHERE ROWID = '{MessageID}' LIMIT 1";

            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                if (!(reader.IsDBNull(0) || reader.IsDBNull(1) || reader.IsDBNull(2) || reader.IsDBNull(3)))
                {
                    textLabel.Text = reader.GetString(0);
                    timeLabel.Text = Util.cocoaToReadable(reader.GetInt64(2));
                    if (reader.GetBoolean(3))
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
                        Contact contact = ContactsManager.FromHandle(reader.GetInt32(1));
                        if (contact.photo != null && contact.photo.Length > 0)
                        {
                            contactImage.Source = Util.ToImage(contact.photo);
                        }
                    }
                }
            }
            reader.Close();
            command.Dispose();
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
