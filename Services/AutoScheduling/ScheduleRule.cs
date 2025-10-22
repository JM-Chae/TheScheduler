using TheScheduler.Models;

namespace TheScheduler.Services.AutoScheduling
{
    public interface IScheduleRule
    {
        bool IsSatisfied(Employee employee, Schedule schedule, ScheduleGenerationContext context);
    }
}
