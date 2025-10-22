using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheScheduler.Data;
using TheScheduler.Models;

namespace TheScheduler.Repositories
{
    public class JobProfileRepo
    {

        public int GetNewId()
        {
            return IdGenerator.GetNextId("jobProfiles");
        }

        public int Add(JobProfile jobProfile)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<JobProfile>("jobProfiles");
            return col.Insert(jobProfile);
        }

        public List<JobProfile> GetAll()
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<JobProfile>("jobProfiles");
            return col.FindAll().ToList();
        }

        public JobProfile? GetById(int id)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<JobProfile>("jobProfiles");
            return col.FindById(id);
        }

        public bool Update(JobProfile jobProfile)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<JobProfile>("jobProfiles");
            return col.Update(jobProfile);
        }

        public bool Delete(int id)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<JobProfile>("jobProfiles");
            return col.Delete(id);
        }

        public bool Upsert(JobProfile jobProfile)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<JobProfile>("jobProfiles");
            return col.Upsert(jobProfile);
        }
    }
}
