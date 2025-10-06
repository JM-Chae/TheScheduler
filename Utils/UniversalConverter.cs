using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TheScheduler.Utils
{
    public class UniversalConverter : IValueConverter
    {
        public bool Inverse { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible;

            if (value is bool boolValue)
            {
                isVisible = boolValue;
            }
            else
            {
                // object null 체크
                isVisible = value != null;
            }

            if (Inverse)
                isVisible = !isVisible;

            if (targetType == typeof(Visibility))
                return isVisible ? Visibility.Visible : Visibility.Collapsed;

            return isVisible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
            {
                bool result = v == Visibility.Visible;
                return Inverse ? !result : result;
            }

            if (value is bool b)
                return Inverse ? !b : b;

            throw new NotImplementedException();
        }
    }
}