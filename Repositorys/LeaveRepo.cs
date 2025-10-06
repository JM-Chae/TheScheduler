using TheScheduler.Models;
using TheScheduler.Data;
using LiteDB;

namespace TheScheduler.Repositories
{
    public class LeaveRepo
    {
        public int Add(Leave leave)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Leave>("leaves");
            return col.Insert(leave);
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
