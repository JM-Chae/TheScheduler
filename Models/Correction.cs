
namespace TheScheduler.Models
{
    public class Correction
    {
        public required int Id { get; set; }
        public required int EmployeeId { get; set; }
        public required CorrectionType Type { get; set; }
        public required DateTime When { get; set; }
        public required TimeOnly CorrectTime { get; set; }
        public required bool IsPaid { get; set; }
        public string? Note { get; set; }
    }

    public enum CorrectionType
    {
        遅刻,
        超過勤務,
        早退,
        その他
    }
}
