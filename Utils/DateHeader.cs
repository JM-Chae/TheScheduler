using System;
using TheScheduler.Services;

namespace TheScheduler.Utils
{
    public class DateHeader
    {
        public int Day { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string DayOfWeekKorean => DayOfWeek switch
        {
            DayOfWeek.Monday => LocalizationService.Instance.GetString("DayOfWeek_Monday"),
            DayOfWeek.Tuesday => LocalizationService.Instance.GetString("DayOfWeek_Tuesday"),
            DayOfWeek.Wednesday => LocalizationService.Instance.GetString("DayOfWeek_Wednesday"),
            DayOfWeek.Thursday => LocalizationService.Instance.GetString("DayOfWeek_Thursday"),
            DayOfWeek.Friday => LocalizationService.Instance.GetString("DayOfWeek_Friday"),
            DayOfWeek.Saturday => LocalizationService.Instance.GetString("DayOfWeek_Saturday"),
            _ => LocalizationService.Instance.GetString("DayOfWeek_Sunday")
        };
    }
}
