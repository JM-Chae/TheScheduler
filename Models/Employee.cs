
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

        // 자동 할당 조건을 위한 정보셋
        public required int JobProfileId { get; set; }
        // 개별 재정의 속성들
        public int? OverrideMaxMonthlyWorkTime { get; set; }
        public int? OverrideMinMonthlyWorkTime { get; set; }
        public int? OverrideMaxWeeklyWorkTime { get; set; }
        public int? OverrideMinWeeklyWorkTime { get; set; }
        public int? OverrideMaxWeeklyOffDay { get; set; }
        public int? OverrideMinWeeklyOffDay { get; set; }

        // 개인에 대한 고유 제약
        public List<DayOfWeek> ForbiddenDays { get; set; } = []; // 필수 제약
        public List<int> PairedEmployeeIds { get; set; } = []; // 준수 제약 : 세트 근무를 위한 파트너 직원 ID
        public List<int> PreferredShiftIds { get; set; } = [];
        public List<DayOfWeek> PreferredDays { get; set; } = [];
        public List<DayOfWeek> DesiredOffDays { get; set; } = [];

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
        public Dictionary<int, int> ShiftCounts { get; set; } = [];
    }

    public class EmployeeCorrectionSummary
    {
        public required int EmployeeId { get; set; }
        public required string EmployeeName { get; set; }
        public Dictionary<CorrectionType, int> CorrectionCounts { get; set; }
        public Dictionary<CorrectionType, int> CorrectionTotals { get; set; }

        public EmployeeCorrectionSummary()
        {
            CorrectionCounts = [];
            CorrectionTotals = [];
        }
    }
}
