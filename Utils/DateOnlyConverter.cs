
using System.Globalization;
using System.Windows.Data;
using System;

namespace TheScheduler.Utils
{
    public class DateTimeConverter : IValueConverter
    {
        // ViewModel -> View
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateOnly d)
                return new DateTime(d.Year, d.Month, d.Day);
            if (value is TimeOnly t)    
                return DateTime.Today.Add(t.ToTimeSpan());

            return null;
        }

        // View -> ViewModel
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
            {
                if (targetType == typeof(DateOnly))
                    return DateOnly.FromDateTime(dt);
                if (targetType == typeof(TimeOnly))
                    return TimeOnly.FromDateTime(dt);
            }
            return null;
        }
    }
}
