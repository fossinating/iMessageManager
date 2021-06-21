using System;
using System.Collections.Generic;
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
            bool messagePathMissing = Properties.Settings.Default.messagesPath == "";
            bool contactsPathMissing = Properties.Settings.Default.contactsPath == "";
            if (messagePathMissing && contactsPathMissing)
            {
                contentControl.Content = settingsPage;
                MessageBox.Show("We did not find a messages path or contacts path currently selected, please define one in this page");
            } else if (messagePathMissing)
            {
                ContactsManager.getContactsFromVCard(Properties.Settings.Default.contactsPath);
                contentControl.Content = settingsPage;
                MessageBox.Show("We did not find a messages path currently selected, please define one in this page");
            } else if (contactsPathMissing)
            {
                MessageManager.loadMessageDatabase(Properties.Settings.Default.messagesPath);
                contentControl.Content = settingsPage;
                MessageBox.Show("We did not find a contacts path currently selected, please define one in this page");
            } else
            {
                contentControl.Content = startPage;
                ContactsManager.getContactsFromVCard(Properties.Settings.Default.contactsPath);
                MessageManager.loadMessageDatabase(Properties.Settings.Default.messagesPath);
            }
        }

        public void changePage(UserControl page)
        {
            contentControl.Content = page;
        }
    }
}
