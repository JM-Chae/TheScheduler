using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using TheScheduler.Components;
using TheScheduler.Models;
using TheScheduler.Repositories;
using TheScheduler.Views;

namespace TheScheduler.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject
    {
        private readonly EmployeeRepo _repo = new();
        public Array SexValues { get; } = Enum.GetValues(typeof(Sex));
        public Array PositionValues { get; } = Enum.GetValues(typeof(Position));

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
        public Employee newEmployee;

        public EmployeeViewModel()
        {
            LoadEmployees();
            newEmployee = new Employee
            {
                Id = _repo.GetNewId(),
                Name = "",
                Sex = Sex.女性,
                Position = Position.看護師
            };
        }

        [RelayCommand]
        private void LoadEmployees()
        {
            Employees = new ObservableCollection<Employee>(_repo.GetAll());
        }

        [RelayCommand]
        private void DeleteEmployee(Employee emp)
        {
            if (emp == null) return;

            var msgBox = new CustomMessageBox("このメンバーのデーターを削除しますか？")
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
        private void Open_AddEmployee()
        {
            IsDialogOpen = true;
        }

        [RelayCommand]
        private void AddEmployeeDialog_Cancelled()
        {
            IsDialogOpen = false;
        }

        [RelayCommand]
        private void AddEmployeeDialog_EmployeeAdded(Employee e)
        {
            switch (e)
            {
                case { Name: null or "" }:
                    ShowError("名前を入力してください。");
                    return;

            }
            _repo.Add(e);

            NewEmployee = new Employee
            {
                Id = _repo.GetNewId(),
                Name = "",
                Sex = Sex.女性,
                Position = Position.看護師
            };

            LoadEmployees();
            IsDialogOpen = false;
        }

        private void ShowError(string message)
        {
            ErrorMessageText = message;
            VisibleError = true;

        }
    }
}
