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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class StartPage : UserControl
    {
        private MainWindow sourceWindow;
        public StartPage(MainWindow window)
        {
            InitializeComponent();
            this.sourceWindow = window;
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            sourceWindow.changePage(sourceWindow.settingsPage);
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            sourceWindow.changePage(sourceWindow.searchPage);
        }
    }
}
