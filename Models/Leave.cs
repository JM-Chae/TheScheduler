

namespace TheScheduler.Models
{
    public class Leave
    {
        public required int Id { get; set; }
        public required int EmployeeId { get; set; }
        public DateTime LeaveAt { get; set; }
        public bool IsPaid { get; set; }
        public string? Why { get; set; }
    }
}
