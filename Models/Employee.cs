
namespace TheScheduler.Models
{
    public class Employee
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required Sex Sex { get; set; }
        public required string Position { get; set; }
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

    public class Position
    {
        public required int PositionId { get; set; }
        public required string Name { get; set; }
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

    public class EmployeeCorrectionSummary
    {
        public required int EmployeeId { get; set; }
        public required string EmployeeName { get; set; }
        public Dictionary<CorrectionType, int> CorrectionCounts { get; set; }
        public Dictionary<CorrectionType, int> CorrectionTotals { get; set; }

        public EmployeeCorrectionSummary()
        {
            CorrectionCounts = new Dictionary<CorrectionType, int>();
            CorrectionTotals = new Dictionary<CorrectionType, int>();
        }
    }
}
