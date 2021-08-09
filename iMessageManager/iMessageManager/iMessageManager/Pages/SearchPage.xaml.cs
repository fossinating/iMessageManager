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

        private MainWindow sourceWindow;
        public SearchPage(MainWindow window)
        {
            InitializeComponent();
            this.sourceWindow = window;
        }

        private void loadConversation(int messageID)
        {
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

                    int targetIndex;
                    messagesListBox.ItemsSource = MessageManager.GetMessagesWhere($"chat_id = '{chatID}'", messageID, out targetIndex);
                    messagesListBox.Items.Refresh();
                    messagesListBox.SelectedIndex = targetIndex;
                    messagesListBox.ScrollIntoView(messagesListBox.SelectedItem);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!MessageManager.IsDatabaseLoaded())
            {
                MessageBox.Show("There is not a message database loaded currently");
                return;
            }

            conversationsListBox.ItemsSource = MessageManager.GetMessagesWhere($"text LIKE '%{containsTextBox.Text}%'", false);
        }

        private void conversationsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (conversationsListBox.SelectedItem != null)
            {
                loadConversation(((MessageShell)conversationsListBox.SelectedItem).MessageID);
            }
        }
    }
}
