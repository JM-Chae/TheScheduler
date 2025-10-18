using TheScheduler.Models;
using TheScheduler.Data;
using LiteDB;
using System.Collections.Generic;
using System.Linq;

namespace TheScheduler.Repositories
{
    public class PositionRepo
    {
        public int GetNewId()
        {
            return IdGenerator.GetNextId("positions");
        }

        public int Add(Position position)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Position>("positions");
            return col.Insert(position);
        }

        public List<Position> GetAll()
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Position>("positions");
            return col.FindAll().ToList();
        }

        public Position? GetById(int id)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Position>("positions");
            return col.FindById(id);
        }

        public bool Update(Position position)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Position>("positions");
            return col.Update(position);
        }

        public bool Delete(int id)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Position>("positions");
            return col.Delete(id);
        }

        public bool Upsert(Position position)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Position>("positions");
            return col.Upsert(position);
        }
    }
}