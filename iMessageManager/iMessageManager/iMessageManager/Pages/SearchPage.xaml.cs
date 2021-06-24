using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace iMessageManager.Pages
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class SearchPage : UserControl
    {

        private List<MessagePreview> messagePreviews = new List<MessagePreview>();

        private MainWindow sourceWindow;
        public SearchPage(MainWindow window)
        {
            InitializeComponent();
            this.sourceWindow = window;
        }

        public void select(MessagePreview messagePreview)
        {
            foreach (MessagePreview preview in messagePreviews)
            {
                if (preview != messagePreview)
                {
                    preview.setSelected(false);
                }
            }
            loadConversation(messagePreview.messageID);
        }

        private void unselectAll()
        {
            foreach (MessagePreview messagePreview in messagePreviews)
            {
                messagePreview.setSelected(false);
            }
        }

        private void loadConversation(int messageID)
        {
            var command = MessageManager.connection.CreateCommand();
            command.CommandText =
                $@"
                    SELECT chat_id
                    FROM chat_message_join
                    WHERE message_id == '{messageID}'
                    LIMIT 1
                ";
            var reader = command.ExecuteReader();

            reader.Read();

            int chatID = reader.GetInt32(0);
            reader.Close();
            command.Dispose();

            command = MessageManager.connection.CreateCommand();
            command.CommandText =
                $@"
                    SELECT message_id
                    FROM chat_message_join
                    WHERE chat_id == '{chatID}'
                    ORDER BY message_date ASC
                ";
            reader = command.ExecuteReader();

            int messageIndex = -1;
            List<int> messageIDQueue = new List<int>();
            while (reader.Read())
            {
                messageIDQueue.Add(reader.GetInt32(0));
                if (reader.GetInt32(0) == messageID)
                {
                    messageIndex = messageIDQueue.Count - 1;
                }
            }
            reader.Close();
            command.Dispose();

            List<Message> messages = new List<Message>();
            int targetIndex = -1;

            foreach (int _messageID in messageIDQueue)
            {
                command = MessageManager.connection.CreateCommand();
                command.CommandText =
                    $@"
                        SELECT ROWID, text, handle_id, date, is_from_me, guid
                        FROM message
                        WHERE ROWID == '{_messageID}'
                        LIMIT 1
                    ";
                reader = command.ExecuteReader();

                reader.Read();

                Contact contact;
                // if is from me, then no contact needed
                if (reader.GetInt32(4) == 1)
                {
                    contact = null;
                }
                // otherwise, get id from handle
                else
                {

                    int handle_id = reader.GetInt32(2);

                    if (handle_id == 0)
                    {
                        if (!reader.IsDBNull(1))
                        {
                            MessageBox.Show($"Found a message (id {_messageID}) with handle 0 not listed as from me but it has message body \"{reader.GetString(1)}\"");
                        }
                        else
                        {
                            reader.Close();
                            command.Dispose();
                            continue;
                        }
                    }

                    var hCommand = MessageManager.connection.CreateCommand();

                    hCommand.CommandText =
                        $@"
                        SELECT id
                        FROM handle
                        WHERE ROWID=='{handle_id}'
                    ";

                    var hReader = hCommand.ExecuteReader();

                    bool exists = hReader.Read();

                    contact = ContactsManager.getContact(hReader.GetString(0));
                    hReader.Close();
                    hCommand.Dispose();
                }

                bool valid = true;
                for (int i = 0; i <= 5; i++)
                {
                    if (reader.IsDBNull(i))
                    {
                        //MessageBox.Show($"Found null value at index {i} of messageID {_messageID}");
                        valid = false;
                        reader.Close();
                        command.Dispose();
                        break;
                    }
                }
                if (valid)
                {
                    Message message = new Message(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        contact,
                        reader.GetInt64(3),
                        reader.GetInt32(4) == 1,
                        reader.GetGuid(5));

                    if (messageID == reader.GetInt32(0))
                    {
                        targetIndex = messages.Count();
                    }

                    messages.Add(message);
                }
                reader.Close();
                command.Dispose();
            }

            messagesStackPanel.Children.Clear();
            double offsetToTarget = 0;
            int loadCount = 101;
            int startIndex = Math.Max(0, targetIndex - (loadCount / 2));
            int endIndex = Math.Min(messages.Count()-1, startIndex + loadCount);
            foreach (Message message in messages.GetRange(startIndex, endIndex-startIndex+1))
            {
                MessageViewer messageViewer = message.GetMessageViewer();
                messagesStackPanel.Children.Add(messageViewer);
                if (message.messageID == messageID)
                {
                    offsetToTarget = messagesViewer.ExtentHeight;
                }
            }
            messagesViewer.ScrollToVerticalOffset(offsetToTarget);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void clearMessages()
        {
            messagePreviews.Clear();
            conversationPanel.Children.Clear();
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!MessageManager.isDatabaseLoaded())
            {
                MessageBox.Show("There is not a message database loaded currently");
                return;
            }
            clearMessages();

            var command = MessageManager.connection.CreateCommand();

            command.CommandText =
                $@"
                    SELECT ROWID,guid,text,handle_id,date
                    FROM message
                    WHERE text LIKE '%{containsTextBox.Text}%'
                    ORDER BY date DESC
                ";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (conversationPanel.Children.Count == 50)
                    {
                        TextBlock limitTextBlock = new TextBlock();
                        limitTextBlock.TextWrapping = TextWrapping.Wrap;
                        limitTextBlock.TextAlignment = TextAlignment.Center;
                        limitTextBlock.Foreground = Brushes.Red;
                        limitTextBlock.Inlines.Add(new Bold(new Run("This search was limited to the 50 most recent results, more results may be available")));
                        conversationPanel.Children.Add(limitTextBlock);
                        break;
                    }

                    // Get the name (have fun with this david :/)
                    string name;
                    string imgSource = "";

                    // chat_message_join command, get the chat id from message id
                    var cmjCommand = MessageManager.connection.CreateCommand();
                    cmjCommand.CommandText =
                        $@"
                            SELECT chat_id
                            FROM chat_message_join
                            WHERE message_id == '{reader.GetInt32(0)}'
                            LIMIT 1
                        ";

                    var cmjReader = cmjCommand.ExecuteReader();
                    cmjReader.Read();

                    // now that we have the chat id we can use that to get more information about the chat
                    int chat_id = cmjReader.GetInt32(0);

                    cmjReader.Close();
                    cmjCommand.Dispose();

                    // chat command, get style(for checking if its a group chat), chat_identifier, and display_name

                    var cCommand = MessageManager.connection.CreateCommand();
                    cCommand.CommandText =
                        $@"
                            SELECT style,chat_identifier,display_name
                            FROM chat
                            WHERE ROWID == '{chat_id}'
                            LIMIT 1
                        ";
                    var cReader = cCommand.ExecuteReader();
                    cReader.Read();

                    int style = cReader.GetInt32(0); // can be used to check if its a groupchat
                    string chat_identifier = cReader.GetString(1);
                    string display_name = cReader.GetString(2);

                    cReader.Close();
                    cCommand.Dispose();

                    if (style == 43)// means its a group chat
                    {
                        if (display_name != "")
                        {
                            name = display_name;
                        } else
                        {
                            name = "Unnamed Group Chat";
                        }
                    } else // non-group chat
                    {
                        Contact contact = ContactsManager.getContact(chat_identifier);
                        if (contact != null)
                        {
                            name = contact.fullName;
                            imgSource = contact.photo;
                        } else
                        {
                            name = chat_identifier;
                        }
                    }

                    if (!name.ToLower().Contains(nameTextBox.Text.ToLower()))
                    {
                        continue;
                    }

                    MessagePreview message = new MessagePreview(
                        reader.GetInt32(0),
                        name,
                        reader.GetString(2),
                        Util.cocoaToReadable(reader.GetInt64(4)),
                        imgSource,
                        this);
                    messagePreviews.Add(message);
                    conversationPanel.Children.Add(message);
                }
                reader.Close();
                command.Dispose();
            }
        }

        private double lastScroll = 0;
        private void messagesViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            bool scrollDown = messagesViewer.VerticalOffset > lastScroll;
            lastScroll = messagesViewer.VerticalOffset;

            int messagesToPurge = 0;

            // different for loop rules for if it is scrolling down or up but same logic regardless
            for (int i = 
                scrollDown ? 0  // if scrolling down, i initialized at 0
                : messagesStackPanel.Children.Count-1; // otherwise initialize i at one less than the number of messages loaded
                scrollDown ? i < messagesStackPanel.Children.Count // if scrolling down, stop when it hits the max value
                : i > 0; // otherwise stop when it hits zero
                i += scrollDown ? 1 // if scrolling down, increment by one while going through the loop
                : -1) // otherwise decrement by one
            {
                MessageViewer messageViewer = (MessageViewer)messagesStackPanel.Children[i];

                /*
                 okay logic check:

                i want to make sure that above and below the scroller are balanced in way of messages
                so
                what i want to do is ...??? 

                okay completely changing the logic here so i should commit what i have
                but
                what i want to do is go through all the messages, count the number above and below the visible box
                if there is a significant difference(more than 5% of the total number) then take balance it*/

               // if (messa)
            }
        }
    }
}
