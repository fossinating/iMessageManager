using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
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
            backupPathSelectTextBlock.Text = Properties.Settings.Default.backupPath;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            sourceWindow.changePage(sourceWindow.startPage);
        }

        private void backupPathSelectButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.InitialDirectory = "C:";
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    backupPathSelectTextBlock.Text = dialog.FileName;
                }
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.backupPath != backupPathSelectTextBlock.Text && MessageManager.LoadBackup(backupPathSelectTextBlock.Text))
            {
                Properties.Settings.Default.backupPath = backupPathSelectTextBlock.Text;
            }
            Properties.Settings.Default.Save();
        }

        private void autoBackupPathSelectButton_Click(object sender, RoutedEventArgs e)
        {
            string rootBackupPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Apple Computer\\MobileSync\\Backup\\");
            if (!Directory.Exists(rootBackupPath))
            {
                rootBackupPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Apple\\MobileSync\\Backup\\");
            }
            if (Directory.Exists(rootBackupPath)) {
                foreach (var dir in Directory.GetDirectories(rootBackupPath))
                {
                    string backupPath = System.IO.Path.Combine(rootBackupPath, Directory.GetDirectories(rootBackupPath).FirstOrDefault() + "\\");
                    if (MessageManager.LoadBackup(backupPath))
                    {
                        backupPathSelectTextBlock.Text = backupPath;
                        MessageBox.Show("Successfully found the backup");
                        return;
                    }
                }

                MessageBox.Show("Could not find a valid backup in the backups folder");
            } else
            {
                MessageBox.Show("Could not find a backup folder");
            }
        }
    }
}
