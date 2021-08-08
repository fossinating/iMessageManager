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
using System.Windows.Shapes;

namespace iMessageManager
{
    /// <summary>
    /// Interaction logic for InfoBox.xaml
    /// </summary>
    public partial class InfoBox : Window
    {
        public InfoBox()
        {
            InitializeComponent();
        }

        public static InfoBox Show(string message)
        {
            InfoBox infoBox = new InfoBox();
            infoBox.text.Text = message;
            infoBox.Show();
            return infoBox;
        }
    }
}
