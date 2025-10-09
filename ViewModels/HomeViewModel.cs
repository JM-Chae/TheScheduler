using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheScheduler.Models;
using TheScheduler.Repositories;

namespace TheScheduler.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly EmployeeRepo _repo = new();
        private readonly ScheduleRepo _scheduleRepo = new();
        private readonly ShiftRepo _shiftRepo = new();

        public HomeViewModel()
        {
        }


        public Dictionary<Employee, List<Shift?>> getAllSchedulesByThisMonth(DateTime value)
        {
            Employee[] employees = _repo.GetAll().OrderBy(e => e.Id).ToArray();
            Schedule[] schedule = _scheduleRepo.GetByMonth(value.Year, value.Month).ToArray();
            Shift[] shifts = _shiftRepo.GetAll().ToArray();

            Dictionary<Employee, List<Shift?>> empShifts = new();

            foreach (var emp in employees)
            {
                var schedules = schedule
                    .Where(s => s.EmployeeId.Contains(emp.Id))
                    .Select(s => new { s.ShiftId, s.WorkDate })
                    .ToList();

                int monthDays = DateTime.DaysInMonth(value.Year, value.Month);
                List<Shift?> fullMonthShifts = new();

                for (int day = 1; day <= monthDays; day++)
                {
                    var shiftForDay = schedules.FirstOrDefault(s => s.WorkDate.Day == day);
                    if (shiftForDay != null)
                    {
                        var shift = shifts.FirstOrDefault(sh => sh.Id == shiftForDay.ShiftId);
                        fullMonthShifts.Add(shift);
                    }
                    else
                    {
                        fullMonthShifts.Add(null);
                    }
                }

   
                empShifts.Add(emp, fullMonthShifts);
            }

            return empShifts;
        }
    }
}
