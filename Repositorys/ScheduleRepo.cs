using TheScheduler.Models;
using TheScheduler.Data;
using LiteDB;

namespace TheScheduler.Repositories
{
    public class ScheduleRepo
    {
        public int GetNewId()
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Schedule>("schedules");
            var last = col.Query()
                            .OrderByDescending(x => x.Id)
                            .Limit(1)
                            .FirstOrDefault();

            return (last?.Id ?? 0) + 1;
        }
       
        public int Add(Schedule schedule)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Schedule>("schedules");
            return col.Insert(schedule);
        }

        public Schedule GetByShiftIdAndDate(int shiftId, DateTime date)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Schedule>("schedules");
            return col.FindOne(s => s.ShiftId == shiftId && s.WorkDate.Date == date.Date);
        }

        public List<Schedule> GetByMonth(int year, int month)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Schedule>("schedules");
            var startDate = new DateTime(year, month, 1).Date;
            var endDate = startDate.AddMonths(1).AddDays(-1).Date;
            return col.Find(s => s.WorkDate.Date >= startDate.Date && s.WorkDate.Date <= endDate.Date).ToList();
        }

        public Schedule? GetByEmployeeIdAndDate(int empId, DateTime date)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Schedule>("schedules");
            return col.FindOne(s => s.EmployeeId.Contains(empId) && s.WorkDate.Date == date.Date);
        }

        public List<Schedule> GetAll()
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Schedule>("schedules");
            return col.FindAll().ToList();
        }

        public Schedule? GetById(int id)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Schedule>("schedules");
            return col.FindById(id);
        }

        public bool Update(Schedule schedule)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Schedule>("schedules");
            return col.Update(schedule);
        }

        public bool Delete(int id)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Schedule>("schedules");
            return col.Delete(id);
        }
    }
}
