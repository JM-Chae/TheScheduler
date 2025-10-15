
using LiteDB;
using System.Collections.ObjectModel;

namespace TheScheduler.Models
{
    public class Shift
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required List<ShiftCondition> Conditions { get; set; }
        public required TimeOnly Start { get; set; }
        public required TimeOnly End { get; set; }
        public required int RestInMinutes { get; set; }
        public required ShiftColor ShiftColor { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Shift shift &&
                   Id == shift.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }

    public class ShiftCondition
    {
        public required Guid Id { get; set; }
        public required Position Position { get; set; }
        public required int Value { get; set; }
    }

    public enum ShiftColor
    {
        A,  // LightBlue
        B,  // LightGreen
        C,  // LightCoral
        D,  // LightGoldenrodYellow
        E,  // LightPink
        F,  // LightSalmon
        G,  // LightSeaGreen
        H,  // LightSkyBlue
        I,  // LightSteelBlue
        J,  // LightYellow
        Y,  // For Deleted  
        // Z, For Leave. It is not saved to the DB. (White)
    }

    public class ShiftConditionVaildCount
    {
        public required Dictionary<Position, int> PositionCount { get; set; }
    }

    public class DayShiftWarning
    {
        public int Day { get; set; }
        public string ShiftName { get; set; } = "";
        public Position Position { get; set; }
        public int Remaining { get; set; }  // +면 부족, 0이면 충족, -면 초과
        public string Message => GenerateMessage();

        private string GenerateMessage()
        {
            if (Remaining > 0)
                return $"{ShiftName} ({Position}) に {Remaining} 名 足りません。";
            return "";
        }
    }
}
