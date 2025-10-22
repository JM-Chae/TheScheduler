
using TheScheduler.Models;

namespace TheScheduler.Services.AutoScheduling.AbsoluteRuls
{
    public class MustNotBeOnLeaveRule : IScheduleRule
    {
        public bool IsSatisfied(Employee employee, Schedule schedule, ScheduleGenerationContext
      context) => !context.IsOnLeave(employee.Id, schedule.WorkDate);
    }
}
