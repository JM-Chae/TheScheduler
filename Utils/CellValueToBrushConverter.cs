using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace TheScheduler.Utils
{
    internal class CellValueToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return Brushes.Transparent;

            string cellText = values[0]?.ToString();
            if (!int.TryParse(values[1]?.ToString(), out int columnIndex))
                return Brushes.White;

            if (columnIndex == 0) return Brushes.Transparent; // 0번 열 제외

            return cellText switch
            {   
                "A" => Brushes.LightBlue,
                "B" => Brushes.LightGreen,
                "C" => Brushes.LightCoral,
                "D" => Brushes.LightGoldenrodYellow,
                "E" => Brushes.LightPink,
                "F" => Brushes.LightSalmon,
                "G" => Brushes.LightSeaGreen,
                "H" => Brushes.LightSkyBlue,
                "I" => Brushes.LightSteelBlue,
                "J" => Brushes.LightYellow,
                "Z" => Brushes.White, // Leave
                _ => Brushes.Transparent
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
