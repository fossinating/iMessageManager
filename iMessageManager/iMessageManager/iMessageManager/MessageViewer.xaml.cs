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
        public static DependencyProperty FirstNameProperty;
        public static DependencyProperty LastNameProperty;
        public static DependencyProperty PhotoProperty;

        public string FirstName
        {
            get => (string)base.GetValue(FirstNameProperty);
            set { base.SetValue(FirstNameProperty, value); }
        }

        public string LastName
        {
            get { return (string)base.GetValue(LastNameProperty); }
            set { base.SetValue(LastNameProperty, value); }
        }

        public byte[] Photo
        {
            get { return (byte[])base.GetValue(PhotoProperty); }
            set { base.SetValue(PhotoProperty, value); }
        }

        static MessageViewer()
        {
            FirstNameProperty = DependencyProperty.Register("FirstName", typeof(string), typeof(MessageViewer));
            LastNameProperty = DependencyProperty.Register("LastName", typeof(string), typeof(MessageViewer));
            PhotoProperty = DependencyProperty.Register("Photo", typeof(byte[]), typeof(MessageViewer));
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
