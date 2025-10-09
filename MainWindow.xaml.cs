using CommunityToolkit.Mvvm.ComponentModel;
using Syncfusion.SfSkinManager;
using System.Windows;
using System.Windows.Input;
using TheScheduler.Components;
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
            SfSkinManager.ApplyThemeAsDefaultStyle = true;
            SfSkinManager.ApplicationTheme = new Theme("MaterialDark");
            InitializeComponent();
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