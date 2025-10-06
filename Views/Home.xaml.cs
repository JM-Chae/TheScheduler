using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace TheScheduler.Views
{
    public partial class Home : UserControl
    {
        private DateTime _displayDate;
        private DataGridCell? _hoveredCell;

        public Home()
        {
            InitializeComponent();
            LoadScheduleData(MyCalendar.DisplayDate);
        }

        private void LoadScheduleData(DateTime displayDate)
        {
            _displayDate = displayDate;
            DataTable dt = new();
            dt.Columns.Add("Name", typeof(string));

            int daysInMonth = DateTime.DaysInMonth(displayDate.Year, displayDate.Month);

            for (int day = 1; day <= daysInMonth; day++)
                dt.Columns.Add(day.ToString(), typeof(string));

            for (int i = 1; i <= 15; i++)
            {
                DataRow row = dt.NewRow();
                row["Name"] = $"Employee {i}";
                for (int day = 1; day <= daysInMonth; day++)
                    row[day.ToString()] = new[] { "A", "B", "C", "Off" }[(i + day) % 4];
                dt.Rows.Add(row);
            }

            MyDataGrid.ItemsSource = dt.DefaultView;
            MyDataGrid.MaxWidth = daysInMonth * 30 + 100 + 14;
        }

        private void MyCalendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
            => LoadScheduleData(MyCalendar.DisplayDate);

        private void MyDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (!int.TryParse(e.PropertyName, out int day)) return;

            for (int i = 0; i < MyDataGrid.Columns.Count; i++)
            {
                var col = MyDataGrid.Columns[i];

                if (int.TryParse(col.Header.ToString(), out int d))
                {
                    col.HeaderStyle = new Style(typeof(DataGridColumnHeader), MyDataGrid.ColumnHeaderStyle)
                    {
                        Setters = { new Setter(DataGridColumnHeader.BackgroundProperty, GetBaseColor(d)) }
                    };
                }
            }

            // Name의 셀은 고정
            if (e.PropertyName == "Name")
            {
                e.Column.Width = 100;
                e.Column.DisplayIndex = 0;
                return;
            }

            e.Column.Width = 30;
            var cellStyle = new Style(typeof(DataGridCell), MyDataGrid.CellStyle)
            {
                Setters = { new Setter(DataGridCell.BackgroundProperty, GetBaseColor(day)) }
            };
            e.Column.CellStyle = cellStyle;
        }

        private SolidColorBrush GetBaseColor(int day)
        {
            return new DateTime(_displayDate.Year, _displayDate.Month, day).DayOfWeek switch
            {
                DayOfWeek.Saturday => new(Color.FromArgb(0x3A, 0x41, 0x82, 0xFF)),
                DayOfWeek.Sunday => new(Color.FromArgb(0x2F, 0xFF, 0x72, 0x72)),
                _ => Brushes.Transparent
            };
        }

        private SolidColorBrush GetHoverColor(int day)
        {
            return new DateTime(_displayDate.Year, _displayDate.Month, day).DayOfWeek switch
            {
                DayOfWeek.Saturday => new(Color.FromArgb(0x4A, 0x41, 0x82, 0xFF)),
                DayOfWeek.Sunday => new(Color.FromArgb(0x4A, 0xFF, 0x72, 0x72)),
                _ => new(Color.FromArgb(0x1C, 0xFF, 0xFF, 0xFF))
            };
        }

        private void DataGridColumnHeader_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is not DataGridColumnHeader header || header.Column == null) return;

            if (!int.TryParse(header.Column.Header.ToString(), out int day)) return;

            foreach (var item in MyDataGrid.Items)
            {
                if (MyDataGrid.Columns[header.Column.DisplayIndex].GetCellContent(item)?.Parent is DataGridCell c)
                    c.Background = GetHoverColor(day);
            }

            // 헤더 색상도 변경
            header.Background = GetHoverColor(day);
        }

        private void DataGridColumnHeader_MouseLeave(object sender, MouseEventArgs e)
        {
            foreach (var col in MyDataGrid.Columns)
            {
                foreach (var item in MyDataGrid.Items)
                {
                    if (col.GetCellContent(item)?.Parent is DataGridCell c)
                        c.ClearValue(DataGridCell.BackgroundProperty);
                }
            }

            if (sender is DataGridColumnHeader header)
                header.ClearValue(DataGridColumnHeader.BackgroundProperty);
        }

        private void DataGridCell_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is not DataGridCell cell) return;

            _hoveredCell = cell;
            int colIndex = cell.Column.DisplayIndex;

            if (!int.TryParse(cell.Column.Header?.ToString(), out int day)) return;

            foreach (var item in MyDataGrid.Items)
            {
                if (MyDataGrid.Columns[colIndex].GetCellContent(item)?.Parent is DataGridCell c)
                    c.Background = GetHoverColor(day);
            }

            var headerStyle = new Style(typeof(DataGridColumnHeader), MyDataGrid.ColumnHeaderStyle)
            {
                Setters = { new Setter(DataGridColumnHeader.BackgroundProperty, GetHoverColor(day)) }
            };
            _hoveredCell.Column.HeaderStyle = headerStyle;
        }

        private void DataGridCell_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_hoveredCell == null) return;
            int colIndex = _hoveredCell.Column.DisplayIndex;

            foreach (var item in MyDataGrid.Items)
            {
                if (MyDataGrid.Columns[colIndex].GetCellContent(item)?.Parent is DataGridCell c)
                    c.ClearValue(DataGridCell.BackgroundProperty);
            }

            if (int.TryParse(_hoveredCell.Column.Header?.ToString(), out int day))
            {
                var hoverStyle = new Style(typeof(DataGridColumnHeader), _hoveredCell.Column.HeaderStyle)
                {
                    Setters = { new Setter(DataGridColumnHeader.BackgroundProperty, GetBaseColor(day))}
                };

                _hoveredCell.Column.HeaderStyle = hoverStyle;
            }
            _hoveredCell = null;
        }
    }
}
