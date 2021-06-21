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
                ";
            var reader = command.ExecuteReader();

            reader.Read();

            int chatID = reader.GetInt32(0);
            reader.Close();

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

            List<Message> messages = new List<Message>();

            foreach (int _messageID in messageIDQueue)
            {
                command.CommandText =
                    $@"
                        SELECT ROWID, text, handle_id, date, is_from_me, guid
                        FROM message
                        WHERE ROWID == '{_messageID}'
                    ";

                reader = command.ExecuteReader();

                reader.Read();

                int handle_id = reader.GetInt32(2);

                // get the id from the handle

                var hCommand = MessageManager.connection.CreateCommand();

                hCommand.CommandText =
                    $@"
                        SELECT id
                        FROM handle
                        WHERE ROWID=='{handle_id}'
                    ";

                var hReader = hCommand.ExecuteReader();

                hReader.Read();

                Contact contact = ContactsManager.getContact(hReader.GetString(0));

                Message message = new Message(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    contact,
                    reader.GetInt64(3),
                    reader.GetInt32(4) == 1,
                    reader.GetGuid(5));
                reader.Close();

                messages.Add(message);
            }

            foreach (Message message in messages)
            {
                messagesStackPanel.Children.Add(message.GetMessageViewer());
            }
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
                
            }
        }
    }
}
