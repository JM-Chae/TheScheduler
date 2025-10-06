
namespace TheScheduler.Models
{
    public class Shift
    {
        public int Id { get; set; }
        public required ShiftCondition[] Conditions { get; set; }
        public required TimeOnly Start { get; set; }
        public required TimeOnly End { get; set; }
        public required TimeOnly Rest { get; set; }
    }

    public class ShiftCondition
    {
        public required Position Position { get; set; }
        public required int Value { get; set; }
    }
}
