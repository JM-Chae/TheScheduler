using TheScheduler.Models;
using TheScheduler.Services.AutoScheduling.AbsoluteRuls;

namespace TheScheduler.Services.AutoScheduling
{
    public class AutoScheduleService
    {
        private readonly List<IScheduleRule> _hardRules;

        public AutoScheduleService()
        {
            _hardRules = new List<IScheduleRule>
            {
                new MustNotBeOnLeaveRule(),
                new MustNotWorkAfterProhibitedShiftRule(),
                new WorkHoursConstraintRule()
            };
        }

        public List<Schedule> Generate(List<Employee> employees, List<Shift> shifts, List<Leave> leaves, DateTime startDate, DateTime endDate)
        {
            var context = new ScheduleGenerationContext(employees, shifts, leaves, startDate, endDate);

            // 일자별 반복
            for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                // 시프트 반복
                foreach (var shift in shifts)
                {
                    var eligibleEmployees = GetEligibleEmployeesForShift(shift, date, context);
                }

                // 셀렉트 로직

            }


            return context.GeneratedSchedules;
        }

        private List<Employee> GetEligibleEmployeesForShift(Shift shift, DateTime date, ScheduleGenerationContext context)
        {
            // 직원별로 제약 조건을 돌려서 통과한 직원풀을 생성
            var eligibleEmployees = new List<Employee>();
            foreach (var employee in context.AllEmployees) // context.AllEmployees를 사용
            {
                if (employee.ForbiddenDays.Contains(date.DayOfWeek)) continue; // 해당 요일 근무 불가 직원이면 패스

                bool allHardRulesSatisfied = true;
                foreach (var rule in _hardRules)
                {
                    var tempSchedule = new Schedule // 임시 객체
                    {
                        Id = 0,
                        EmployeeId = new List<int> { employee.Id },
                        ShiftId = shift.Id,
                        WorkDate = date
                    };

                    if (!rule.IsSatisfied(employee, tempSchedule, context)) // 제약 조건 검증
                    {
                        allHardRulesSatisfied = false;
                        break;
                    }
                }
                if (allHardRulesSatisfied) eligibleEmployees.Add(employee);
            }
            return eligibleEmployees;
        }

    }

}