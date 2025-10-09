
using LiteDB;
using System.Collections.ObjectModel;

namespace TheScheduler.Models
{
    public class Shift
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required ObservableCollection<ShiftCondition> Conditions { get; set; }
        public required TimeOnly Start { get; set; }
        public required TimeOnly End { get; set; }
        public required TimeOnly Rest { get; set; }
        public required ShiftColor ShiftColor { get; set; }

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
        J   // LightYellow
    }
}
