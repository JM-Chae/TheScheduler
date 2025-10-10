using TheScheduler.Models;
using LiteDB;
using TheScheduler.Data;

namespace TheScheduler.Repositories
{
    public class EmployeeRepo
    {
        public int GetNewId() 
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Employee>("employees");

            var last = col.Query()
                            .OrderByDescending(x => x.Id)
                            .Limit(1)
                            .FirstOrDefault();

            return (last?.Id ?? 0) + 1;
        }

        public int Add(Employee employee)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Employee>("employees");
            return col.Insert(employee);
        }

        public List<Employee> GetAll()
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Employee>("employees");
            return col.FindAll().ToList();
        }

        public Employee? GetById(int id)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Employee>("employees");
            return col.FindById(id);
        }

        public bool Update(Employee employee)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Employee>("employees");
            return col.Update(employee);
        }

        public bool Delete(int id)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Employee>("employees");
            return col.Delete(id);
        }

        public bool Upsert(Employee employee)
        {
            using var db = LiteDBService.GetDatabase();
            var col = db.GetCollection<Employee>("employees");
            return col.Upsert(employee);
        }
    }
}
