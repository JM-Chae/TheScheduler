using NLog;
using TheScheduler.Models;
using TheScheduler.Repositories;

namespace TheScheduler.Services.AutoScheduling
{
    public class ScheduleGenerationContext
    {
        private readonly ScheduleRepo _scheduleRepo = new();
        private readonly JobProfileRepo _jobProfileRepo = new();
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();


        // --- 입력 데이터 ---
        public IReadOnlyList<Employee> AllEmployees { get; }
        public IReadOnlyList<Shift> AllShifts { get; }
        public IReadOnlyList<Leave> AllLeaves { get; }
        public IReadOnlyList<JobProfile> AllJobs { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        // --- 상태 데이터 ---
        // Key: 직원 ID
        public Dictionary<int, double> HoursWorkedPerEmployeePerMonth { get; }
        public Dictionary<int, Dictionary<int, double>> HoursWorkedPerEmployeePerWeek { get; }
        public Dictionary<int, int> ConsecutiveWorkDays { get; }
        public List<Schedule> GeneratedSchedules { get; }
        public List<Schedule> UnassignedSchedules { get; }

        public ScheduleGenerationContext(
            IReadOnlyList<Employee> employees,
            IReadOnlyList<Shift> shifts,
            IReadOnlyList<Leave> leaves,
            DateTime startDate,
            DateTime endDate)
        {
            AllEmployees = employees;
            AllShifts = shifts;
            AllLeaves = leaves;
            AllJobs = _jobProfileRepo.GetAll();
            StartDate = startDate;
            EndDate = endDate;

            DateTime firstMondayOfStartWeek = StartDate.Date;
            while (firstMondayOfStartWeek.DayOfWeek != DayOfWeek.Monday) firstMondayOfStartWeek = firstMondayOfStartWeek.AddDays(-1);
            DateTime lastSundayOfEndWeek = EndDate.Date;
            while (lastSundayOfEndWeek.DayOfWeek != DayOfWeek.Sunday) lastSundayOfEndWeek = lastSundayOfEndWeek.AddDays(1);

            GeneratedSchedules = _scheduleRepo.GetByDateRange(firstMondayOfStartWeek, lastSundayOfEndWeek);

            UnassignedSchedules = [];
            HoursWorkedPerEmployeePerMonth = employees.ToDictionary(e => e.Id, _ => 0.0);
            HoursWorkedPerEmployeePerWeek = employees.ToDictionary(e => e.Id, _ => new Dictionary<int, double>());
            ConsecutiveWorkDays = employees.ToDictionary(e => e.Id, _ => 0);
            GetWorkTime();
        }
        // 월별, 주별 근무 시간 채우기
        private void GetWorkTime()
        {
            foreach (var schedule in GeneratedSchedules)
            {
                Shift? shift = AllShifts.FirstOrDefault(s => s.Id == schedule.ShiftId);
                if (shift == null) continue;

                foreach (int employeeId in schedule.EmployeeId)
                {
                    if (schedule.WorkDate.Month == StartDate.Month) HoursWorkedPerEmployeePerMonth[employeeId] += shift.Duration;

                    int weekNumber = GetWeekNumber(schedule.WorkDate);
                    if (!HoursWorkedPerEmployeePerWeek[employeeId].ContainsKey(weekNumber))
                    {
                        HoursWorkedPerEmployeePerWeek[employeeId][weekNumber] = 0.0;
                    }
                    HoursWorkedPerEmployeePerWeek[employeeId][weekNumber] += shift.Duration;
                }
            }
        }
        private static int GetWeekNumber(DateTime date) => System.Globalization.ISOWeek.GetWeekOfYear(date);



        // 이하 제약 확인 헬퍼

        // 월 근무 시간 넘겼는지 확인
        public bool IsOnOverMonthlyMaxWorkTime(int employeeId)
        {
            var emp = AllEmployees.FirstOrDefault(e => e.Id == employeeId);
            var empJobProfile = AllJobs.FirstOrDefault(j => j.Id == emp?.JobProfileId);
            var empsMonthlyWorkTime = HoursWorkedPerEmployeePerMonth[employeeId];

            if (emp == null || empJobProfile == null)
            {
                WarnLog($"[ScheduleConsistency] Missing object: EmployeeId={employeeId}, " +
                        $"Employee={emp}, JobProfile={empJobProfile}");
                return false;
            }

            if (emp.OverrideMaxMonthlyWorkTime.HasValue)
            {
                return empsMonthlyWorkTime > emp.OverrideMaxMonthlyWorkTime.Value;
            }

            if (!empJobProfile.MaxMonthlyWorkTime.HasValue) { return false; }

            return empsMonthlyWorkTime > empJobProfile.MaxMonthlyWorkTime.Value;
        }

        // 주 근무 시간 넘겼는지 확인
        public bool IsOnOverWeeklyMaxWorkTime(int employeeId, Schedule schedule)
        {
            var weekNumber = GetWeekNumber(schedule.WorkDate.Date);

            var emp = AllEmployees.FirstOrDefault(e => e.Id == employeeId);
            var empJobProfile = AllJobs.FirstOrDefault(j => j.Id == emp?.JobProfileId);
            var empsWeeklyWorkTime = HoursWorkedPerEmployeePerWeek[employeeId][weekNumber];



            if (emp == null || empJobProfile == null)
            {
                WarnLog($"[ScheduleConsistency] Missing object: EmployeeId={employeeId}, " +
                        $"Employee={emp}, JobProfile={empJobProfile}");
                return false;
            }

            if (emp.OverrideMaxWeeklyWorkTime.HasValue)
            {
                return empsWeeklyWorkTime > emp.OverrideMaxWeeklyWorkTime.Value;
            }

            if (!empJobProfile.MaxWeeklyWorkTime.HasValue) { return false; }

            return empsWeeklyWorkTime > empJobProfile.MaxWeeklyWorkTime.Value;
        }

        // 전날 근무가 다음 날 근무 불가능인지 확인
        public bool IsOnNextDayWorkProhibitionByYesterdays(int employeeId, Schedule schedule)
        {
            DateTime date = schedule.WorkDate.Date;
            Schedule? yesterdaySchedule = GeneratedSchedules.FirstOrDefault(s =>
                s.EmployeeId.Contains(employeeId) && s.WorkDate.Date == date.AddDays(-1));

            if (yesterdaySchedule == null) return false;

            var scheduleShift = AllShifts.FirstOrDefault(s => s.Id == yesterdaySchedule.ShiftId);

            if (scheduleShift == null)
            {
                WarnLog($"[ScheduleConsistency] Missing shift: EmployeeId={employeeId}, " +
                        $"Date={yesterdaySchedule.WorkDate:yyyy-MM-dd}, ShiftId={yesterdaySchedule.ShiftId}");

                return true;
            }

            return scheduleShift.IsTriggerForNextDayWorkProhibition;
        }

        // 휴가 확인
        public bool IsOnLeave(int employeeId, DateTime date)
        => AllLeaves.Any(l => l.EmployeeId == employeeId && l.LeaveAt.Date == date.Date);

        private static void WarnLog(string message)
        {
            _logger.Warn(message);
        }
    }
}