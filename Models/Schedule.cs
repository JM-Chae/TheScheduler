
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace TheScheduler.Models
{
    public class Schedule
    {
        public required int Id { get; set; }
        public required List<int> EmployeeId { get; set; }
        public required int ShiftId { get; set; }
        public required DateTime WorkDate { get; set; }   
    }
    public class DailyCellInfo
    {
        public Shift? Shift { get; set; }
        public Leave? Leave { get; set; }
        public Correction? Correction { get; set; }
        public string DisplayValue => Shift?.ShiftColor.ToString() ?? (Leave != null ? "Z" : "");

        public SolidColorBrush? CorrectionIndicatorBrush
        {
            get
            {
                if (Correction == null) return null;
                return Correction.CorrectMin > 0
                    ? new SolidColorBrush(Color.FromArgb(0xFF, 0x05, 0xDA, 0xC5)) // #05dac5 for positive
                    : new SolidColorBrush(Color.FromArgb(0xFF, 0xF3, 0x58, 0x4D)); // #f3584d for negative
            }
        }
    }
}
