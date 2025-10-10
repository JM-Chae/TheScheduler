using Syncfusion.Windows.Controls.Primitives;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TheScheduler.Models;

namespace TheScheduler.Views
{
    public partial class Home : UserControl
    {
        private DateTime _displayDate;
        private DataGridCell? _hoveredCell;
        private Dictionary<Employee, List<object?>>? _allSchedules = new();

        public Home()
        {
            InitializeComponent();
            LoadScheduleData(MyCalendar.DisplayDate);
        }

        private void LoadScheduleData(DateTime displayDate)
        {
            _displayDate = displayDate;
            int daysInMonth = DateTime.DaysInMonth(displayDate.Year, displayDate.Month);

            DataTable dt = new();
            dt.Columns.Add("Name", typeof(string));

            DataTable dayOfWeekTable = new();
            dayOfWeekTable.Columns.Add("Name", typeof(string));

            for (int day = 1; day <= daysInMonth; day++)
            {
                dt.Columns.Add(day.ToString(), typeof(string));
                dayOfWeekTable.Columns.Add(day.ToString(), typeof(string));
            }

            dt.Columns.Add("ID", typeof(string));

            // 해당 월의 모든 직원들의 스케줄을 딕셔너리 형태로 가져옴. 직원 : 시프트 or 휴가 or null
            _allSchedules = (this.DataContext as ViewModels.HomeViewModel)?.GetAllSchedulesByThisMonth(displayDate);

            if (_allSchedules == null) return;
            foreach (var (employee, schedules) in _allSchedules)
            {

                DataRow row = dt.NewRow();
                row["Name"] = employee.Name;
                row["ID"] = employee.Id;

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var objectType = schedules[day - 1]?.GetType();
                    var shift = objectType == typeof(Shift) ? (Shift?)schedules[day - 1] : null;
                    var leave = objectType == typeof(Leave) ? (Leave?)schedules[day - 1] : null;
                    
                    if (shift != null) row[day.ToString()] = shift.ShiftColor;
                    else if (leave != null) row[day.ToString()] = "Z";
                    else row[day.ToString()] = "";

                }
                dt.Rows.Add(row);
            }

            DataRow dow = dayOfWeekTable.NewRow();
            dow["Name"] = "";
            for (int day = 1; day <= daysInMonth; day++) 
                dow[day.ToString()] = new DateTime(_displayDate.Year, _displayDate.Month, day).DayOfWeek switch
                {                                        
                    DayOfWeek.Monday => "月",
                    DayOfWeek.Tuesday => "火",
                    DayOfWeek.Wednesday => "水",
                    DayOfWeek.Thursday => "木",
                    DayOfWeek.Friday => "金",
                    DayOfWeek.Saturday => "土",
                        _ => "日"
                };

            dayOfWeekTable.Rows.Add(dow);

            MyDataGrid.ItemsSource = dt.DefaultView;
            MyDataGrid.MaxWidth = daysInMonth * 30 + 100 + 10;
            MyDOWGrid.ItemsSource = dayOfWeekTable.DefaultView;
            MyDOWGrid.MaxWidth = daysInMonth * 30 + 100 - 6;
        }

        private void MyCalendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
            => LoadScheduleData(MyCalendar.DisplayDate);

        private void DOWGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                e.Column.CellStyle = new Style(typeof(DataGridCell), MyDOWGrid.CellStyle)
                {
                    Setters =
                     {
                        new Setter(DataGridCell.BackgroundProperty, new SolidColorBrush(Color.FromArgb(0xFE, 0x53, 0x8D, 0x18)))
                     }
                };
                e.Column.MinWidth = 100;
                e.Column.DisplayIndex = 0;
                return;
            }

            if (int.TryParse(e.PropertyName, out int day))
            {
                e.Column.Width = 30;

                e.Column.CellStyle = new Style(typeof(DataGridCell), MyDOWGrid.CellStyle)
                {
                    Setters =
            {
                new Setter(DataGridCell.BackgroundProperty, GetBaseColor(day) == Brushes.Transparent ? new(Color.FromArgb(0xFE, 0x53, 0x8D, 0x18)) : GetBaseColor(day) )
            }
                };
            }

            MyDOWGrid.FrozenColumnCount = 1;
        }

        private void MyDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                e.Column.Width = 100;
                e.Column.DisplayIndex = 0;
                return;
            }
            else if (e.PropertyName == "ID") e.Column.Visibility = Visibility.Collapsed;

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
            e.Column.Width = 30;

            MyDataGrid.FrozenColumnCount = 1;
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

        private void MyDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject dep)
            {
                var cell = FindParent<DataGridCell>(dep);
                if (cell == null) return;

                var row = DataGridRow.GetRowContainingElement(cell);
                if (row == null) return;

                var dataItem = row.Item;
                var columnHeader = cell.Column.Header;      // 클릭한 열 헤더

                if (dataItem is DataRowView drv)
                {
                    var employeeId = drv["ID"];
                }


            }
        }
        public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is T tParent) return tParent;
            return FindParent<T>(parent);
        }
    }
}
