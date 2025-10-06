
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TheScheduler.Models;
using TheScheduler.Repositories;
using TheScheduler.ViewModels;

namespace TheScheduler.Components
{
    public partial class AddEmployeeDialog : UserControl
    {
        public AddEmployeeDialog()
        {
            InitializeComponent();
            this.PreviewKeyDown += UserControl_KeyDown;
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
