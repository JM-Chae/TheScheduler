using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using TheScheduler.Services;
using TheScheduler.Views;

using CommunityToolkit.Mvvm.Messaging;
using TheScheduler.Utils;
using TheScheduler.Components;

namespace TheScheduler.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isShiftDialogOpen;

        [ObservableProperty]
        private UserControl _mainContent = new Home();

        public Action RefreshHomeView { get; set; }

        public List<CultureInfo> AvailableCultures => SettingsService.Instance.AvailableCultures;

        public CultureInfo SelectedCulture
        {
            get => SettingsService.Instance.CurrentCulture;
            set
            {
                if (SettingsService.Instance.CurrentCulture != value)
                {
                    SettingsService.Instance.CurrentCulture = value;
                    var msgBox = new CustomMessageBox(LocalizationService.Instance.GetString("RestartConfirmation"));
                    msgBox.Owner = Application.Current.MainWindow;
                    msgBox.ShowDialog();

                    if (msgBox.Result)
                    {
                        Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                        Application.Current.Shutdown();
                    }
                }
            }
        }

        [RelayCommand]
        private void Print()
        {
            WeakReferenceMessenger.Default.Send(new PrintRequestMessage());
        }

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
            ShiftVM = new ShiftViewModel(() => IsShiftDialogOpen = false, () => RefreshHomeView?.Invoke());
        }

    }
}
