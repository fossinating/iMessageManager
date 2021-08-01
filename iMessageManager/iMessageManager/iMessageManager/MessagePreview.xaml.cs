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
        private Pages.SearchPage searchPage;
        public int messageID
        {
            get; private set;
        }

        public MessagePreview(int messageID, string name, string content, string time, byte[] imageData, Pages.SearchPage searchPage)
        {
            InitializeComponent();
            this.searchPage = searchPage;
            this.messageID = messageID;

            conversationNameLabel.Content = name;
            messageLabel.Content = content;
            timeLabel.Content = time;

            if (imageData != null && imageData.Length > 0)
            {
                contactImage.Source = Util.ToImage(imageData);
            }
        }

        private void selectButton_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)selectButton.IsChecked)
            {
                searchPage.select(this);
            }
        }

        public void setSelected(bool selected)
        {
            selectButton.IsChecked = selected;
        }
    }
}
