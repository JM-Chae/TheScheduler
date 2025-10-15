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
    public class CellValueToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3) return Brushes.Transparent;

            var dailyCellInfo = values[0] as TheScheduler.Models.DailyCellInfo;

            if (!int.TryParse(values[1]?.ToString(), out int columnIndex))
                return Brushes.White;
            if (!int.TryParse(values[2]?.ToString(), out int hoveredColumnIndex))
                return Brushes.White;

            if (columnIndex == 0) return Brushes.Transparent; // 0번 열 제외

            string displayKey = dailyCellInfo?.DisplayValue ?? "";

            // 이 열이 호버되었고 셀이 비어 있으면 호버 효과
            if (columnIndex == hoveredColumnIndex && string.IsNullOrEmpty(displayKey))
            {
                return new SolidColorBrush(Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF));
            }

            return displayKey switch
            {
                "A" => Brushes.Olive,
                "B" => Brushes.LightGreen,
                "C" => Brushes.LightCoral,
                "D" => Brushes.LightGoldenrodYellow,
                "E" => Brushes.LightPink,
                "F" => Brushes.LightSalmon,
                "G" => Brushes.LightSeaGreen,
                "H" => Brushes.LightSkyBlue,
                "I" => Brushes.Lime,
                "J" => Brushes.SlateBlue,
                "Y" => Brushes.Gray, // Deleted Shift
                "Z" => Brushes.White, // Leave
                _ => Brushes.Transparent
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }}
