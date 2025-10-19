
using System.Globalization;
using System.Windows.Data;
using System;

namespace TheScheduler.Utils
{
    public class DateOnlyConverter : IValueConverter
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
                if (targetType == typeof(DateOnly) || targetType == typeof(DateOnly?))
                    return DateOnly.FromDateTime(dt);
                if (targetType == typeof(TimeOnly) || targetType == typeof(TimeOnly?))
                    return TimeOnly.FromDateTime(dt);
            }
            // If value is null, and targetType is nullable DateOnly or TimeOnly, return null
            if (value == null && (targetType == typeof(DateOnly?) || targetType == typeof(TimeOnly?)))
            {
                return null;
            }
            // For other cases, let the binding engine handle it, or return UnsetValue if conversion is truly impossible
            return System.Windows.DependencyProperty.UnsetValue;
        }
    }
}
