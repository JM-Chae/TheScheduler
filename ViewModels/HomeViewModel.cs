using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nager.Date;
using Nager.Date.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private DateTime _currentDate;  // 데이터 CRUD용 날짜.
        private int _currentYear;  // 데이터 가공 시 조건으로 사용
        private int _monthDays;    // 데이터 가공 시 조건으로 사용
        public IEnumerable<PublicHoliday> PublicHolidays { get; private set; }

        private ObservableCollection<Schedule> _schedules;

        [ObservableProperty]
        private Employee _selectedEmployee;
        private List<Employee> _employees;
        private int _selectedEmployeeId;

        [ObservableProperty]
        private bool _isVisibleScheduleEditDitalog = false;

        [ObservableProperty]
        private ObservableCollection<Shift> _shifts;
        [ObservableProperty]
        private ObservableCollection<EmployeeMonthlySummary> _summaries = new();

        [ObservableProperty]
        private ObservableCollection<Shift> _summaryDisplayShifts = new();
        private List<Dictionary<Shift, ShiftConditionVaildCount>> _shiftConditionWarns = new();

        [ObservableProperty]
        public ObservableCollection<DayShiftWarning> _allShiftWarnings = new();

        [ObservableProperty]
        private int _hoveredColumnIndex = -1; // 컬럼 호버 

        [ObservableProperty]
        private ScheduleEditType? type;

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

        // 해당 연도의 공휴일을 불러옴.
        private void LoadHolidays(int year)
        {
            PublicHolidays = DateSystem.GetPublicHolidays(year, CountryCode.JP);
            _currentYear = year;
        }

        [RelayCommand]
        // ScheduleEditType을 문자열로 받아서 enum으로 변환 후 설정(컴포넌트 변경 용도)
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

        //DataGrid에서 (더블클릭) 직원과 날짜를 받아서 스케줄 편집 다이얼로그를 오픈.
        public void ScheduleEditDialogOpen(int empId, DateTime date)
        {
            SelectedEmployee = _employees.FirstOrDefault(e => e.Id.Equals(empId));

            _selectedEmployeeId = empId;
            CurrentDate = date;

            if (Shifts.Count == 0)
            {
                MessageBox.Show("まずはシフトを登録してください。");
                return;
            }

            IsSchedule = _scheduleRepo.GetByEmployeeIdAndDate(_selectedEmployeeId, CurrentDate.Date);
            IsLeave = _leaveRepo.GetByEmployeeIdAndDate(_selectedEmployeeId, CurrentDate.Date);
            IsCorrection = _correctionRepo.GetByEmployeeIdAndDate(_selectedEmployeeId, CurrentDate.Date);

            if (IsSchedule != null) Schedule = DeepCopyHandler.Clone(IsSchedule);
            else Schedule = new Schedule()
            { Id = 0, ShiftId = Shifts.First().Id, WorkDate = CurrentDate.Date, EmployeeId = new List<int>() { _selectedEmployeeId } };

            if (IsLeave != null) Leave = DeepCopyHandler.Clone(IsLeave);
            else Leave = new Leave()
            { Id = 0, LeaveAt = CurrentDate.Date, EmployeeId = _selectedEmployeeId, IsPaid = true, Why = "" };

            if (IsCorrection != null) Correction = DeepCopyHandler.Clone(IsCorrection);
            else Correction = new Correction()
            { Id = 0, When = CurrentDate.Date, EmployeeId = _selectedEmployeeId, Note = "", CorrectMin = 0, Type = CorrectionType.遅刻 };

            IsVisibleScheduleEditDitalog = true;
        }

        [RelayCommand]
        public void ScheduleEditDialogClose()
        {
            IsVisibleScheduleEditDitalog = false;
        }

        // 해당 월의 모든 직원들의 스케줄을 딕셔너리 형태로 반환.
        public Dictionary<Employee, List<DailyCellInfo>> GetAllSchedulesByThisMonth(DateTime date)
        {
            if (date.Year != _currentYear)
            {
                LoadHolidays(date.Year);
            }
            int monthDays = DateTime.DaysInMonth(date.Year, date.Month);


            // 모든 데이터 로드 및 삭제된 empId, shiftId를 참조하는 스케쥴을 허용하기 때문에, 삭제된 객체도 생성
            var deletedEmployeePlaceholders = _LoadAllDataForMonth(date);
            var employeeDict = _employees.ToDictionary(e => e.Id);
            var shiftDict = Shifts.ToDictionary(s => s.Id);

            var allSummaryShifts = new List<Shift>(Shifts);
            var scheduledShiftIds = _schedules.Select(s => s.ShiftId).Distinct();
            var deletedShiftIds = scheduledShiftIds.Where(id => !shiftDict.ContainsKey(id));

            foreach (var deletedId in deletedShiftIds)
            {
                allSummaryShifts.Add(new Shift
                {
                    Id = deletedId,
                    Name = "{削除済み}",
                    Start = new TimeOnly(0, 0),
                    End = new TimeOnly(0, 0),
                    RestInMinutes = 0,
                    ShiftColor = ShiftColor.Y,
                    Conditions = new List<ShiftCondition>()
                });
            }

            var allDisplay_employees = _employees.Concat(deletedEmployeePlaceholders).OrderBy(e => e.Id).ToList();

            // 데이터 가공
            var (newSummaries, empShifts) = _ProcessMonthlyData(date, allDisplay_employees, employeeDict, shiftDict, allSummaryShifts);

            if(monthDays != _monthDays)
            {
                _monthDays = monthDays;
                _CalculateShiftWarnings();
            }

            Summaries = new ObservableCollection<EmployeeMonthlySummary>(newSummaries);
            SummaryDisplayShifts = new ObservableCollection<Shift>(allSummaryShifts.OrderBy(s => s.Start));
            return empShifts;
        }

        private void _CalculateShiftWarnings()
        {
            _shiftConditionWarns = new List<Dictionary<Shift, ShiftConditionVaildCount>>(_monthDays + 1);

            for (int i = 0; i <= _monthDays; i++)
            {
                _shiftConditionWarns.Add(new Dictionary<Shift, ShiftConditionVaildCount>());
            }

            var shiftDict = Shifts.ToDictionary(s => s.Id);
            var employeeDict = _employees.ToDictionary(e => e.Id);

            foreach (var item in _schedules)
            {
                if (!shiftDict.TryGetValue(item.ShiftId, out var shift)) continue;

                var shiftConditionVaildCount = new ShiftConditionVaildCount
                {
                    PositionCount = shift.Conditions.ToDictionary(c => c.Position, c => c.Value)
                };

                item.EmployeeId.ForEach(empId =>
                {
                    if (!employeeDict.TryGetValue(empId, out var emp)) return;
                    if (shiftConditionVaildCount.PositionCount.ContainsKey(emp.Position))
                    {
                        shiftConditionVaildCount.PositionCount[emp.Position]--;
                    }
                });

                _shiftConditionWarns[item.WorkDate.Day].Add(shift, shiftConditionVaildCount);
            }

            AllShiftWarnings.Clear();

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

        // _selectedEmployeeId: 선택된 직원 ID, date: 스케줄 날짜
        [RelayCommand]
        public void ScheduleEditHandler()
        {
            if (_selectedEmployeeId == 0)
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
                Correction.EmployeeId = _selectedEmployeeId;
                Correction.When = CurrentDate.Date;
                _correctionRepo.Add(Correction);
            }
            else
            {
                IsCorrection.Note = Correction.Note;
                IsCorrection.CorrectMin = Correction.CorrectMin;
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
                Leave.EmployeeId = _selectedEmployeeId;
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
            Leave? isLeave = _leaveRepo.GetByEmployeeIdAndDate(_selectedEmployeeId, CurrentDate.Date);
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

        private bool UpsertSchedule()
        {
            Shift shift = Shifts.First(s => s.Id == Schedule.ShiftId);

            // 새로운 스케줄 추가. 이미 같은 날 동일한 스케줄이 있다면 거기 추가.
            Schedule alreadyExsitWithShiftType = _scheduleRepo.GetByShiftIdAndDate(Schedule.ShiftId, CurrentDate.Date);
            if (alreadyExsitWithShiftType != null)
            {
                alreadyExsitWithShiftType.EmployeeId.Add(_selectedEmployeeId);
                _scheduleRepo.Update(alreadyExsitWithShiftType);
                // _schedules 최신화
                var existingScheduleInCollection = _schedules.FirstOrDefault(s => s.Id == alreadyExsitWithShiftType.Id);
                if (existingScheduleInCollection != null)
                {
                    existingScheduleInCollection.EmployeeId = alreadyExsitWithShiftType.EmployeeId;
                }
            }
            else
            {
                Schedule.Id = _scheduleRepo.GetNewId();
                Schedule.EmployeeId = new List<int>() { _selectedEmployeeId };
                _scheduleRepo.Add(Schedule);
                // _schedules 최신화
                _schedules.Add(Schedule);
            }

            _CalculateShiftWarnings();

            return true;
        }

        private bool UpdateIsSchedule()
        {
            // 변경사항이 없으면
            if (IsSchedule?.ShiftId == Schedule?.ShiftId) return true;

            DeleteEmpIdWhereIsShcehedule();
            return false;
        }

        private void DeleteEmpIdWhereIsShcehedule()
        {
            if (IsSchedule == null) return;

            // 기존 스케줄에서 해당 직원만 제거.
            List<int> newEmployeeIds = new();
            IsSchedule.EmployeeId.ForEach(eId =>
            {
                if (eId == _selectedEmployeeId) return;
                newEmployeeIds.Add(eId);
            });

            if(newEmployeeIds.Count == 0)
            {
                _scheduleRepo.Delete(IsSchedule.Id);
                // _schedules 최신화
                var scheduleToRemove = _schedules.FirstOrDefault(s => s.Id == IsSchedule.Id);
                if (scheduleToRemove != null)
                {
                    _schedules.Remove(scheduleToRemove);
                }
            } 
            else
            {
                IsSchedule.EmployeeId = newEmployeeIds;
                _scheduleRepo.Update(IsSchedule);
                // _schedules 최신화
                var existingScheduleInCollection = _schedules.FirstOrDefault(s => s.Id == IsSchedule.Id);
                if (existingScheduleInCollection != null)
                {
                    existingScheduleInCollection.EmployeeId = IsSchedule.EmployeeId;
                }
            }

            _CalculateShiftWarnings();
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

        private List<Employee> _LoadAllDataForMonth(DateTime date)
        {
            _schedules = new ObservableCollection<Schedule>(_scheduleRepo.GetByMonth(date.Year, date.Month).ToList());
            _employees = _empRepo.GetAll().OrderBy(e => e.Id).ToList();
            Shifts = new ObservableCollection<Shift>(_shiftRepo.GetAll().ToList());

            var scheduledEmployeeIds = _schedules.SelectMany(s => s.EmployeeId).Distinct();
            var existingEmployeeIds = _employees.Select(e => e.Id);
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
            }).ToList();

            return deletedEmployeePlaceholders;
        }
        private (List<EmployeeMonthlySummary> newSummaries, Dictionary<Employee, List<DailyCellInfo>> empShifts) _ProcessMonthlyData(DateTime date, List<Employee> allDisplay_employees, Dictionary<int, Employee> employeeDict, Dictionary<int, Shift> shiftDict, List<Shift> allSummaryShifts)
        {
            int monthDays = DateTime.DaysInMonth(date.Year, date.Month);
            var newSummaries = new List<EmployeeMonthlySummary>();
            var empShifts = new Dictionary<Employee, List<DailyCellInfo>>();

            foreach (var emp in allDisplay_employees)
            {
                var empSchedulesByDay = _schedules
                    .Where(s => s.EmployeeId.Contains(emp.Id))
                    .ToDictionary(s => s.WorkDate.Day);

                var empLeavesByDay = _leaveRepo.GetByEmployeeIdAndMonth(emp.Id, date.Year, date.Month)?
                                               .ToDictionary(l => l.LeaveAt.Day);

                var empCorrectionByDay = _correctionRepo.GetByEmployeeIdAndMonth(emp.Id, date.Year, date.Month)
                                                        .ToDictionary(c => c.When.Day);
                
                List<DailyCellInfo> fullMonths = new();

                var summary = new EmployeeMonthlySummary
                {
                    EmployeeId = emp.Id,
                    EmployeeName = emp.Name,
                    WorkDays = 0,
                    PaidLeaveDays = 0,
                    UnpaidLeaveDays = 0,
                    TotalWorkHours = "0H",
                    ShiftCounts = allSummaryShifts.ToDictionary(s => s.Id, s => 0)
                };

                TimeSpan totalWorkTime = TimeSpan.Zero;

                for (int day = 1; day <= monthDays; day++)
                {
                    empSchedulesByDay.TryGetValue(day, out var shiftForDay);
                    empLeavesByDay.TryGetValue(day, out var leaveForDay);
                    empCorrectionByDay.TryGetValue(day, out var correctionForDay);

                    var dailyCellInfo = new DailyCellInfo { Correction = correctionForDay };

                    if (shiftForDay != null)
                    {
                        shiftDict.TryGetValue(shiftForDay.ShiftId, out var shift);
                        shift ??= new Shift
                        {
                            Id = shiftForDay.ShiftId,
                            Name = "{削除済み}",
                            Start = new TimeOnly(0, 0),
                            End = new TimeOnly(0, 0),
                            RestInMinutes = 0,
                            ShiftColor = ShiftColor.Y,
                            Conditions = new List<ShiftCondition>()
                        };

                        dailyCellInfo.Shift = shift;

                        summary.WorkDays++;
                        totalWorkTime += shift.End - shift.Start - TimeSpan.FromMinutes(shift.RestInMinutes);

                        if (summary.ShiftCounts.TryGetValue(shift.Id, out int currentCount))
                        {
                            summary.ShiftCounts[shift.Id] = currentCount + 1;
                        }
                        else
                        {
                            summary.ShiftCounts.Add(shift.Id, 1);
                        }
                    }
                    else if (leaveForDay != null)
                    {
                        dailyCellInfo.Leave = leaveForDay;
                        if (leaveForDay.IsPaid) summary.PaidLeaveDays++;
                        else summary.UnpaidLeaveDays++;
                    }

                    fullMonths.Add(dailyCellInfo);

                    // 근무 정정 있을 시 조정
                    if (correctionForDay != null) totalWorkTime += TimeSpan.FromMinutes(correctionForDay.CorrectMin);
                }

                summary.TotalWorkHours = $"{totalWorkTime.TotalHours:F1}H";
                newSummaries.Add(summary);
                empShifts.Add(emp, fullMonths);
            }
            return (newSummaries, empShifts);
        }
    }
}