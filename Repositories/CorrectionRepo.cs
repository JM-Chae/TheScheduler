using TheScheduler.Models;
using LiteDB;
using TheScheduler.Data;
using System.Reflection;

namespace TheScheduler.Repositories
{
    public class CorrectionRepo
    {
        public int GetNewId()
        {
            return IdGenerator.GetNextId("corrections");
        }

        public List<Correction> GetByEmployeeIdAndMonth(int empId, int year, int month)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Correction>("corrections");
            var startDate = new DateTime(year, month, 1).Date;
            var endDate = startDate.AddMonths(1).AddDays(-1).Date;
            return col.Find(l => l.EmployeeId == empId && l.When.Date >=
                                    startDate.Date && l.When.Date <=
                                    endDate.Date).ToList();
        }

        public Correction GetByEmployeeIdAndDate(int empId, DateTime date)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Correction>("corrections");
            return col.FindOne(c => c.EmployeeId == empId && c.When.Date == date.Date);
        }

        public int Add(Correction correction)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Correction>("corrections");
            return col.Insert(correction);
        }

        public List<Correction> GetAll()
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Correction>("corrections");
            return col.FindAll().ToList();
        }

        public Correction? GetById(int id)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Correction>("corrections");
            return col.FindById(id);
        }

        public bool Update(Correction correction)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Correction>("corrections");
            return col.Update(correction);
        }

        public bool Delete(int id)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Correction>("corrections");
            return col.Delete(id);
        }
    }
}
