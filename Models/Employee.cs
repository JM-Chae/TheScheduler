
namespace TheScheduler.Models
{
    public class Employee
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required Sex Sex { get; set; }
        public required Position Position { get; set; }
        public DateOnly? Bod { get; set; }
        public string? Phone { get; set; }
        public DateOnly? HireAt { get; set; }
        public string? Address { get; set; }
        public int? Category { get; set; }
        public string? Note { get; set; }
        public Leave[]? Leaves { get; set; }

    }

    public enum Sex
    {
        女性,
        男性
    }

    public enum Position
    {
        看護師,
        看護主任,
        看護副主任,
        看護助手,
        削除済み
    }

    public class EmployeeMonthlySummary
    {
        public required int EmployeeId { get; set; }
        public required string EmployeeName { get; set; }
        public required string TotalWorkHours { get; set; }
        public required int WorkDays { get; set; }
        public required int PaidLeaveDays { get; set; }
        public required int UnpaidLeaveDays { get; set; }
        public Dictionary<int, int> ShiftCounts { get; set; } = new();
    }
}
