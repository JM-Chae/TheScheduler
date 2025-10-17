using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using TheScheduler.Models; // CorrectionType enum이 있는 네임스페이스

namespace TheScheduler.Utils
{
    public class CorrectionValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value는 Dictionary, parameter는 조회할 CorrectionType enum 값
            if (value is Dictionary<CorrectionType, int> intDict && parameter is CorrectionType intKey)
            {
                return intDict.TryGetValue(intKey, out var result) ? result : 0;
            }
            
            if (value is Dictionary<CorrectionType, double> doubleDict && parameter is CorrectionType doubleKey)
            {
                return doubleDict.TryGetValue(doubleKey, out var result) ? result : 0.0;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
