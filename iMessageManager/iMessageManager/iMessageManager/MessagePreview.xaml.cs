using iMessageManager.Pages;
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
    /// Interaction logic for MessagePreview.xaml
    /// </summary>
    public partial class MessagePreview : UserControl
    {
        public static DependencyProperty MessageIDProperty;
        private Message messageReference;

        public int MessageID
        {
            get => (int)GetValue(MessageIDProperty);
            set => SetValue(MessageIDProperty, value);
        }

        static MessagePreview()
        {
            MessageIDProperty = DependencyProperty.Register("MessageID", typeof(int), typeof(MessagePreview));
        }

        public MessagePreview()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (messageReference == null)
            {
                messageReference = MessageManager.GetMessage(MessageID);
            }
            conversationNameLabel.Content = messageReference.Conversation.Name;
            messageLabel.Content = messageReference.Text;
            timeLabel.Content = Util.cocoaToReadable(messageReference.Date);

            if (messageReference.Conversation.Members.Count > 1)
            {
                List<string> usedNames = new();

                tryUseContact(messageReference.Contact, ref usedNames);
                tryUseContact(messageReference.Contact, ref usedNames, true);

                foreach (Contact contact in messageReference.Conversation.Members)
                {
                    if (usedNames.Count >= 4)
                    {
                        break;
                    }
                    tryUseContact(contact, ref usedNames);
                }
                foreach (Contact contact in messageReference.Conversation.Members)
                {
                    if (usedNames.Count >= 4)
                    {
                        break;
                    }
                    tryUseContact(contact, ref usedNames, true);
                }
            } else
            {
                if (messageReference.Conversation.Members[0] != null)
                {
                    contactGrid.ColumnDefinitions.Clear();
                    contactGrid.RowDefinitions.Clear();
                    contactImageTL.Source = messageReference.Conversation.Members[0].getPhoto();
                }
            }
        }

        private void tryUseContact(Contact contact, ref List<string> usedNames)
        {
            tryUseContact(contact, ref usedNames, false);
        }

        private void tryUseContact(Contact contact, ref List<string> usedNames, bool useName)
        {
            Image[] images = new Image[] { contactImageTL, contactImageBR, contactImageTR, contactImageBL };
            
            if (contact != null && !usedNames.Contains(contact.displayName) && ((contact.photo != null && contact.photo.Length > 0) || useName))
            {
                images[usedNames.Count].Source = contact.getPhoto();
                usedNames.Add(contact.displayName);
            }
        }
    }
}
