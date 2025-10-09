using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheScheduler.Models;

namespace TheScheduler.Repositorys
{
    internal class IndexRepo
    {
        public void EnsureIndexes()
        {
            using var db = new LiteDatabase("Filename=Data.db;Connection=shared");

            var col = db.GetCollection<Schedule>("schedules");

            col.EnsureIndex(x => new { x.WorkDate, x.ShiftId }, unique: true);
        }
    }
}
