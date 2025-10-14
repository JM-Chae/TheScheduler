using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TheScheduler.Models;
using TheScheduler.Utils;
using TheScheduler.ViewModels;

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
            if (this.DataContext is HomeViewModel vm)
            {
                vm.OnScheduleUpdated = () => LoadScheduleData(MyCalendar.DisplayDate);
            }
            LoadScheduleData(MyCalendar.DisplayDate);
        }

        private void LoadScheduleData(DateTime displayDate)
        {
            _displayDate = displayDate;
            int daysInMonth = DateTime.DaysInMonth(displayDate.Year, displayDate.Month);

            DataTable dt = new();
            dt.Columns.Add("Name", typeof(string));

            for (int day = 1; day <= daysInMonth; day++)
            {
                dt.Columns.Add(day.ToString(), typeof(string));
            }

            dt.Columns.Add("ID", typeof(string));

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

            MyDataGrid.ItemsSource = dt.DefaultView;
        }

        private void MyCalendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
            => LoadScheduleData(MyCalendar.DisplayDate);

        private void MyDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                e.Column.Header = "名前";
                e.Column.Width = 100;
                e.Column.DisplayIndex = 0;
                e.Column.HeaderTemplate = (DataTemplate)this.FindResource("StringHeaderTemplate");
                e.Column.HeaderStyle = (Style)this.FindResource("WeekdayHeaderStyle");
                return;
            }
            
            if (e.PropertyName == "ID")
            {
                e.Column.Visibility = Visibility.Collapsed;
                return;
            }

            if (int.TryParse(e.PropertyName, out int day))
            {
                var dateHeader = new DateHeader
                {
                    Day = day,
                    DayOfWeek = new DateTime(_displayDate.Year, _displayDate.Month, day).DayOfWeek
                };
                e.Column.Header = dateHeader;
                e.Column.Width = 30;
                e.Column.HeaderTemplate = (DataTemplate)this.FindResource("DateHeaderTemplate");

                switch (dateHeader.DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                        e.Column.HeaderStyle = (Style)this.FindResource("SaturdayHeaderStyle");
                        break;
                    case DayOfWeek.Sunday:
                        e.Column.HeaderStyle = (Style)this.FindResource("SundayHeaderStyle");
                        break;
                    default:
                        e.Column.HeaderStyle = (Style)this.FindResource("WeekdayHeaderStyle");
                        break;
                }
            }
            
            MyDataGrid.FrozenColumnCount = 1;
        }

        private SolidColorBrush GetHoverColor(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Saturday => new(Color.FromArgb(0x4A, 0x41, 0x82, 0xFF)),
                DayOfWeek.Sunday => new(Color.FromArgb(0x4A, 0xFF, 0x72, 0x72)),
                _ => new(Color.FromArgb(0x1C, 0xFF, 0xFF, 0xFF))
            };
        }

        private void DataGridCell_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is not DataGridCell cell) return;
            if (cell.Column.Header is not DateHeader dateHeader) return;

            _hoveredCell = cell;
            int colIndex = cell.Column.DisplayIndex;

            foreach (var item in MyDataGrid.Items)
            {
                if (MyDataGrid.Columns[colIndex].GetCellContent(item)?.Parent is DataGridCell c)
                {
                    if (item is DataRowView rowView)
                    {
                        object cellData = rowView[cell.Column.SortMemberPath];
                        if (cellData == null || string.IsNullOrEmpty(cellData.ToString()))
                        {
                            c.Background = GetHoverColor(dateHeader.DayOfWeek);
                        }
                    }
                }
            }
        }

        private void DataGridCell_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_hoveredCell == null) return;
            int colIndex = _hoveredCell.Column.DisplayIndex;

            foreach (var item in MyDataGrid.Items)
            {
                if (MyDataGrid.Columns[colIndex].GetCellContent(item)?.Parent is DataGridCell c)
                {
                    c.ClearValue(DataGridCell.BackgroundProperty);
                }
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
                var idColumnVal = ((dataItem is DataRowView drv) ? drv["ID"] : 0).ToString();
                
                if (cell.Column.Header is not DateHeader dateHeader) return;
                int clickedDay = dateHeader.Day;

                if (!int.TryParse(idColumnVal, out int employeeId)) return;

                DateTime date = new DateTime(_displayDate.Year, _displayDate.Month, clickedDay);
                var vm = FindResource("vm") as HomeViewModel;
                vm?.ScheduleEditDialogOpen(employeeId, date);
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