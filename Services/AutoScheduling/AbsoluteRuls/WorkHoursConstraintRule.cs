using TheScheduler.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TheScheduler.Services.AutoScheduling.AbsoluteRuls
{
    public class WorkHoursConstraintRule : IScheduleRule
    {
        public bool IsSatisfied(Employee employee, Schedule schedule, ScheduleGenerationContext context)
        {
            // Get the shift for the current schedule
            Shift? shift = context.AllShifts.FirstOrDefault(s => s.Id == schedule.ShiftId);
            if (shift == null) return false;

            return !(context.IsOnOverMonthlyMaxWorkTime(employee.Id) ||
                    context.IsOnOverWeeklyMaxWorkTime(employee.Id, schedule));
        }
    }
}