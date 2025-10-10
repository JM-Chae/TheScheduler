using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Syncfusion.Windows.Shared;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using TheScheduler.Components;
using TheScheduler.Models;
using TheScheduler.Repositories;

namespace TheScheduler.ViewModels
{
    public partial class ShiftViewModel : ObservableObject
    {
        private readonly ShiftRepo _repo = new ShiftRepo();
        public RelayCommand CloseCommand { get; }

        public ShiftViewModel(Action closeAction)
        {
            CloseCommand = new RelayCommand(() =>
            {
                IsDialogOpen = false;
                closeAction();
            });
            LoadShifts();
            resetSelected();
        }

        [ObservableProperty]
        private bool _isDialogOpen;

        [ObservableProperty]
        private ObservableCollection<Shift> _shifts = new();

        [ObservableProperty]
        private ObservableCollection<ShiftCondition> _conditions = new();

        [ObservableProperty]
        private Shift? _selectedShift;

        [ObservableProperty]
        private string _errorMessageText = "";

        [ObservableProperty]
        private bool _visibleError = false;

        [RelayCommand]
        private void LoadShifts()
        {
            Shifts = new ObservableCollection<Shift>(_repo.GetAll());
        }

        [RelayCommand]
        private void Delete(Shift shift)
        {
            if (shift == null) return;

            var msgBox = new CustomMessageBox("このシフトのデーターを削除しますか？")
            {
                Owner = Application.Current.MainWindow
            };

            msgBox.ShowDialog();

            bool result = msgBox.Result;

            if (result)
            {
                _repo.Delete(shift.Id);
                LoadShifts();
            }
        }

        [RelayCommand]
        private void AddCondition()
        {
            ShiftCondition shiftCondition = new ShiftCondition
            {
                Id = Guid.NewGuid(),
                Position = Position.看護師,
                Value = 1
            };
            Conditions?.Add(shiftCondition);
        }

        [RelayCommand]
        private void DeleteCondition(Guid id)
        {
            var target = Conditions.FirstOrDefault(c => c.Id == id);
            if (target != null) Conditions?.Remove(target);
        }

        [RelayCommand]
        private void Open_Edit(int id)
        {
            if (id == 0)
            {
                var msgBox = new CustomMessageBox("メンバーを選択してください。")
                {
                    Owner = Application.Current.MainWindow
                };
            }

            else
            {
                Conditions.Clear();
                Conditions.AddRange(SelectedShift?.Conditions.Select(c => new ShiftCondition
                {
                    Id = c.Id,
                    Position = c.Position,
                    Value = c.Value
                })); 
                IsDialogOpen = true;
            }
        }

        [RelayCommand]
        private void Open_Add()
        {
            resetSelected();
            IsDialogOpen = true;
        }

        [RelayCommand]
        private void Dialog_CancelledAdded()
        {
            resetSelected();
            IsDialogOpen = false;
        }

        [RelayCommand]
        private void Dialog_Upsert(Shift s)
        {
            switch (s)
            {
                case { Name: null or "" }:
                    ShowError("名前を入力してください。");
                    return;

            }

            SelectedShift?.Conditions.Clear();
            SelectedShift?.Conditions.AddRange(Conditions);

            if (s.Id == 0)
            {
                s.Id = _repo.GetNewId();
                _repo.Add(s);
            }
            else
            {
                _repo.Update(s);
            }

            resetSelected();

            LoadShifts();
            SelectedShift = s;
            IsDialogOpen = false;
        }

        private void ShowError(string message)
        {
            ErrorMessageText = message;
            VisibleError = true;
        }

        private Shift resetSelected()
        {
            Conditions.Clear();
            VisibleError = false;
            return SelectedShift = new Shift
            {
                Id = 0,
                Name = "",
                Conditions = new List<ShiftCondition>(),
                Start = new TimeOnly(12, 0, 0),
                End = new TimeOnly(12, 0, 0),
                Rest = new TimeOnly(12, 0, 0),
                ShiftColor = ShiftColor.A

            };
        }
    }
}
