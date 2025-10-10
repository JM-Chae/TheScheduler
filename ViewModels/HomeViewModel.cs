using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TheScheduler.Models;
using TheScheduler.Repositories;

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

        // 해당 월의 모든 직원들의 스케줄을 딕셔너리 형태로 반환.
        public Dictionary<Employee, List<object?>> GetAllSchedulesByThisMonth(DateTime date)
        {
            Employee[] employees = _empRepo.GetAll().OrderBy(e => e.Id).ToArray();
            Schedule[] schedules = _scheduleRepo.GetByMonth(date.Year, date.Month).ToArray();
            Shift[] shifts = _shiftRepo.GetAll().ToArray();

            Dictionary<Employee, List<object?>> empShifts = new();

            foreach (var emp in employees)
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

        // empId: 선택된 직원 ID, date: 스케줄 날짜
        public void ScheduleEditHandler(int empId, DateTime date)
        {
            if (empId == 0)
            {
                MessageBox.Show("メンバーが選択されていません。");
                return;
            }

            // 명령 타입에 따른 분기
            switch (type)
            {
                case ScheduleEditType.Shift:
                    IsSchedule = _scheduleRepo.GetByEmployeeIdAndDate(empId, Schedule.WorkDate);
                    UpsertScheduleHandler(empId);
                    break;
                case ScheduleEditType.Leave:
                    IsLeave = _leaveRepo.GetByEmployeeIdAndDate(empId, Leave.LeaveAt);
                    UpsertLeave();
                    break;
                case ScheduleEditType.Correction:
                    IsCorrection = _correctionRepo.GetByEmployeeIdAndDate(empId, Correction.When);
                    UpsertCorrection();
                    break;
            }
        }

        private void UpsertCorrection()
        {
            if (IsCorrection == null)
            {
                _correctionRepo.Add(Correction);
            }
            else
            {
                IsCorrection.Note = Correction.Note;
                IsCorrection.CorrectTime = Correction.CorrectTime;
                IsCorrection.IsPaid = Correction.IsPaid;
                IsCorrection.Type = Correction.Type;
                IsCorrection.When = Correction.When;
                _correctionRepo.Update(IsCorrection);
            }
        }

        private void UpsertLeave()
        {
            // 해당 직원이 해당 날짜에 스케줄이 있으면 수정, 없으면 휴가 추가.
            if (IsLeave == null)
            {
                _leaveRepo.Add(Leave);
            } else
            {
                IsLeave.LeaveAt = Leave.LeaveAt;
                IsLeave.IsPaid = Leave.IsPaid;
                IsLeave.Why = Leave.Why;
                _leaveRepo.Update(IsLeave);
            }
        }

        private void UpsertScheduleHandler(int empId)
        {
            // 해당 직원이 해당 날짜에 휴가가 있으면 메시지박스, 없으면 스케줄 추가.
            Leave? isLeave = _leaveRepo.GetByEmployeeIdAndDate(empId, Schedule.WorkDate);
            if (isLeave == null)
            { 
                MessageBox.Show("この日には休暇が当てられています。");
                return;
            }

            // 해당 직원이 해당 날짜에 스케줄이 있으면 기존 스케줄에서 제거.
            if (IsSchedule != null)
            {
                UpdateIsSchedule(empId);
            }

            UpsertSchedule(empId);
        }

        public void UpdateIsSchedule(int empId)
        {
            if (IsSchedule.ShiftId == Schedule.ShiftId) return; // 변경사항이 없으면 리턴.

            // 기존 스케줄에서 해당 직원만 제거.
            List<int> newEmployeeIds = new();
            IsSchedule.EmployeeId.ForEach(eId =>
            {
                if (eId == empId) return;
                newEmployeeIds.Add(eId);
            });

            IsSchedule.EmployeeId = newEmployeeIds;
            _scheduleRepo.Update(IsSchedule);
        }

        public void UpsertSchedule(int empId)
        {
            // 새로운 스케줄 추가. 이미 같은 날 동일한 스케줄이 있다면 거기 추가.
            Schedule alreadyExsitWithShiftType = _scheduleRepo.GetByShiftIdAndDate(empId, Schedule.WorkDate);
            if (alreadyExsitWithShiftType != null)
            {
                alreadyExsitWithShiftType.EmployeeId.Add(empId);
                _scheduleRepo.Update(alreadyExsitWithShiftType);
            }
            else
            {
                _scheduleRepo.Add(Schedule);
            }
        }
    }
}
