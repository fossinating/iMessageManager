using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Diagnostics;

namespace iMessageManager.Pages
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class SearchPage : UserControl
    {

        private List<MessagePreview> messagePreviews = new List<MessagePreview>();
        private ObservableCollection<Message> messages = new ObservableCollection<Message>();
        private int currentStartIndex = -1;
        private bool ignoreScrollUpdate = false;

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
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int targetIndex = -1;
            using (var chatIDCommand = MessageManager.connection.CreateCommand())
            {
                chatIDCommand.CommandText =
                    $@"
                    SELECT chat_id
                    FROM messages_master
                    WHERE message_id == '{messageID}'
                    LIMIT 1
                ";
                using (var chatIDReader = chatIDCommand.ExecuteReader())
                {

                    chatIDReader.Read();

                    int chatID = chatIDReader.GetInt32(0);

                    using (var msgCommand = MessageManager.connection.CreateCommand())
                    {
                        msgCommand.CommandText =
                        $@"
                            SELECT *
                            FROM messages_master
                            WHERE chat_id == '{chatID}'
                            ORDER BY date ASC
                        ";
                        using (var msgReader = msgCommand.ExecuteReader())
                        {
                            while (msgReader.Read())
                            {

                                Contact contact;
                                // if is from me, then no contact needed
                                if (msgReader.GetInt32(msgReader.GetOrdinal("is_from_me")) == 1)
                                {
                                    contact = null;
                                }
                                // otherwise, get id from handle
                                else
                                {

                                    int handle_id = msgReader.GetInt32(msgReader.GetOrdinal("handle_id"));

                                    if (handle_id == 0)
                                    {
                                        if (!msgReader.IsDBNull(msgReader.GetOrdinal("text")))
                                        {
                                            MessageBox.Show($"Found a message (id {msgReader.GetInt32(msgReader.GetOrdinal("message_id"))}) with handle 0 not listed as from me but it has message body \"{msgReader.GetString(1)}\"");
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }

                                    contact = ContactsManager.FromHandle(handle_id);

                                }

                                bool valid = true;
                                for (int i = 0; i <= 5; i++)
                                {
                                    if (msgReader.IsDBNull(i))
                                    {
                                        //MessageBox.Show($"Found null value at index {i} of messageID {_messageID}");
                                        valid = false;
                                        break;
                                    }
                                }
                                if (valid)
                                {
                                    Message message = new Message(
                                        msgReader.GetInt32(msgReader.GetOrdinal("handle_id")),
                                        msgReader.GetString(msgReader.GetOrdinal("text")),
                                        contact,
                                        msgReader.GetInt64(msgReader.GetOrdinal("date")),
                                        msgReader.GetInt32(msgReader.GetOrdinal("is_from_me")) == 1,
                                        msgReader.GetGuid(msgReader.GetOrdinal("message_guid")));

                                    if (messageID == msgReader.GetInt32(0))
                                    {
                                        targetIndex = messages.Count;
                                    }

                                    messages.Add(message);
                                }
                            }
                        }
                    }

                    messagesListBox.ItemsSource = messages;
                    messagesListBox.Items.Refresh();
                    messagesListBox.SelectedIndex = targetIndex;
                    messagesListBox.ScrollIntoView(messagesListBox.SelectedItem);
                    sw.Stop();
                    MessageBox.Show($"Loaded {messages.Count} messages in {sw.Elapsed}, an average of {sw.ElapsedMilliseconds/messages.Count}ms per message");
                }
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
            if (!MessageManager.IsDatabaseLoaded())
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
                    byte[] imgSource = null;

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
                        Contact contact = ContactsManager.GetContact(chat_identifier);
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
    }
}
