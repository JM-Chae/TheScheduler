using System;
using System.Globalization;
using System.Windows.Data;
using TheScheduler.Services;

namespace TheScheduler.Utils
{
    public class EnumToLocalizedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            string key = $"{value.GetType().Name}_{value}";
            return LocalizationService.Instance.GetString(key);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
