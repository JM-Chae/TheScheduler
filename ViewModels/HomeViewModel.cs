using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using TheScheduler.Components;
using TheScheduler.Models;
using TheScheduler.Repositories;
using TheScheduler.Utils;

namespace TheScheduler.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly EmployeeRepo _empRepo = new();
        private readonly ScheduleRepo _scheduleRepo = new();
        private readonly ShiftRepo _shiftRepo = new();
        private readonly LeaveRepo _leaveRepo = new();
        private readonly CorrectionRepo _correctionRepo = new();

        [ObservableProperty]
        private bool _isVisibleScheduleEditDitalog = false;

        [ObservableProperty]
        private DateTime _currentDate;
        [ObservableProperty]
        private int _selectedEmployeeId;
        [ObservableProperty]
        private Employee _selectedEmployee;

        [ObservableProperty]
        private ObservableCollection<Employee> _Employees;
        [ObservableProperty]
        private ObservableCollection<Shift> _shifts;

        [ObservableProperty]
        private ScheduleEditType? type;

        [ObservableProperty]
        private Schedule? _schedule;
        [ObservableProperty]
        private Schedule? _isSchedule;

        [ObservableProperty]
        private Leave? _leave;
        [ObservableProperty]
        private Leave? _isLeave;

        [ObservableProperty]
        private Correction? _correction;
        [ObservableProperty]
        private Correction? _isCorrection;

        public HomeViewModel()
        {
            type = ScheduleEditType.Shift;
        }

        public void ScheduleEditDialogOpen(int empId, DateTime date)
        {
            SelectedEmployee = Employees.FirstOrDefault(e => e.Id.Equals(empId));
            Shifts = new ObservableCollection<Shift>(_shiftRepo.GetAll());

            SelectedEmployeeId = empId;
            CurrentDate = date;

            if (Shifts.Count == 0)
            {
                MessageBox.Show("まずはシフトを登録してください。");
                return;
            }

            IsSchedule = _scheduleRepo.GetByEmployeeIdAndDate(SelectedEmployeeId, CurrentDate.Date);
            IsLeave = _leaveRepo.GetByEmployeeIdAndDate(SelectedEmployeeId, CurrentDate.Date);
            IsCorrection = _correctionRepo.GetByEmployeeIdAndDate(SelectedEmployeeId, CurrentDate.Date);

            if (IsSchedule != null) Schedule = DeepCopyHandler.Clone(IsSchedule);
            else Schedule = new Schedule()
            { Id = 0, ShiftId = Shifts.First().Id, WorkDate = CurrentDate, EmployeeId = new List<int>() { SelectedEmployeeId } };

            if (IsLeave != null) Leave = DeepCopyHandler.Clone(IsLeave);
            else Leave = new Leave()
            { Id = 0, LeaveAt = CurrentDate, EmployeeId = SelectedEmployeeId, IsPaid = true, Why = "" };

            if (IsCorrection != null) Correction = DeepCopyHandler.Clone(IsCorrection);
            else Correction = new Correction()
            { Id = 0, When = CurrentDate, EmployeeId = SelectedEmployeeId, IsTimeIncreasing = false, Note = "", CorrectTime = new TimeOnly(1, 0), Type = CorrectionType.遅刻 };

            IsVisibleScheduleEditDitalog = true;
        }

        [RelayCommand]
        public void ScheduleEditDialogClose()
        {
            IsVisibleScheduleEditDitalog = false;
        }


        [RelayCommand]
        public void SwitchScheduleEditType(string newType)
        {
            Type = newType switch
            {
                "Shift" => ScheduleEditType.Shift,
                "Leave" => ScheduleEditType.Leave,
                "Correction" => ScheduleEditType.Correction,
                _ => Type
            };

            TypeChanged?.Invoke();
        }
        public Action TypeChanged { get; set; }

        // 해당 월의 모든 직원들의 스케줄을 딕셔너리 형태로 반환.
        public Dictionary<Employee, List<object?>> GetAllSchedulesByThisMonth(DateTime date)
        {
            Employees = new ObservableCollection<Employee>(_empRepo.GetAll().OrderBy(e => e.Id).ToArray());
            Schedule[] schedules = _scheduleRepo.GetByMonth(date.Year, date.Month).ToArray();
            Shift[] shifts = _shiftRepo.GetAll().ToArray();

            Dictionary<Employee, List<object?>> empShifts = new();

            foreach (var emp in Employees)
            {
                var empSchedules = schedules
                    .Where(s => s.EmployeeId.Contains(emp.Id))
                    .Select(s => new { s.ShiftId, s.WorkDate })
                    .ToList();

                List<Leave>? empLeaves = _leaveRepo.GetByEmployeeIdAndMonth(emp.Id, date.Year, date.Month)?.ToList();

                int monthDays = DateTime.DaysInMonth(date.Year, date.Month);
                List<object?> fullMonths = new();

                for (int day = 1; day <= monthDays; day++)
                {
                    var shiftForDay = empSchedules.FirstOrDefault(s => s.WorkDate.Day == day);
                    var leaveForDay = empLeaves?.FirstOrDefault(l => l.LeaveAt.Day == day);
                    if (shiftForDay != null)
                    {
                        var shift = shifts.FirstOrDefault(sh => sh.Id == shiftForDay.ShiftId);
                        fullMonths.Add(shift);

                    }
                    else if (leaveForDay != null) fullMonths.Add(leaveForDay);
                    else fullMonths.Add(null);
                }

                empShifts.Add(emp, fullMonths);
            }

            return empShifts;
        }

        // SelectedEmployeeId: 선택된 직원 ID, date: 스케줄 날짜
        [RelayCommand]
        public void ScheduleEditHandler()
        {
            if (SelectedEmployeeId == 0)
            {
                MessageBox.Show("メンバーが選択されていません。");
                return;
            }

            // 명령 타입에 따른 분기
            var done = type switch
            {
                ScheduleEditType.Shift => UpsertScheduleHandler(),
                ScheduleEditType.Leave => UpsertLeave(),
                ScheduleEditType.Correction => UpsertCorrection()
            };

            if (done)
            {
                OnScheduleUpdated?.Invoke();
                IsVisibleScheduleEditDitalog = false;
            }
        }

        public Action OnScheduleUpdated { get; set; }

        private bool UpsertCorrection()
        {
            if (IsCorrection == null)
            {
                Correction.Id = _correctionRepo.GetNewId();
                Correction.EmployeeId = SelectedEmployeeId;
                Correction.When = CurrentDate.Date;
                _correctionRepo.Add(Correction);
            }
            else
            {
                IsCorrection.Note = Correction.Note;
                IsCorrection.CorrectTime = Correction.CorrectTime;
                IsCorrection.IsTimeIncreasing = Correction.IsTimeIncreasing;
                IsCorrection.Type = Correction.Type;
                IsCorrection.When = Correction.When;
                _correctionRepo.Update(IsCorrection);
            }

            return true;
        }

        private bool UpsertLeave()
        {
            // 해당 직원이 해당 날짜에 스케줄이 있으면 수정, 없으면 휴가 추가.
            if (IsLeave == null)
            {
                Leave.Id = _leaveRepo.GetNewId();
                Leave.EmployeeId = SelectedEmployeeId;
                Leave.LeaveAt = CurrentDate.Date;
                _leaveRepo.Add(Leave);
                return true;
            }
            else
            {
                IsLeave.LeaveAt = Leave.LeaveAt;
                IsLeave.IsPaid = Leave.IsPaid;
                IsLeave.Why = Leave.Why;
                _leaveRepo.Update(IsLeave);
                return true;
            }
        }

        private bool UpsertScheduleHandler()
        {
            // 해당 직원이 해당 날짜에 휴가가 있으면 메시지박스, 없으면 스케줄 추가.
            Leave? isLeave = _leaveRepo.GetByEmployeeIdAndDate(SelectedEmployeeId, CurrentDate.Date);
            if (isLeave != null)
            {
                MessageBox.Show("この日には休暇が当てられています。", "警告");       // 나중에 커스텀으로
                return false;
            }

            // 해당 직원이 해당 날짜에 스케줄이 있으면 기존 스케줄에서 제거.
            if (IsSchedule != null)
            {
                bool isChange = UpdateIsSchedule();
                if (isChange) return true;
            }

            return UpsertSchedule();
        }

        private bool UpdateIsSchedule()
        {
            // 변경사항이 없으면
            if (IsSchedule.ShiftId == Schedule.ShiftId) return true;

            DeleteEmpIdWhereIsShcehedule();
            return false;
        }

        private bool UpsertSchedule()
        {
            // 새로운 스케줄 추가. 이미 같은 날 동일한 스케줄이 있다면 거기 추가.
            Schedule alreadyExsitWithShiftType = _scheduleRepo.GetByShiftIdAndDate(Schedule.ShiftId, CurrentDate.Date);
            if (alreadyExsitWithShiftType != null)
            {
                alreadyExsitWithShiftType.EmployeeId.Add(SelectedEmployeeId);
                _scheduleRepo.Update(alreadyExsitWithShiftType);
            }
            else
            {
                Schedule.Id = _scheduleRepo.GetNewId();
                Schedule.EmployeeId = new List<int>() { SelectedEmployeeId };
                _scheduleRepo.Add(Schedule);
            }

            return true;
        }


        private void DeleteEmpIdWhereIsShcehedule()
        {
            if (IsSchedule == null) return;

            // 기존 스케줄에서 해당 직원만 제거.
            List<int> newEmployeeIds = new();
            IsSchedule.EmployeeId.ForEach(eId =>
            {
                if (eId == SelectedEmployeeId) return;
                newEmployeeIds.Add(eId);
            });

            IsSchedule.EmployeeId = newEmployeeIds;
            _scheduleRepo.Update(IsSchedule);
        }


        [RelayCommand]
        public void DeleteHandler()
        {
            var done = type switch
            {
                ScheduleEditType.Shift => UnAssignShift(),
                ScheduleEditType.Leave => UnAssignLeave(),
                ScheduleEditType.Correction => DeleteCorrection()
            };

            if(done)
            {
                OnScheduleUpdated?.Invoke();
                IsVisibleScheduleEditDitalog = false;
            }
        }

        private bool UnAssignShift()
        {
            var msgBox = new CustomMessageBox("このシフトから外しますか？") { Owner = Application.Current.MainWindow };

            msgBox.ShowDialog();

            bool result = msgBox.Result;

            if (result)
            {
                DeleteEmpIdWhereIsShcehedule();
            }

            return result;
        }

        private bool UnAssignLeave()
            {
            if (IsLeave == null) return false;

            var msgBox = new CustomMessageBox("この休暇から外しますか？") { Owner = Application.Current.MainWindow };

            msgBox.ShowDialog();

            bool result = msgBox.Result;

            if (result)
            {
                _leaveRepo.Delete(IsLeave.Id);
            }

            return result;
        }

        private bool DeleteCorrection()
        {
            if (IsCorrection == null) return false;

            var msgBox = new CustomMessageBox("この時間訂正を取り消しますか？") { Owner = Application.Current.MainWindow };

            msgBox.ShowDialog();

            bool result = msgBox.Result;

            if (result)
            {
                _correctionRepo.Delete(IsCorrection.Id);
            }

            return result;
        }
    }
}


// 이번 달 총 근무 시간, 시프트별 근무 시간, 시프트 횟수, 휴가 일수, 지각 및 초과 근무 횟수랑 시간