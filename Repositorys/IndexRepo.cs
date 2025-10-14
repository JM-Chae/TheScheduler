using LiteDB;
using TheScheduler.Models;

namespace TheScheduler.Repositorys
{
    internal class IndexRepo
    {
        public void EnsureIndexes()
        {
            using var db = new LiteDatabase("Filename=Data.db;Connection=shared");

            var schedules = db.GetCollection<Schedule>("schedules");
            // For GetByEmployeeIdAndDate (����ID ����Ʈ �ε��� ���)
            schedules.EnsureIndex(x => x.EmployeeId);
            // For GetByMonth
            schedules.EnsureIndex(x => x.WorkDate);
            // ��¥ + ShiftId ���� �ε��� / ����ũ ����
            schedules.EnsureIndex(x => new { x.WorkDate, x.ShiftId }, unique: true);

            var leaves = db.GetCollection<Leave>("leaves");
            // For GetByEmployeeIdAndMonth and GetByEmployeeIdAndDate
            leaves.EnsureIndex(x => new { x.EmployeeId, x.LeaveAt });

            var corrections = db.GetCollection<Correction>("corrections");
            // For GetByEmployeeIdAndDate
            corrections.EnsureIndex(x => new { x.EmployeeId, x.When });
        }
    }
}