using Syncfusion.SfSkinManager;
using Syncfusion.Themes.MaterialDark.WPF;
using Syncfusion.Windows.Shared;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using TheScheduler.Models;
using TheScheduler.ViewModels;

namespace TheScheduler.Components
{
    /// <summary>
    /// ShiftManagemantDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ShiftManagementDialog : UserControl
    {
        public ShiftManagementDialog()
        {
            InitializeComponent();
            this.PreviewKeyDown += UserControl_KeyDown;
            this.IsVisibleChanged += ShiftManagementDialogAdded_IsVisibleChanged;
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (DataContext is ShiftViewModel vm)
                {
                    vm.IsDialogOpen = false;
                }
            }
        }

        private void ShiftManagementDialogAdded_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DependencyPropertyDescriptor
                .FromProperty(UIElement.VisibilityProperty, typeof(Grid))
                .AddValueChanged(DialogOverlay, (s, e) =>
                {
                    if (DialogOverlay.Visibility == Visibility.Visible)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            NameF.Focus();
                            Keyboard.Focus(NameF);
                            NameF.SelectAll();
                        }), System.Windows.Threading.DispatcherPriority.Input);
                    }
                });
        }

        public void PrintShiftsList(object sender, RoutedEventArgs e)
        {
            PrintManager.PrintShiftDataGrid(ShiftsGrid);
        }
    }
}
