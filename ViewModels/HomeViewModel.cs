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
using Nager.Date;
using Nager.Date.Model;

namespace TheScheduler.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly EmployeeRepo _empRepo = new();
        private readonly ScheduleRepo _scheduleRepo = new();
        private readonly ShiftRepo _shiftRepo = new();
        private readonly LeaveRepo _leaveRepo = new();
        private readonly CorrectionRepo _correctionRepo = new();

        private int _currentYear;
        private int _monthDays;
        public IEnumerable<PublicHoliday> PublicHolidays { get; private set; }


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
        private ObservableCollection<EmployeeMonthlySummary> _summaries = new();

        [ObservableProperty]
        private ObservableCollection<Shift> _summaryDisplayShifts = new();
        private List<Dictionary<Shift, ShiftConditionVaildCount>> _shiftConditionWarns = new();

        [ObservableProperty]
        public ObservableCollection<DayShiftWarning> _allShiftWarnings;


        [ObservableProperty]
        private ScheduleEditType? type;

        private ObservableCollection<Schedule> _schedules;
        [ObservableProperty]
        private Schedule? _schedule;
        private Schedule? IsSchedule;

        [ObservableProperty]
        private Leave? _leave;
        private Leave? IsLeave;

        [ObservableProperty]
        private Correction? _correction;
        private Correction? IsCorrection;

        public HomeViewModel()
        {
            type = ScheduleEditType.Shift;
            LoadHolidays(DateTime.Now.Year);
        }

        private void LoadHolidays(int year)
        {
            PublicHolidays = DateSystem.GetPublicHolidays(year, CountryCode.JP);
            _currentYear = year;
        }

        public void ScheduleEditDialogOpen(int empId, DateTime date)
        {
            SelectedEmployee = Employees.FirstOrDefault(e => e.Id.Equals(empId));

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
            { Id = 0, ShiftId = Shifts.First().Id, WorkDate = CurrentDate.Date, EmployeeId = new List<int>() { SelectedEmployeeId } };

            if (IsLeave != null) Leave = DeepCopyHandler.Clone(IsLeave);
            else Leave = new Leave()
            { Id = 0, LeaveAt = CurrentDate.Date, EmployeeId = SelectedEmployeeId, IsPaid = true, Why = "" };

            if (IsCorrection != null) Correction = DeepCopyHandler.Clone(IsCorrection);
            else Correction = new Correction()
            { Id = 0, When = CurrentDate.Date, EmployeeId = SelectedEmployeeId, IsTimeIncreasing = false, Note = "", CorrectTime = new TimeOnly(1, 0), Type = CorrectionType.遅刻 };

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
            if (date.Year != _currentYear)
            {
                LoadHolidays(date.Year);
            }
            int monthDays = DateTime.DaysInMonth(date.Year, date.Month);

            // DB에서 데이터 갖고 오기
            Employees = new ObservableCollection<Employee>(_empRepo.GetAll().OrderBy(e => e.Id).ToArray());
            _schedules = new ObservableCollection<Schedule>(_scheduleRepo.GetByMonth(date.Year, date.Month));
            Shifts = new ObservableCollection<Shift>(_shiftRepo.GetAll());
            // 요약에 표시할 시프트 데이터 생성
            var allSummaryShifts = new List<Shift>(Shifts);

            //스케쥴에서 삭제된 직원을 찾아내고 삭제 객체로 표현
            var scheduledEmployeeIds = _schedules.SelectMany(s => s.EmployeeId).Distinct();
            var existingEmployeeIds = Employees.Select(e => e.Id);
            var deletedEmployeeIds = scheduledEmployeeIds.Except(existingEmployeeIds);

            var deletedEmployeePlaceholders = deletedEmployeeIds.Select(id => new Employee
            {
                Id = id,
                Name = "{削除済み}",
                Sex = Sex.女性,
                Position = Position.削除済み,
                Note = "",
                Address = "",
                Bod = null,
                Category = 0,
                HireAt = null,
                Leaves = null,
                Phone = ""
            }
            ).ToList();

            // 표시용 직원 객체
            var allDisplayEmployees = Employees.Concat(deletedEmployeePlaceholders).OrderBy(e => e.Id).ToList();
            var newSummaries = new List<EmployeeMonthlySummary>();
            var empShifts = new Dictionary<Employee, List<object?>>();

            // 직원별 데이터 설정 진입
            foreach (var emp in allDisplayEmployees)
            {
                var empSchedules = _schedules
                    .Where(s => s.EmployeeId.Contains(emp.Id))
                    .Select(s => new { s.ShiftId, s.WorkDate })
                    .ToList();

                // 해당 월 휴가 조회
                List<Leave>? empLeaves = _leaveRepo.GetByEmployeeIdAndMonth(emp.Id, date.Year, date.Month)?.ToList();
                
                List<object?> fullMonths = new();

                // 요약 기본 값 할당
                var summary = new EmployeeMonthlySummary
                {
                    EmployeeId = emp.Id,
                    EmployeeName = emp.Name,
                    WorkDays = 0,
                    PaidLeaveDays = 0,
                    UnpaidLeaveDays = 0,
                    TotalWorkHours = "0H",
                    ShiftCounts = Shifts.ToDictionary(s => s.Id, s => 0)
                };

                TimeSpan totalWorkTime = TimeSpan.Zero;

                // 일별 할당 진입
                for (int day = 1; day <= monthDays; day++)
                {
                    // 근무, 혹은 휴가 설정
                    var shiftForDay = empSchedules.FirstOrDefault(s => s.WorkDate.Day == day);
                    var leaveForDay = empLeaves?.FirstOrDefault(l => l.LeaveAt.Day == day);

                    if (shiftForDay != null)
                    {
                    //시프트 ID 조회 불가할 경우 삭제된 객체로 표현
                        var shift = Shifts.FirstOrDefault(sh => sh.Id == shiftForDay.ShiftId) ?? new Shift
                        {
                            Id = shiftForDay.ShiftId,
                            Name = "{削除済み}",
                            Start = new TimeOnly(0, 0),
                            End = new TimeOnly(0, 0),
                            RestInMinutes = 0,
                            ShiftColor = ShiftColor.Y,
                            Conditions = new List<ShiftCondition>()
                        };

                        if (shift.Name == "{削除済み}")
                        {
                            var isShift = allSummaryShifts.Find(s => s.Id == shift.Id);
                            if (isShift == null) allSummaryShifts.Add(shift);
                        }

                        fullMonths.Add(shift);

                        summary.WorkDays++;
                        totalWorkTime += shift.End - shift.Start - TimeSpan.FromMinutes(shift.RestInMinutes);

                        if (summary.ShiftCounts.ContainsKey(shift.Id))
                        {
                            summary.ShiftCounts[shift.Id]++;
                        }
                        else if (shift.Name == "{削除済み}")
                        {
                            if (!summary.ShiftCounts.ContainsKey(shift.Id))
                            {
                                summary.ShiftCounts.Add(shift.Id, 1);
                            }
                            else
                            {
                                summary.ShiftCounts[shift.Id]++;
                            }
                        }
                    }
                    else if (leaveForDay != null)
                    {
                        fullMonths.Add(leaveForDay);
                        if (leaveForDay.IsPaid) summary.PaidLeaveDays++;
                        else summary.UnpaidLeaveDays++;
                    }
                    else
                    {
                        fullMonths.Add(null);
                    }
                }

                summary.TotalWorkHours = $"{totalWorkTime.TotalHours:F1}H";
                newSummaries.Add(summary);
                empShifts.Add(emp, fullMonths);
            }

            if(monthDays != _monthDays)
            {
                _monthDays = monthDays;
                GetShiftConditionWarnMessage();
            }

            Summaries = new ObservableCollection<EmployeeMonthlySummary>(newSummaries);
            SummaryDisplayShifts = new ObservableCollection<Shift>(allSummaryShifts.OrderBy(s => s.Name));
            return empShifts;
        }

        private void GetShiftConditionWarnMessage()
        {
            _shiftConditionWarns = new List<Dictionary<Shift, ShiftConditionVaildCount>>(_monthDays + 1);

            for (int i = 0; i <= _monthDays; i++)
            {
                _shiftConditionWarns.Add(new Dictionary<Shift, ShiftConditionVaildCount>());
            }

            foreach (var item in _schedules)
            {
                var shift = Shifts.FirstOrDefault(s => s.Id == item.ShiftId);

                if (shift == null) continue;

                var shiftConditionVaildCount = new ShiftConditionVaildCount
                {
                    PositionCount = shift.Conditions.ToDictionary(c => c.Position, c => c.Value)
                };

                item.EmployeeId.ForEach(empId =>
                {
                    var emp = Employees.FirstOrDefault(e => e.Id == empId);
                    if (emp == null) return;
                    if (shiftConditionVaildCount.PositionCount.ContainsKey(emp.Position))
                    {
                        shiftConditionVaildCount.PositionCount[emp.Position]--;
                    }
                });

                _shiftConditionWarns[item.WorkDate.Day].Add(shift, shiftConditionVaildCount);
            }

            GenerateWarnings();
        }

        private void GenerateWarnings()
        {
            AllShiftWarnings = new ObservableCollection<DayShiftWarning>();

            for (int day = 1; day < _shiftConditionWarns.Count; day++)
            {
                var dict = _shiftConditionWarns[day];
                foreach (var kv in dict)
                {
                    var shift = kv.Key;
                    var cond = kv.Value;

                    foreach (var posPair in cond.PositionCount)
                    {
                        var pos = posPair.Key;
                        var remaining = posPair.Value;

                        if (remaining > 0)
                        {
                            AllShiftWarnings.Add(new DayShiftWarning
                            {
                                Day = day,
                                ShiftName = shift.Name,
                                Position = pos,
                                Remaining = remaining
                            });
                        }
                    }
                }
            }
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
            if (IsSchedule?.ShiftId == Schedule?.ShiftId) return true;

            DeleteEmpIdWhereIsShcehedule();
            return false;
        }

        private bool UpsertSchedule()
        {
            Shift shift = Shifts.First(s => s.Id == Schedule.ShiftId);

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
                
                _shiftConditionWarns[CurrentDate.Day].Add(shift, new ShiftConditionVaildCount
                {
                    PositionCount = shift.Conditions.ToDictionary(c => c.Position, c => c.Value)
                });
            }

            var warns = _shiftConditionWarns[CurrentDate.Day];
            if (warns.ContainsKey(shift))
            {
                var cond = warns[shift];
                if (cond.PositionCount.ContainsKey(SelectedEmployee.Position))
                {
                    cond.PositionCount[SelectedEmployee.Position]--;
                }
                GenerateWarnings();
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

            if(IsSchedule.EmployeeId.Count == 0)
            {
                _scheduleRepo.Delete(IsSchedule.Id);
            } 
            else
            {
                IsSchedule.EmployeeId = newEmployeeIds;
                _scheduleRepo.Update(IsSchedule);
            }

            Shift shift = Shifts.First(s => s.Id == IsSchedule.ShiftId);
            var warns = _shiftConditionWarns[CurrentDate.Day];
            if (warns.ContainsKey(shift))
            {
                var cond = warns[shift];
                if (cond.PositionCount.ContainsKey(SelectedEmployee.Position))
                {
                    cond.PositionCount[SelectedEmployee.Position]++;
                    GenerateWarnings();
                }
            }
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

            if (done)
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