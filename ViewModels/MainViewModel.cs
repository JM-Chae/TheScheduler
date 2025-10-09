using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;
using TheScheduler.Views;

namespace TheScheduler.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isShiftDialogOpen;

        [ObservableProperty]
        private UserControl _mainContent = new Home();

        [RelayCommand]
        private void Home_Click()
        {
            MainContent = new Home();
        }

        [RelayCommand]
        private void Member_Click()
        {
            MainContent = new EmployeeManagement();
        }

        [RelayCommand]
        private void Shift_Click()
        {
            IsShiftDialogOpen = true;
        }

        public ShiftViewModel ShiftVM { get; }

        public MainViewModel()
        {
            ShiftVM = new ShiftViewModel(() => IsShiftDialogOpen = false);
        }

    }
}
