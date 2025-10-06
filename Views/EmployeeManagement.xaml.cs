
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using System.Windows.Input;
using TheScheduler.Components;
using TheScheduler.Models;
using TheScheduler.ViewModels;


namespace TheScheduler.Views
{
    public partial class EmployeeManagement : UserControl
    {
        public EmployeeManagement()
        {
            InitializeComponent();
        }
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (DataContext is EmployeeViewModel vm)
                {
                    vm.IsDialogOpen = false;
                }
            }
        }
    }
}
