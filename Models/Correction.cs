
namespace TheScheduler.Models
{
    public class Correction
    {
        public required int Id { get; set; }
        public required int ScheduleId { get; set; }
        public required CorrectionType Type { get; set; }
        public string? Note { get; set; }
    }

    public enum CorrectionType
    {
        Late,
        Overtime,
        EarlyLeave,
        ETC
    }
}
