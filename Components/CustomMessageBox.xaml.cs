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

namespace TheScheduler.Components
{
    /// <summary>
    /// CustomMessageBox.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public bool Result { get; private set; } = false;

        public CustomMessageBox(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            this.DialogResult = true;
            this.Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            this.DialogResult = false;
            this.Close();
        }
    }

}
