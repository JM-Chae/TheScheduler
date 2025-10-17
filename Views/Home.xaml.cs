using Nager.Date.Model;
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

        private Dictionary<Employee, List<DailyCellInfo>>? _allSchedules = new();
        private HomeViewModel VM => DataContext as HomeViewModel;

        private IEnumerable<PublicHoliday> _holidays;

        private bool isSyncing = false;

        public Home()
        {
            InitializeComponent();
            this.Loaded += Home_Loaded;
            this.Unloaded += Home_Unloaded;
        }

        private void Home_Loaded(object sender, RoutedEventArgs e)
        {
            if (VM != null)
            {
                VM.OnScheduleUpdated = () => LoadScheduleData(MyCalendar.DisplayDate);
                VM.RequestPrint = PrintMyDataGrid; // Subscribe to the ViewModel's print event
            }

            if (Application.Current.MainWindow.DataContext is MainViewModel mainVM)
            {
                mainVM.RefreshHomeView = RefreshData;
            }

            LoadScheduleData(MyCalendar.DisplayDate);
        }

        private void Home_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow.DataContext is MainViewModel mainVM)
            {
                mainVM.RefreshHomeView -= RefreshData;
            }

            if (VM != null)
            {
                VM.RequestPrint -= PrintMyDataGrid; // Unsubscribe from the ViewModel's print event
            }
        }

        private void OnDataGridScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (isSyncing || e.VerticalChange == 0) return;

            isSyncing = true;

            var sourceScroll = FindScrollViewer(sender as DependencyObject);
            if (sourceScroll == null)
            {
                isSyncing = false;
                return;
            }

            // 다른 DataGrid 동기화
            if (sender == MyDataGrid)
            {
                var targetScroll = FindScrollViewer(CorrectionSummaries);
                targetScroll?.ScrollToVerticalOffset(sourceScroll.VerticalOffset);
            }
            else if (sender == CorrectionSummaries)
            {
                var targetScroll = FindScrollViewer(MyDataGrid);
                targetScroll?.ScrollToVerticalOffset(sourceScroll.VerticalOffset);
            }

            isSyncing = false;
        }
        private static ScrollViewer? FindScrollViewer(DependencyObject parent)
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is ScrollViewer scrollViewer)
                    return scrollViewer;

                var result = FindScrollViewer(child);
                if (result != null)
                    return result;
            }

            return null;
        }

        private void RefreshData()
        {
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
                dt.Columns.Add(day.ToString(), typeof(DailyCellInfo));
            }

            dt.Columns.Add("ID", typeof(string));

            _allSchedules = VM?.GetAllSchedulesByThisMonth(displayDate);

            var sortedSchedules = _allSchedules?.OrderBy(s => s.Key.Id);

            if (sortedSchedules == null) return;
            foreach (var (employee, schedules) in sortedSchedules)
            {
                DataRow row = dt.NewRow();
                row["Name"] = employee.Name;
                row["ID"] = employee.Id;

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var dailyCellInfo = schedules[day - 1];
                    row[day.ToString()] = dailyCellInfo ?? new DailyCellInfo();
                }
                dt.Rows.Add(row);
            }

            MyDataGrid.ItemsSource = dt.DefaultView;
            UpdateSummaryGridColumns();
        }

        private void UpdateSummaryGridColumns()
        {
            var headerTemplate = this.FindResource("ShiftCountHeaderTemplate") as DataTemplate;

            // Remove dynamically added columns before adding new ones
            for (int i = SummaryDataGrid.Columns.Count - 1; i >= 5; i--)
            {
                SummaryDataGrid.Columns.RemoveAt(i);
            }

            int displayIndex = 5; // Start DisplayIndex after the 5 static columns
            foreach (var shift in VM.SummaryDisplayShifts)
            {
                var column = new DataGridTextColumn
                {
                    Header = new ContentControl
                    {
                        Content = shift,
                        ContentTemplate = headerTemplate,
                        Margin = new Thickness(5,0,5,0)
                    },
                    Binding = new System.Windows.Data.Binding($"ShiftCounts[{shift.Id}]"), // Bind Shift.Id
                    MinWidth = 100,
                    DisplayIndex = displayIndex++,
                };
                SummaryDataGrid.Columns.Add(column);
            }
        }

        private void MyCalendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
            => LoadScheduleData(MyCalendar.DisplayDate);

        private void MyDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                var templateColumn = new DataGridTemplateColumn();
                templateColumn.Header = "名前";
                templateColumn.Width = 150;
                templateColumn.DisplayIndex = 0;
                templateColumn.HeaderTemplate = (DataTemplate)this.FindResource("StringHeaderTemplate");
                templateColumn.HeaderStyle = (Style)this.FindResource("WeekdayHeaderStyle");

                string cellTemplateString = 
                    $@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                        <TextBlock Text=""{{Binding Name}}"" HorizontalAlignment=""Center"" VerticalAlignment=""Center"" Foreground=""White"" />
                    </DataTemplate>";

                templateColumn.CellTemplate = (DataTemplate)System.Windows.Markup.XamlReader.Parse(cellTemplateString);
                e.Column = templateColumn;
                return;
            }
            
            if (e.PropertyName == "ID")
            {
                e.Column.Visibility = Visibility.Collapsed;
                return;
            }

            _holidays = VM.PublicHolidays;

            if (int.TryParse(e.PropertyName, out int day))
            {
                var templateColumn = new DataGridTemplateColumn();

                var currentDate = new DateTime(_displayDate.Year, _displayDate.Month, day);
                var dateHeader = new DateHeader
                {
                    Day = day,
                    DayOfWeek = currentDate.DayOfWeek
                };
                templateColumn.Header = dateHeader;
                templateColumn.Width = 30;
                templateColumn.HeaderTemplate = (DataTemplate)this.FindResource("DateHeaderTemplate");

                bool isHoliday = _holidays?.Any(h => h.Date.Date == currentDate.Date) ?? false;

                if (isHoliday)
                {
                    templateColumn.HeaderStyle = (Style)this.FindResource("SundayHeaderStyle");
                }
                else
                {
                    switch (dateHeader.DayOfWeek)
                    {
                        case DayOfWeek.Saturday:
                            templateColumn.HeaderStyle = (Style)this.FindResource("SaturdayHeaderStyle");
                            break;
                        case DayOfWeek.Sunday:
                            templateColumn.HeaderStyle = (Style)this.FindResource("SundayHeaderStyle");
                            break;
                        default:
                            templateColumn.HeaderStyle = (Style)this.FindResource("WeekdayHeaderStyle");
                            break;
                    }
                }

                string cellTemplateString = 
                    $@"<DataTemplate 
                        xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                        xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                        xmlns:utils=""clr-namespace:TheScheduler.Utils;assembly=TheScheduler"">
                        <DataTemplate.Resources>
                            <utils:CellValueToBrushConverter x:Key=""CellValueToBrushConverter"" />
                        </DataTemplate.Resources>
                        <Grid HorizontalAlignment=""Stretch"" VerticalAlignment=""Stretch"">
                            <Grid.Style>
                                <Style TargetType=""Grid"">
                                    <Setter Property=""Background"">
                                        <Setter.Value>
                                            <MultiBinding Converter=""{{StaticResource CellValueToBrushConverter}}"">
                                                <Binding Path=""[{e.PropertyName}]"" />
                                                <Binding Path=""Column.DisplayIndex"" RelativeSource=""{{RelativeSource AncestorType=DataGridCell}}"" />
                                                <Binding Path=""DataContext.HoveredColumnIndex"" RelativeSource=""{{RelativeSource AncestorType=DataGrid}}"" />
                                            </MultiBinding>
                                        </Setter.Value>
                                    </Setter>
                                    <Style.Triggers>
                                        <DataTrigger Binding=""{{Binding RelativeSource={{RelativeSource AncestorType=DataGridCell}}, Path=IsSelected}}"" Value=""True"">
                                            <Setter Property=""Background"" Value=""#8794D692"" />
                                        </DataTrigger>
                                    </Style.Triggers>
                                </Style>
                            </Grid.Style>
                            <Border BorderThickness=""3,3,3,3"" HorizontalAlignment=""Stretch"" VerticalAlignment=""Stretch"" BorderBrush=""{{Binding Path=[{e.PropertyName}].CorrectionIndicatorBrush}}""/>
                            <TextBlock Text=""{{Binding Path=[{e.PropertyName}].Shift.Name}}"" HorizontalAlignment=""Center"" VerticalAlignment=""Center"" FontSize=""14"" />
                        </Grid>
                    </DataTemplate>";

                templateColumn.CellTemplate = (DataTemplate)System.Windows.Markup.XamlReader.Parse(cellTemplateString);
                e.Column = templateColumn;
            }

            MyDataGrid.FrozenColumnCount = 1;
        }

        private void DataGridCell_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is not DataGridCell cell) return;
            if (cell.Column.Header is not DateHeader dateHeader) return;

            VM.HoveredColumnIndex = cell.Column.DisplayIndex;
        }

        private void DataGridCell_MouseLeave(object sender, MouseEventArgs e)
        {
            VM.HoveredColumnIndex = -1; // Reset to -1 when mouse leaves
        }

        public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is T tParent) return tParent;
            return FindParent<T>(parent);
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

        private void DataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject dep)
            {
                var row = FindParent<DataGridRow>(dep);
                if (row != null) return;
            }

            CorrectionSummaries.UnselectAllCells();
            SummaryDataGrid.UnselectAllCells();
            MyDataGrid.UnselectAllCells();
            WarnGrid.UnselectAllCells();
        }

        public void PrintMyDataGrid()
        {
            PrintManager.PrintDataGrid(MyDataGrid, _holidays, _displayDate);
        }
    }
}