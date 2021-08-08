using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using iMessageManager.Pages;

namespace iMessageManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public StartPage startPage { get; private set; }
        public SearchPage searchPage { get; private set; }
        public SettingsPage settingsPage { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            startPage = new StartPage(this);
            searchPage = new SearchPage(this);
            settingsPage = new SettingsPage(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool messagePathMissing = Properties.Settings.Default.backupPath == "";
            bool contactsPathMissing = Properties.Settings.Default.contactsPath == "";
            if (messagePathMissing)
            {
                contentControl.Content = settingsPage;
                MessageBox.Show("We did not find a backup path currently selected, please define one in this page");
            } else
            {
                contentControl.Content = startPage;

                if (!File.Exists(MessageManager.preloadPath))
                {
                    MessageManager.LoadBackup(Properties.Settings.Default.backupPath);
                } else 
                {
                    MessageManager.LoadPreload();
                }
            }
        }

        public void changePage(UserControl page)
        {
            contentControl.Content = page;
        }
    }
}
