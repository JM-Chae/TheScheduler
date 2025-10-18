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
    public partial class EmployeeViewModel : ObservableObject
    {
        private readonly EmployeeRepo _repo = new();
        private readonly PositionRepo _positionRepo = new();
        public Array SexValues { get; } = Enum.GetValues(typeof(Sex));

        [ObservableProperty]
        private ObservableCollection<Position> _availablePositions;

        [ObservableProperty]
        private bool _isDialogOpen;

        [ObservableProperty]
        private string errorMessageText = "";

        [ObservableProperty]
        private bool visibleError = false;

        [ObservableProperty]
        private ObservableCollection<Employee> employees = new();

        [ObservableProperty]
        private Employee? selectedEmployee;

        [ObservableProperty]
        private Employee? editingEmployee;

        [ObservableProperty]
        private string _newPosition;

        [ObservableProperty]
        private bool _isPositionDialogOpen;

        [RelayCommand]
        private void LoadEmployees()
        {
            Employees = new ObservableCollection<Employee>(_repo.GetAll());
        }

        [RelayCommand]
        private void LoadPositions()
        {
            AvailablePositions = new ObservableCollection<Position>(_positionRepo.GetAll());
        }

        public EmployeeViewModel()
        {
            LoadEmployees();
            LoadPositions();
            SelectedEmployee = new Employee
            {
                Id = 0,
                Name = "",
                Sex = Sex.女性,
                Position = AvailablePositions.FirstOrDefault()?.Name ?? "",
                Category = 0
            };
        }

        [RelayCommand]
        private void DeleteEmployee(Employee emp)
        {
            if (emp.Id == 0) return;

            var msgBox = new CustomMessageBox(LocalizationService.Instance.GetString("DeleteEmployeeConfirmation"))
            {
                Owner = Application.Current.MainWindow // 모달처럼 띄우기
            };

            msgBox.ShowDialog();

            bool result = msgBox.Result;

            if (result)
            {
                _repo.Delete(emp.Id);
                LoadEmployees();
            }
        }

        [RelayCommand]
        private void Open_EditEmployee(int id)
        {
            if (id == 0)
            {
                var msgBox = new CustomMessageBox(LocalizationService.Instance.GetString("SelectEmployeeMessage"))
                {
                    Owner = Application.Current.MainWindow // 모달처럼 띄우기
                };
            }
            if (SelectedEmployee != null)
            {
                EditingEmployee = DeepCopyHandler.Clone<Employee>(SelectedEmployee);
                IsDialogOpen = true;
            }
        }

        [RelayCommand]
        private void Open_Position()
        {
            NewPosition = "";
            IsPositionDialogOpen = true;
        }

        [RelayCommand]
        private void Close_Position()
        {
            IsPositionDialogOpen = false;
        }

        [RelayCommand]
        private void SaveNewPosition()
        {
            if (!string.IsNullOrWhiteSpace(NewPosition)) 
            {
                if (AvailablePositions.Any(p => p.Name == NewPosition))
                {
                    MessageBox.Show(LocalizationService.Instance.GetString("PositionExistsMessage"));
                    return;
                }

                var newPosition = new Position { PositionId = _positionRepo.GetNewId(), Name = NewPosition };
                _positionRepo.Add(newPosition);
                LoadPositions();
                IsPositionDialogOpen = false;
            }
        }

        [RelayCommand]
        private void EditEmployeeDialog_Cancelled()
        {
            IsDialogOpen = false;
            EditingEmployee = null;
        }

        [RelayCommand]
        private void Open_AddEmployee()
        {
            EditingEmployee = new Employee
            {
                Id = 0,
                Name = "",
                Sex = Sex.女性,
                Position = AvailablePositions.FirstOrDefault()?.Name ?? "",
                Category = 0
            };
            IsDialogOpen = true;
        }

        [RelayCommand]
        private void Dialog_EmployeeUpsert(Employee e)
        {
            if (EditingEmployee is null) return;

            switch (EditingEmployee)
            {
                case { Name: null or "" }:
                    ShowError(LocalizationService.Instance.GetString("EnterNameMessage"));
                    return;

            }

            if (EditingEmployee.Id == 0)
            {
                EditingEmployee.Id = _repo.GetNewId();
                _repo.Add(EditingEmployee);
            } else
            {
                _repo.Update(EditingEmployee);
            }

            var selectedId = EditingEmployee.Id;
            LoadEmployees();
            SelectedEmployee = Employees.FirstOrDefault(emp => emp.Id == selectedId);
            IsDialogOpen = false;
            EditingEmployee = null;
        }

        private void ShowError(string message)
        {
            ErrorMessageText = message;
            VisibleError = true;

        }
     }
}
