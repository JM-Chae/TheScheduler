
using System.Runtime.Intrinsics.X86;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TheScheduler.Models;
using TheScheduler.Repositories;
using TheScheduler.ViewModels;

namespace TheScheduler.Components
{
    public partial class EmployeeDialog : UserControl
    {
        public EmployeeDialog()
        {
            InitializeComponent();
            this.PreviewKeyDown += UserControl_KeyDown;
            this.IsVisibleChanged += EmployeeDialog_IsVisibleChanged;
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

        private void EmployeeDialog_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue) // Visible이 된 순간
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    NameF.Focus();
                    Keyboard.Focus(NameF);
                    NameF.SelectAll();
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }
    }
}
