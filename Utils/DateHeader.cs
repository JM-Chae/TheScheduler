using System;

namespace TheScheduler.Utils
{
    public class DateHeader
    {
        public int Day { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string DayOfWeekKorean => DayOfWeek switch
        {
            DayOfWeek.Monday => "月",
            DayOfWeek.Tuesday => "火",
            DayOfWeek.Wednesday => "水",
            DayOfWeek.Thursday => "木",
            DayOfWeek.Friday => "金",
            DayOfWeek.Saturday => "土",
            _ => "日"
        };
    }
}
