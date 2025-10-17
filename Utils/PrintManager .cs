using Nager.Date.Model;
using System.Data;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using TheScheduler.Models;
using TheScheduler.Utils;

public static class PrintManager
{
    private static Brush GetHeaderBackgroundBrush(object header)
    {
        Color weekdayColor = Color.FromArgb(0xFF, 0x53, 0x8D, 0x18);
        Color saturdayColor = Color.FromArgb(0xFF, 0x07, 0x3E, 0xA7);
        Color sundayColor = Color.FromArgb(0xFF, 0x99, 0x05, 0x05);

        if (header is DateHeader dateHeader)
        {
            switch (dateHeader.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                    return new SolidColorBrush(saturdayColor);
                case DayOfWeek.Sunday:
                    return new SolidColorBrush(sundayColor);
                default:
                    return new SolidColorBrush(weekdayColor);
            }
        }
        else if (header.ToString() == "名前")
        {
            return new SolidColorBrush(weekdayColor);
        }
        return Brushes.LightGray;
    }

    private static Brush CalculateCellBackground(DailyCellInfo dailyCellInfo)
    {
        if (dailyCellInfo == null) return Brushes.Transparent;
        string displayKey = dailyCellInfo.DisplayValue ?? "";

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
            "Y" => Brushes.Gray,
            "Z" => Brushes.Black,
            _ => Brushes.Transparent
        };
    }

    public static void PrintDataGrid(DataGrid dataGrid, IEnumerable<PublicHoliday> _holidays, DateTime _displayDate)
    {
        DataView dataView = (dataGrid.ItemsSource as DataView);
        if (dataView == null) return;
        DataTable dataTable = dataView.Table;

        FlowDocument doc = new FlowDocument();
        doc.FontFamily = new FontFamily("Meiryo");
        double paddingValue = 45;
        doc.PagePadding = new Thickness(paddingValue);

        Table table = new Table();

        table.Background = Brushes.White;
        doc.Blocks.Add(table);
        table.CellSpacing = 0;
        table.BorderBrush = Brushes.Black;
        table.BorderThickness = new Thickness(1);
        table.TextAlignment = TextAlignment.Center;

        double totalContentWidth = 0;
        List<double> originalColumnWidths = new List<double>();

        TableRowGroup headerGroup = new TableRowGroup();
        table.RowGroups.Add(headerGroup);
        TableRow headerRow = new TableRow();
        headerGroup.Rows.Add(headerRow);

        List<TableCell> headerTemplateCells = new List<TableCell>();

        foreach (var column in dataGrid.Columns.OfType<DataGridColumn>().Where(c => c.Visibility == Visibility.Visible))
        {
            double colWidth = column.ActualWidth > 0 ? column.ActualWidth : 100;

            table.Columns.Add(new TableColumn() { Width = new GridLength(colWidth) });

            totalContentWidth += colWidth;
            originalColumnWidths.Add(colWidth);

            string headerText = column.Header is DateHeader dh ? $"{dh.Day}\n{dh.DayOfWeekKorean}" : column.Header.ToString();
            if (column.Header is DateHeader dhForHolidayCheck)
            {
                var currentDate = new DateTime(_displayDate.Year, _displayDate.Month, dhForHolidayCheck.Day);
                bool isHoliday = _holidays?.Any(h => h.Date.Date == currentDate.Date) ?? false;

                if (isHoliday)
                {
                    dhForHolidayCheck.DayOfWeek = DayOfWeek.Sunday;
                }
            }
       
            Brush backgroundBrush = GetHeaderBackgroundBrush(column.Header);

            TableCell headerCell = new TableCell(new Paragraph(new Run(headerText)));
            headerCell.Background = backgroundBrush;
            headerCell.Foreground = Brushes.White;
            headerCell.BorderBrush = Brushes.Black;
            headerCell.BorderThickness = new Thickness(1);
            headerCell.FontWeight = FontWeights.Bold;
            headerCell.TextAlignment = TextAlignment.Center;
            headerCell.Padding = new Thickness(0, 10, 0, 10);
            headerRow.Cells.Add(headerCell);

            headerTemplateCells.Add(headerCell);
        }

        TableRowGroup bodyGroup = new TableRowGroup();
        table.RowGroups.Add(bodyGroup);

        PrintDialog printDlg = new PrintDialog();

        PrintTicket printTicket = printDlg.PrintTicket;
        if (printTicket.PageOrientation != PageOrientation.Landscape)
        {
            printTicket.PageOrientation = PageOrientation.Landscape;
        }

        if (printDlg.ShowDialog() == true)
        {
            // 인쇄 가능 영역 계산
            double printableWidth = printDlg.PrintableAreaWidth;
            double printableHeight = printDlg.PrintableAreaHeight;

            // 콘텐츠 영역 계산
            double horizontalPadding = doc.PagePadding.Left + doc.PagePadding.Right;
            double verticalPadding = doc.PagePadding.Top + doc.PagePadding.Bottom;
            double contentWidth = printableWidth - horizontalPadding;
            double availableHeight = printableHeight - verticalPadding;

            // FlowDocument 크기 설정
            doc.PageWidth = printableWidth;
            doc.ColumnWidth = printableWidth;

            // 테이블 컬럼 너비 재설정
            if (totalContentWidth > 0)
            {
                double scaleFactor = contentWidth / totalContentWidth;
                table.Columns.Clear();

                foreach (double originalWidth in originalColumnWidths)
                {
                    double newColWidth = originalWidth * scaleFactor;
                    table.Columns.Add(new TableColumn() { Width = new GridLength(newColWidth) });
                }
            }
            else
            {
                MessageBox.Show("인쇄할 컬럼 너비를 계산할 수 없습니다.", "인쇄 오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 행 높이 계산
            double headerHeight = 80.0; // 헤더 높이 추정
            double rowHeight = 56.0; // 기본 행 높이

            // 페이지당 들어갈 수 있는 행 수 계산
            int rowsPerPage = (int)Math.Floor((availableHeight - headerHeight) / rowHeight);

            // 최소 행 수 보장 (너무 작으면 조정)
            if (rowsPerPage < 1) rowsPerPage = 1;

            // 데이터 행 추가
            int rowIndex = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                // 페이지 시작 시 헤더 삽입 (첫 페이지 제외)
                if (rowIndex > 0 && rowIndex % rowsPerPage == 0)
                {
                    TableRow repeatedHeader = new TableRow();
                    foreach (var cellTemplate in headerTemplateCells)
                    {
                        TableCell cloned = new TableCell(new Paragraph(new Run(
                            ((Run)((Paragraph)cellTemplate.Blocks.FirstBlock).Inlines.FirstInline).Text)))
                        {
                            Background = cellTemplate.Background,
                            Foreground = cellTemplate.Foreground,
                            BorderBrush = cellTemplate.BorderBrush,
                            BorderThickness = cellTemplate.BorderThickness,
                            FontWeight = cellTemplate.FontWeight,
                            TextAlignment = cellTemplate.TextAlignment,
                            Padding = cellTemplate.Padding
                        };
                        repeatedHeader.Cells.Add(cloned);
                    }
                    bodyGroup.Rows.Add(repeatedHeader);
                }

                // 데이터
                TableRow newRow = new TableRow();

                foreach (var column in dataGrid.Columns.OfType<DataGridColumn>().Where(c => c.Visibility == Visibility.Visible))
                {
                    string dataFieldName;
                    if (column.Header.ToString() == "名前")
                    {
                        dataFieldName = "Name";
                    }
                    else if (column.Header is DateHeader dateHeader)
                    {
                        dataFieldName = dateHeader.Day.ToString();
                    }
                    else
                    {
                        continue;
                    }

                    object cellValue = row[dataFieldName];
                    TableCell cell = new TableCell();
                    DailyCellInfo dailyCellInfo = cellValue as DailyCellInfo;

                    string text = (dataFieldName == "Name") ? row["Name"].ToString() : "";
                    Paragraph cellParagraph = new Paragraph(new Run(text));
                    cellParagraph.LineHeight = 40;
                    cellParagraph.Foreground = Brushes.Black;
                    cellParagraph.Margin = new Thickness(0);

                    cell.TextAlignment = TextAlignment.Center;
                    cell.Blocks.Add(cellParagraph);
                    cell.Padding = new Thickness(0, 8, 0, 8);

                    if (dailyCellInfo != null)
                    {
                        cell.Background = CalculateCellBackground(dailyCellInfo);
                        if (dailyCellInfo.CorrectionIndicatorBrush != null)
                        {
                            cell.BorderBrush = dailyCellInfo.CorrectionIndicatorBrush;
                            cell.BorderThickness = new Thickness(3);
                            cellParagraph.LineHeight -= 20; // Thickness 너비를 차감, Thickness로 늘어난 너비 이상은 삭제되지 않더라.
                        }
                    }

                    if (cell.BorderBrush == null)
                    {
                        cell.BorderBrush = Brushes.Black;
                        cell.BorderThickness = new Thickness(0, 0, 1, 1);
                    }

                    newRow.Cells.Add(cell);
                }

                bodyGroup.Rows.Add(newRow);
                rowIndex++;
            }

            // DocumentPaginator 설정 및 인쇄
            var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
            paginator.PageSize = new Size(printableWidth, printableHeight);

            printDlg.PrintDocument(paginator, "DataGrid Paged Print (Landscape)");
        }
    }

    public static void PrintShiftDataGrid(DataGrid dataGrid)
    {
        if (dataGrid?.ItemsSource == null) return;

        var shifts = dataGrid.ItemsSource as IEnumerable<Shift>;
        if (shifts == null || !shifts.Any()) return;

        var shiftList = shifts.ToList();

        FlowDocument doc = new FlowDocument();
        doc.FontFamily = new FontFamily("Meiryo");
        doc.PagePadding = new Thickness(30);
        doc.FontSize = 11;

        PrintDialog printDlg = new PrintDialog();
        printDlg.PrintTicket.PageOrientation = PageOrientation.Portrait;

        if (printDlg.ShowDialog() == true)
        {
            double printableWidth = printDlg.PrintableAreaWidth;
            double printableHeight = printDlg.PrintableAreaHeight;

            double horizontalPadding = doc.PagePadding.Left + doc.PagePadding.Right;
            double verticalPadding = doc.PagePadding.Top + doc.PagePadding.Bottom;
            double contentWidth = printableWidth - horizontalPadding;
            double availableHeight = printableHeight - verticalPadding;

            doc.PageWidth = printableWidth;
            doc.ColumnWidth = printableWidth;

            // 테이블 2개를 가로로 배치
            double tableWidth = (contentWidth - 20) / 2; // 20은 테이블 간 여백
            double colorWidth = 70;
            double nameWidth = 200;
            double scaleFactor = tableWidth / (colorWidth + nameWidth);

            colorWidth *= scaleFactor;
            nameWidth *= scaleFactor;

            // 행 높이
            double headerHeight = 70;
            double rowHeight = 50;
            int rowsPerTable = (int)Math.Floor((availableHeight - headerHeight * 2 - 20) / rowHeight / 2);

            // 절반씩 나누기
            int halfCount = (int)Math.Ceiling(shiftList.Count / 2.0);

            // 왼쪽 테이블 데이터
            var leftShifts = shiftList.Take(halfCount).ToList();
            // 오른쪽 테이블 데이터
            var rightShifts = shiftList.Skip(halfCount).ToList();

            // 2개의 테이블을 감싸는 Grid 생성
            var container = new BlockUIContainer();
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(tableWidth) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) }); // 간격
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(tableWidth) });

            // 왼쪽 테이블
            var leftTable = CreateShiftTable(leftShifts, colorWidth, nameWidth, headerHeight, rowHeight);
            Grid.SetColumn(leftTable, 0);
            grid.Children.Add(leftTable);

            // 오른쪽 테이블
            var rightTable = CreateShiftTable(rightShifts, colorWidth, nameWidth, headerHeight, rowHeight);
            Grid.SetColumn(rightTable, 2);
            grid.Children.Add(rightTable);

            container.Child = grid;
            doc.Blocks.Add(container);

            // DocumentPaginator 설정 및 인쇄
            var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
            paginator.PageSize = new Size(printableWidth, printableHeight);

            printDlg.PrintDocument(paginator, "Shift List Print");
        }
    }

    private static Border CreateShiftTable(List<Shift> shifts, double colorWidth, double nameWidth, double headerHeight, double rowHeight)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(colorWidth) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(nameWidth) });

        // 헤더 + 데이터 행 수
        int totalRows = shifts.Count + 1;
        for (int i = 0; i < totalRows; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(i == 0 ? headerHeight : rowHeight) });
        }

        // 헤더 - 色
        var colorHeader = new Border
        {
            Background = Brushes.LightGray,
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(1),
            Child = new TextBlock
            {
                Text = "色",
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(2)
            }
        };
        Grid.SetRow(colorHeader, 0);
        Grid.SetColumn(colorHeader, 0);
        grid.Children.Add(colorHeader);

        // 헤더 - シフト名
        var nameHeader = new Border
        {
            Background = Brushes.LightGray,
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0, 1, 1, 1),
            Child = new TextBlock
            {
                Text = "シフト名",
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(2)
            }
        };
        Grid.SetRow(nameHeader, 0);
        Grid.SetColumn(nameHeader, 1);
        grid.Children.Add(nameHeader);

        // 데이터 행
        for (int i = 0; i < shifts.Count; i++)
        {
            var shift = shifts[i];
            int rowIndex = i + 1;

            // 色 셀 (배경색으로 표현)
            var colorCell = new Border
            {
                Background = GetShiftColorBrush(shift.ShiftColor),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 0, 1, 1)
            };
            Grid.SetRow(colorCell, rowIndex);
            Grid.SetColumn(colorCell, 0);
            grid.Children.Add(colorCell);

            // シフト名 셀
            var nameCell = new Border
            {
                Background = Brushes.White,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Child = new TextBlock
                {
                    Text = shift.Name ?? "",
                    TextAlignment = TextAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 2, 5, 2)
                }
            };
            Grid.SetRow(nameCell, rowIndex);
            Grid.SetColumn(nameCell, 1);
            grid.Children.Add(nameCell);
        }

        return new Border
        {
            Child = grid,
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0)
        };
    }
    private static Brush GetShiftColorBrush(ShiftColor shiftColor)
    {
        // EnumToBrushConverter와 동일한 로직 적용
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
            ShiftColor.Y => Brushes.Gray,
            _ => Brushes.Transparent
        };
    }
}
