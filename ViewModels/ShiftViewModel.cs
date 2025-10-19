using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using TheScheduler.Components;
using TheScheduler.Models;
using TheScheduler.Repositories;
using TheScheduler.Services;
using TheScheduler.Utils;

namespace TheScheduler.ViewModels
{
    public partial class ShiftViewModel : ObservableObject
    {
        private readonly ShiftRepo _repo = new ShiftRepo();
        private readonly PositionRepo _positionRepo = new();
        private readonly Action _onShiftUpdated;

        public RelayCommand CloseCommand { get; }

        public IEnumerable<ShiftColor> AvailableShiftColors { get; }
        [ObservableProperty]
        public ObservableCollection<Position> _availablePositions;

        public ShiftViewModel(Action closeAction, Action onShiftUpdated)
        { 
            CloseCommand = new RelayCommand(() =>
            {
                IsDialogOpen = false;
                closeAction();
            });

            AvailableShiftColors = Enum.GetValues<ShiftColor>()
            .Where(sc => sc != ShiftColor.Y)
            .ToList();

            LoadPositions();

            _onShiftUpdated = onShiftUpdated;
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

        [ObservableProperty]
        private ShiftColor _selectedShiftColor;

        [ObservableProperty]
        private string _selectedPosition;

        [RelayCommand]
        private void LoadShifts()
        {
            Shifts = new ObservableCollection<Shift>(_repo.GetAll());
        }

        [RelayCommand]
        private void LoadPositions()
        {
            AvailablePositions = new ObservableCollection<Position>(_positionRepo.GetAll());
        }

        [RelayCommand]
        private void Delete(Shift shift)
        {
            if (shift == null) return;

            var msgBox = new CustomMessageBox(LocalizationService.Instance.GetString("Shift_ConfirmDelete"))
            {
                Owner = Application.Current.MainWindow
            };

            msgBox.ShowDialog();

            bool result = msgBox.Result;

            if (result)
            {
                _repo.Delete(shift.Id);
                LoadShifts();
                _onShiftUpdated?.Invoke();
            }
        }

        [RelayCommand]
        private void AddCondition()
        {
            if(AvailablePositions == null || AvailablePositions.Count == 0)
                {
                MessageBox.Show(LocalizationService.Instance.GetString("NoPositionsDefinedMessage"));
                return;
            }
            ShiftCondition shiftCondition = new ShiftCondition
            {
                Id = Guid.NewGuid(),
                Position = AvailablePositions.FirstOrDefault()?.Name ?? "",
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
                var msgBox = new CustomMessageBox(LocalizationService.Instance.GetString("Shift_SelectShiftMessage"))
                {
                    Owner = Application.Current.MainWindow
                };
            }

            else
            {
                Conditions.Clear();
                if (SelectedShift?.Conditions != null)
                {
                    foreach (var c in SelectedShift.Conditions)
                    {
                        Conditions.Add(new ShiftCondition
                        {
                            Id = c.Id,
                            Position = c.Position,
                            Value = c.Value
                        });
                    }
                }
                ; 
                IsDialogOpen = true;
            }
        }

        [RelayCommand]
        private void Open_Add()
        {
            LoadPositions();
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
            s.ShiftColor = SelectedShiftColor;
            switch (s)
            {
                case { Name: null or "" }:
                    ShowError(LocalizationService.Instance.GetString("Shift_EnterNameMessage"));
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
            _onShiftUpdated?.Invoke();
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
                RestInMinutes = 60,
                ShiftColor = ShiftColor.A

            };
        }
    }
}
