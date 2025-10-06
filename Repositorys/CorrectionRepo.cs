using TheScheduler.Models;
using LiteDB;
using TheScheduler.Data;

namespace TheScheduler.Repositories
{
    public class CorrectionRepo
    {
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
