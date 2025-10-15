using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TheScheduler.Models;

namespace TheScheduler.Utils
{
    public class EnumToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ShiftColor shiftColor)
            {
                return shiftColor switch
                {
                    ShiftColor.A => Brushes.Olive,
                    ShiftColor.B => Brushes.LightGreen,
                    ShiftColor.C => Brushes.LightCoral,
                    ShiftColor.D => Brushes.LightGoldenrodYellow,
                    ShiftColor.E => Brushes.LightPink,
                    ShiftColor.F => Brushes.LightSalmon,
                    ShiftColor.G => Brushes.LightSeaGreen,
                    ShiftColor.H => Brushes.LightSkyBlue,
                    ShiftColor.I => Brushes.Lime,
                    ShiftColor.J => Brushes.SlateBlue,
                    _ => Brushes.Gray
                };
            }

            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

