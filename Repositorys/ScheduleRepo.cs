using TheScheduler.Models;
using TheScheduler.Data;
using LiteDB;

namespace TheScheduler.Repositories
{
    public class ScheduleRepo
    {
        public int Add(Schedule schedule)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Schedule>("schedules");
            return col.Insert(schedule);
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
