﻿using System;
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
        private List<Message> messages = new List<Message>();
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


            messagesListBox.ItemsSource = messageIDQueue;
            return;

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
            MessageViewer target = null;
            int loadCount = 101;
            int startIndex = Math.Max(0, targetIndex - (loadCount / 2));
            currentStartIndex = startIndex;
            int endIndex = Math.Min(messages.Count()-1, startIndex + loadCount);
            foreach (Message message in messages.GetRange(startIndex, endIndex-startIndex+1))
            {
                MessageViewer messageViewer = message.GetMessageViewer();
                messagesStackPanel.Children.Add(messageViewer);
                if (message.messageID == messageID)
                {
                    target = messageViewer;
                }
            }
            ignoreScrollUpdate = true;
            Point relativePoint = target.TransformToAncestor(messagesStackPanel)
                              .Transform(new Point(0, 0));
            messagesViewer.ScrollToVerticalOffset(relativePoint.Y);
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
            if (ignoreScrollUpdate)
            {
                ignoreScrollUpdate = false;
                return;
            }

            bool scrollDown = messagesViewer.VerticalOffset > lastScroll;
            lastScroll = messagesViewer.VerticalOffset;

            int upperMessages = 0;
            int lowerMessages = 0;

            foreach (MessageViewer messageViewer in messagesStackPanel.Children)
            {
                Point relativePoint = messageViewer.TransformToAncestor(messagesViewer)
                              .Transform(new Point(0, 0));
                if (relativePoint.Y < -messageViewer.ActualHeight)
                {
                    upperMessages++;
                } else if (relativePoint.Y > messagesViewer.ActualHeight)
                {
                    lowerMessages++;
                }
            }

            int messageShift = Math.Abs((upperMessages - lowerMessages) / 2);
            bool updatingMessages = false;
            double verticalChange = 0;
            if (scrollDown)
            {
                if (upperMessages - lowerMessages > messagesStackPanel.Children.Count * 0.05)
                {
                    updatingMessages = true;
                    messageShift = Math.Min(messages.Count - (currentStartIndex + messagesStackPanel.Children.Count + 1), messageShift);
                    verticalChange = -getVerticalOffset(messagesStackPanel.Children[messageShift]);
                    messagesStackPanel.Children.RemoveRange(0, messageShift);
                    foreach (Message message in messages.GetRange(currentStartIndex + messagesStackPanel.Children.Count + 1, messageShift))
                    {
                        messagesStackPanel.Children.Add(message.GetMessageViewer());
                    }
                    currentStartIndex += messageShift;
                    ignoreScrollUpdate = true;
                }
            } else
            {
                if (lowerMessages - upperMessages > messagesStackPanel.Children.Count * 0.05)
                {
                    updatingMessages = true;
                    messageShift = Math.Min(currentStartIndex, messageShift);
                    verticalChange = -getVerticalOffset(messagesStackPanel.Children[messageShift]);
                    Console.WriteLine(getVerticalOffset(messagesStackPanel.Children[messageShift]));
                    messagesStackPanel.Children.RemoveRange(messagesStackPanel.Children.Count - 1 - messageShift, messageShift);
                    List<Message> range = messages.GetRange(currentStartIndex - messageShift, messageShift);
                    bool first = true;
                    foreach (Message message in range)
                    {
                        MessageViewer viewer = message.GetMessageViewer();
                        if (first)
                        {
                            viewer.nominateForNormalization(getVerticalOffset(messagesStackPanel.Children[messagesStackPanel.Children.Count / 2]));
                            first = false;
                        }
                        messagesStackPanel.Children.Insert(range.IndexOf(message), viewer);
                        //MessageBox.Show(viewer.ActualHeight.ToString());
                    }
                    Console.WriteLine(getVerticalOffset(messagesStackPanel.Children[messageShift]));
                    verticalChange += getVerticalOffset(messagesStackPanel.Children[messageShift]);
                    currentStartIndex -= messageShift;
                    ignoreScrollUpdate = true;
                }
            }
            messagesViewer.ScrollToVerticalOffset(messagesViewer.VerticalOffset + verticalChange);
            nameSearchLabel.Content = currentStartIndex + " : " + (currentStartIndex + messagesStackPanel.Children.Count) + "\n" + verticalChange;
            contentSearchLabel.Content = lowerMessages + " : " + upperMessages + "(" + updatingMessages + ")";
        }

        private double getVerticalOffset(UIElement element)
        {
            Point relativePoint = element.TransformToAncestor(messagesStackPanel)
                              .Transform(new Point(0, 0));
            return relativePoint.Y;
        }
    }
}
