
using TheScheduler.Models;

namespace TheScheduler.Services.AutoScheduling.AbsoluteRuls
{
    public class MustNotWorkAfterProhibitedShiftRule : IScheduleRule
    {
        public bool IsSatisfied(Employee employee, Schedule schedule, ScheduleGenerationContext context)
        {
            return !context.IsOnNextDayWorkProhibitionByYesterdays(employee.Id, schedule);
        }
    }
}
