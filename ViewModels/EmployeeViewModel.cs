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

        [RelayCommand]
        private void LoadEmployees()
        {
            Employees = new ObservableCollection<Employee>(_repo.GetAll());
        }

        public EmployeeViewModel()
        {
            LoadEmployees();
            SelectedEmployee = new Employee
            {
                Id = 0,
                Name = null,
                Sex = Sex.女性,
                Position = Position.看護師
            };
        }

        [RelayCommand]
        private void DeleteEmployee(Employee emp)
        {
            if (emp.Id == 0) return;

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
        private void Open_EditEmployee(int id)
        {
            if (id == 0)
            {
                var msgBox = new CustomMessageBox("メンバーを選択してください。")
                {
                    Owner = Application.Current.MainWindow // 모달처럼 띄우기
                };
            }

            else IsDialogOpen = true;
        }

        [RelayCommand]
        private void EditEmployeeDialog_Cancelled()
        {
            IsDialogOpen = false;
        }

        [RelayCommand]
        private void Open_AddEmployee()
        {
            SelectedEmployee = new Employee
            {
                Id = 0,
                Name = null,
                Sex = Sex.女性,
                Position = Position.看護師
            };
            IsDialogOpen = true;
        }

        [RelayCommand]
        private void Dialog_CancelledAdded()
        {
            IsDialogOpen = false;
        }

        [RelayCommand]
        private void Dialog_EmployeeUpsert(Employee e)
        {
            switch (e)
            {
                case { Name: null or "" }:
                    ShowError("名前を入力してください。");
                    return;

            }

            if (e.Id == 0)
            {
                e.Id = _repo.GetNewId();
                _repo.Add(e); 
            } else
            {
                _repo.Update(e);
            }

            SelectedEmployee = new Employee
            {
                Id = 0,
                Name = "",
                Sex = Sex.女性,
                Position = Position.看護師
            };

            LoadEmployees();
            SelectedEmployee = e;
            IsDialogOpen = false;
        }

        private void ShowError(string message)
        {
            ErrorMessageText = message;
            VisibleError = true;

        }
    }
}
