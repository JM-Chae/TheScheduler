using TheScheduler.Models;
using TheScheduler.Data;
using LiteDB;

namespace TheScheduler.Repositories
{
    public class LeaveRepo
    {
        public int GetNewId()
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Leave>("leaves");
            var last = col.Query()
                           .OrderByDescending(x => x.Id)
                           .Limit(1)
                           .FirstOrDefault();

            return (last?.Id ?? 0) + 1;
        }

        public int Add(Leave leave)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Leave>("leaves");
            return col.Insert(leave);
        }

        public List<Leave> GetByEmployeeIdAndMonth(int empId, int year, int month)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Leave>("leaves");
            var startDate = new DateTime(year, month, 1).Date;
            var endDate = startDate.AddMonths(1).AddDays(-1).Date;
            return col.Find(l => l.EmployeeId == empId && l.LeaveAt.Date >= 
                                    startDate.Date && l.LeaveAt.Date <= 
                                    endDate.Date).ToList();
        }

        public Leave? GetByEmployeeIdAndDate(int empId, DateTime date)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Leave>("leaves");
            return col.FindOne(l => l.EmployeeId == empId && l.LeaveAt.Date == date.Date);
        }

        public List<Leave> GetAll()
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Leave>("seaves");
            return col.FindAll().ToList();
        }

        public Leave? GetById(int id)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Leave>("leaves");
            return col.FindById(id);
        }

        public bool Update(Leave leave)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Leave>("leaves");
            return col.Update(leave);
        }

        public bool Delete(int id)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Leave>("leaves");
            return col.Delete(id);
        }
    }
}
