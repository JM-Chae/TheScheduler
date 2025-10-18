using TheScheduler.Models;
using TheScheduler.Data;
using LiteDB;
using System.Collections.Generic;
using System.Linq;

namespace TheScheduler.Repositories
{
    public class ShiftRepo
    {
        public int GetNewId()
        {
            return IdGenerator.GetNextId("shifts");
        }

        public int Add(Shift shift)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Shift>("shifts");
            return col.Insert(shift);
        }

        public List<Shift> GetAll()
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Shift>("shifts");
            return col.FindAll().ToList();
        }

        public Shift? GetById(int id)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Shift>("shifts");
            return col.FindById(id);
        }

        public bool Update(Shift shift)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Shift>("shifts");
            return col.Update(shift);
        }

        public bool Delete(int id)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Shift>("shifts");
            return col.Delete(id);
        }

        public bool Upsert(Shift shift)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Shift>("shifts");
            return col.Upsert(shift);
        }
    }
}
