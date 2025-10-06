using System.Windows;
using System.Windows.Input;
using TheScheduler.Views;


namespace TheScheduler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainContent.Content = new Home();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Home();
        }

        private void Member_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new EmployeeManagement();
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


    }
}