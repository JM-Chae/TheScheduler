using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheScheduler.Data
{
    public static class IdGenerator
    {
        public static int GetNextId(string collectionName)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Counter>("counters");

            var counter = col.FindOne(x => x.Name == collectionName);
            if (counter == null)
            {
                counter = new Counter { Name = collectionName, Value = 1 };
                col.Insert(counter);
            }
            else
            {
                counter.Value++;
                col.Update(counter);
            }

            return counter.Value;
        }

        public class Counter
        {
            public ObjectId Id { get; set; }  // LiteDB 기본 _id
            public string Name { get; set; }  // 예: "corrections"
            public int Value { get; set; }
        }
    }
}
