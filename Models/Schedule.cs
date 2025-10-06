
namespace TheScheduler.Models
{
    public class Schedule
    {
        public required int Id { get; set; }
        public required int EmployeeId { get; set; }
        public required int ShiftId { get; set; }
        public required DateTime WorkDate { get; set; }
    }
}
