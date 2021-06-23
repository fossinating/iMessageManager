using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace iMessageManager.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        private MainWindow sourceWindow;
        public SettingsPage(MainWindow sourceWindow)
        {
            InitializeComponent();
            this.sourceWindow = sourceWindow;
            messagesFilePathTextBlock.Text = Properties.Settings.Default.messagesPath;
            contactsPathTextBlock.Text = Properties.Settings.Default.contactsPath;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            sourceWindow.changePage(sourceWindow.startPage);
        }

        private void messagesFileSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All files (*.*)|*.*";
            bool? success = dialog.ShowDialog();
            if ((bool)success)
            {
                messagesFilePathTextBlock.Text = dialog.FileName;
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.messagesPath != messagesFilePathTextBlock.Text && MessageManager.loadMessageDatabase(messagesFilePathTextBlock.Text))
            {
                Properties.Settings.Default.messagesPath = messagesFilePathTextBlock.Text;
            }
            if (Properties.Settings.Default.contactsPath != contactsPathTextBlock.Text && ContactsManager.getContactsFromVCard(contactsPathTextBlock.Text))
            {
                Properties.Settings.Default.contactsPath = contactsPathTextBlock.Text;
            }
            Properties.Settings.Default.Save();
        }

        private void autoMessagesFileSelectButton_Click(object sender, RoutedEventArgs e)
        {
            string rootBackupPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Apple Computer\\MobileSync\\Backup\\");
            if (!Directory.Exists(rootBackupPath))
            {
                rootBackupPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Apple\\MobileSync\\Backup\\");
            }
            if (Directory.Exists(rootBackupPath)) {
                if (Directory.GetDirectories(rootBackupPath).Length == 1)
                {
                    string backupPath = System.IO.Path.Combine(rootBackupPath, Directory.GetDirectories(rootBackupPath).FirstOrDefault() + "\\");
                    string backupManifestPath = System.IO.Path.Combine(backupPath, "manifest.db");
                    SqliteConnection connection = new SqliteConnection($"Data Source={backupManifestPath}");
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                        @"
                            SELECT fileID
                            FROM Files
                            WHERE relativePath = 'Library/SMS/sms.db'
                        ";

                    var reader = command.ExecuteReader();
                    reader.Read();

                    var fileID = reader.GetString(0);

                    reader.Close();


                    string messageDirPath = System.IO.Path.Combine(backupPath, fileID.Substring(0, 2) + "\\");

                    if (Directory.Exists(messageDirPath))
                    {
                        string messagePath = System.IO.Path.Combine(messageDirPath, fileID);
                        if (File.Exists(messagePath))
                        {
                            messagesFilePathTextBlock.Text = messagePath;
                            MessageBox.Show("Successfully found the message database");
                        } else
                        {
                            MessageBox.Show("Message database somehow doesn't exist");
                        }
                    } else
                    {
                        MessageBox.Show("Something failed with finding the message directory");
                    }
                }
            } else
            {
                MessageBox.Show("Could not find a backup folder");
            }
        }

        private void contactFileSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "vCard Files (*.vcf)|*.vcf|All files (*.*)|*.*";
            bool? success = dialog.ShowDialog();
            if ((bool)success)
            {
                contactsPathTextBlock.Text = dialog.FileName;
            }
        }
    }
}
