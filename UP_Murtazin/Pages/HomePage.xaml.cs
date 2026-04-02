using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using UP_Murtazin.DB;

namespace UP_Murtazin.Pages
{
    public partial class HomePage : Page
    {
        private UP_MurtazinEntities dbContext;
        private DateTime lastDataUpdate;

        public HomePage()
        {
            InitializeComponent();
            dbContext = new UP_MurtazinEntities();
            lastDataUpdate = DateTime.Now;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                lastDataUpdate = DateTime.Now;
                LastUpdateDate.Text = $"Данные актуальны на: {lastDataUpdate:dd.MM.yyyy HH:mm}";

                LoadEfficiencyData();
                LoadNetworkState();
                LoadSummaryData();
                LoadSalesData();
                LoadNews();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadEfficiencyData()
        {
            try
            {
                var machines = dbContext.kit_online_id.ToList();
                int total = machines.Count;
                int working = machines.Count(m => m.status == "Работает");
                double percent = total > 0 ? (double)working / total : 0;

                WorkingMachinesPercent.Text = $"{Math.Round(percent * 100)}%";
                WorkingMachinesCount.Text = $"Работающих автоматов: {working} из {total}";

                UpdateSemiCircleProgress(percent);
            }
            catch (Exception ex)
            {
                WorkingMachinesPercent.Text = "0%";
                WorkingMachinesCount.Text = $"Работающих автоматов: 0 из 0";
            }
        }

        private void UpdateSemiCircleProgress(double percent)
        {
            try
            {
                double angle = percent * 180;
                double radians = angle * Math.PI / 180;

                double centerX = 50;
                double centerY = 50;
                double radius = 40;

                double startX = 10;
                double startY = 50;
                double endX = centerX + radius * Math.Cos(radians);
                double endY = centerY - radius * Math.Sin(radians);

                bool isLargeArc = angle > 180;

                var arcSegment = new ArcSegment
                {
                    Point = new Point(endX, endY),
                    Size = new Size(radius, radius),
                    SweepDirection = SweepDirection.Clockwise,
                    IsLargeArc = isLargeArc
                };

                var pathFigure = new PathFigure
                {
                    StartPoint = new Point(startX, startY)
                };
                pathFigure.Segments.Add(arcSegment);

                var pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(pathFigure);

                ProgressPath.Data = pathGeometry;
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки отрисовки
            }
        }

        private void LoadNetworkState()
        {
            try
            {
                // Получаем статусы автоматов
                var machines = dbContext.kit_online_id.ToList();
                int total = machines.Count;

                int working = machines.Count(m => m.status == "Работает");
                int maintenance = machines.Count(m => m.status == "Обслуживается");
                int broken = machines.Count(m => m.status == "Сломан");

                WorkingCount.Text = working.ToString();
                MaintenanceCount.Text = maintenance.ToString();
                BrokenCount.Text = broken.ToString();
                TotalMachinesCount.Text = total.ToString();

                // Рисуем разноцветные сектора круга
                DrawNetworkCircleSectors(total, working, maintenance, broken);
            }
            catch (Exception ex)
            {
                WorkingCount.Text = "0";
                MaintenanceCount.Text = "0";
                BrokenCount.Text = "0";
                TotalMachinesCount.Text = "0";
            }
        }

        private void DrawNetworkCircleSectors(int total, int working, int maintenance, int broken)
        {
            try
            {
                if (total == 0)
                {
                    WorkingSector.Data = null;
                    MaintenanceSector.Data = null;
                    BrokenSector.Data = null;
                    return;
                }

                double workingPercent = (double)working / total;
                double maintenancePercent = (double)maintenance / total;
                double brokenPercent = (double)broken / total;

                double radius = 46; // Радиус круга
                double centerX = 50;
                double centerY = 50;

                double startAngle = -90; // Начинаем с верхней точки

                // Сектор "Работает" (зеленый)
                if (workingPercent > 0)
                {
                    double sweepAngle = workingPercent * 360;
                    WorkingSector.Data = CreateArcSegment(centerX, centerY, radius, startAngle, sweepAngle);
                    startAngle += sweepAngle;
                }
                else
                {
                    WorkingSector.Data = null;
                }

                // Сектор "Обслуживается" (оранжевый)
                if (maintenancePercent > 0)
                {
                    double sweepAngle = maintenancePercent * 360;
                    MaintenanceSector.Data = CreateArcSegment(centerX, centerY, radius, startAngle, sweepAngle);
                    startAngle += sweepAngle;
                }
                else
                {
                    MaintenanceSector.Data = null;
                }

                // Сектор "Сломан" (красный)
                if (brokenPercent > 0)
                {
                    double sweepAngle = brokenPercent * 360;
                    BrokenSector.Data = CreateArcSegment(centerX, centerY, radius, startAngle, sweepAngle);
                }
                else
                {
                    BrokenSector.Data = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отрисовки круга: {ex.Message}");
            }
        }

        private PathGeometry CreateArcSegment(double centerX, double centerY, double radius, double startAngle, double sweepAngle)
        {
            try
            {
                // Конвертируем углы в радианы
                double startRad = startAngle * Math.PI / 180;
                double endRad = (startAngle + sweepAngle) * Math.PI / 180;

                // Вычисляем начальную и конечную точки дуги
                double startX = centerX + radius * Math.Cos(startRad);
                double startY = centerY + radius * Math.Sin(startRad);
                double endX = centerX + radius * Math.Cos(endRad);
                double endY = centerY + radius * Math.Sin(endRad);

                bool isLargeArc = sweepAngle > 180;

                // Создаем дугу
                var arcSegment = new ArcSegment
                {
                    Point = new Point(endX, endY),
                    Size = new Size(radius, radius),
                    SweepDirection = SweepDirection.Clockwise,
                    IsLargeArc = isLargeArc
                };

                // Создаем фигуру
                var pathFigure = new PathFigure
                {
                    StartPoint = new Point(startX, startY)
                };
                pathFigure.Segments.Add(arcSegment);

                var pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(pathFigure);

                return pathGeometry;
            }
            catch
            {
                return null;
            }
        }

        private void LoadSummaryData()
        {
            try
            {
                SummaryDate.Text = $"Данные на {lastDataUpdate:dd.MM.yyyy HH:mm}";

                // Денег в ТА
                double totalIncome = 0;
                var incomes = dbContext.rfid_services.Where(r => r.total_income != null).ToList();
                foreach (var income in incomes)
                {
                    if (double.TryParse(income.total_income, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double value))
                        totalIncome += value;
                }
                MoneyInMachines.Text = $"{totalIncome:N0} р.";

                double change = totalIncome * 0.3;
                ChangeInMachines.Text = $"{change:N0} р.";

                // Получаем продажи
                var sales = dbContext.sales.Where(s => s.timestamp != null).ToList();
                var salesByDate = new Dictionary<DateTime, SalesData>();

                foreach (var sale in sales)
                {
                    if (DateTime.TryParse(sale.timestamp, out DateTime date))
                    {
                        date = date.Date;
                        double total = 0;
                        double quantity = sale.quantity ?? 0;

                        if (double.TryParse(sale.total_price, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double price))
                            total = price;

                        if (salesByDate.ContainsKey(date))
                        {
                            salesByDate[date].TotalSum += total;
                            salesByDate[date].TotalQuantity += quantity;
                        }
                        else
                        {
                            salesByDate[date] = new SalesData
                            {
                                Date = date,
                                TotalSum = total,
                                TotalQuantity = quantity
                            };
                        }
                    }
                }

                var sortedSales = salesByDate.Values.OrderByDescending(x => x.Date).ToList();

                if (sortedSales.Count > 0)
                {
                    RevenueToday.Text = $"{sortedSales[0].TotalSum:N0} р.";
                    RevenueToday.Text += $" ({sortedSales[0].Date:dd.MM})";

                    if (sortedSales.Count > 1)
                    {
                        RevenueYesterday.Text = $"{sortedSales[1].TotalSum:N0} р.";
                        RevenueYesterday.Text += $" ({sortedSales[1].Date:dd.MM})";
                    }
                    else
                    {
                        RevenueYesterday.Text = "0 р. (нет данных)";
                    }
                }
                else
                {
                    RevenueToday.Text = "0 р.";
                    RevenueYesterday.Text = "0 р.";
                }

                double collectedToday = sortedSales.Count > 0 ? sortedSales[0].TotalSum * 0.8 : 0;
                double collectedYesterday = sortedSales.Count > 1 ? sortedSales[1].TotalSum * 0.8 : 0;
                CollectedToday.Text = $"{collectedToday:N0} р.";
                CollectedYesterday.Text = $"{collectedYesterday:N0} р.";

                var today = DateTime.Now.Date;
                var yesterday = today.AddDays(-1);

                int servicedToday = 0;
                int servicedYesterday = 0;

                var maintenances = dbContext.maintenance.Where(m => m.date.HasValue).ToList();
                foreach (var maint in maintenances)
                {
                    if (maint.date.HasValue)
                    {
                        if (maint.date.Value.Date == today)
                            servicedToday++;
                        else if (maint.date.Value.Date == yesterday)
                            servicedYesterday++;
                    }
                }

                ServicedToday.Text = $"{servicedToday} / {servicedYesterday}";
            }
            catch (Exception ex)
            {
                MoneyInMachines.Text = "0 р.";
                ChangeInMachines.Text = "0 р.";
                RevenueToday.Text = "0 р.";
                RevenueYesterday.Text = "0 р.";
                CollectedToday.Text = "0 р.";
                CollectedYesterday.Text = "0 р.";
                ServicedToday.Text = "0 / 0";
            }
        }

        private void LoadSalesData()
        {
            try
            {
                var sales = dbContext.sales.Where(s => s.timestamp != null).ToList();
                var salesByDate = new Dictionary<DateTime, SalesData>();

                foreach (var sale in sales)
                {
                    if (DateTime.TryParse(sale.timestamp, out DateTime date))
                    {
                        date = date.Date;
                        double total = 0;
                        double quantity = sale.quantity ?? 0;

                        if (double.TryParse(sale.total_price, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double price))
                            total = price;

                        if (salesByDate.ContainsKey(date))
                        {
                            salesByDate[date].TotalSum += total;
                            salesByDate[date].TotalQuantity += quantity;
                        }
                        else
                        {
                            salesByDate[date] = new SalesData
                            {
                                Date = date,
                                TotalSum = total,
                                TotalQuantity = quantity
                            };
                        }
                    }
                }

                var sortedSales = salesByDate.Values.OrderBy(x => x.Date).ToList();
                var last10Sales = new List<SalesData>();

                if (sortedSales.Count > 10)
                {
                    last10Sales = sortedSales.Skip(sortedSales.Count - 10).Take(10).ToList();
                }
                else
                {
                    last10Sales = sortedSales;
                }

                if (last10Sales.Any())
                {
                    var startDate = last10Sales.First().Date;
                    var endDate = last10Sales.Last().Date;
                    SalesDateRange.Text = $"Данные по продажам с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}";
                }
                else
                {
                    SalesDateRange.Text = "Нет данных о продажах";
                }

                DrawChart(SalesChartCanvas, last10Sales);
                DrawQuantityChart(QuantityChartCanvas, last10Sales);
            }
            catch (Exception ex)
            {
                SalesDateRange.Text = "Данные по продажам отсутствуют";
            }
        }

        private void DrawChart(Canvas canvas, List<SalesData> salesData)
        {
            canvas.Children.Clear();

            if (!salesData.Any() || salesData.All(d => d.TotalSum == 0))
            {
                var noDataText = new TextBlock
                {
                    Text = "Нет данных для отображения",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#95A5A6")),
                    FontSize = 12
                };
                canvas.Children.Add(noDataText);
                Canvas.SetLeft(noDataText, canvas.Width / 2 - 70);
                Canvas.SetTop(noDataText, canvas.Height / 2 - 10);
                return;
            }

            double maxValue = salesData.Max(d => d.TotalSum);
            if (maxValue == 0) maxValue = 1;

            double canvasWidth = canvas.Width;
            double canvasHeight = canvas.Height;
            double barWidth = (canvasWidth - 80) / salesData.Count;
            double bottomMargin = 40;
            double leftMargin = 50;

            // Рисуем горизонтальные линии сетки
            int steps = 4;
            for (int i = 0; i <= steps; i++)
            {
                double value = maxValue * i / steps;
                double y = canvasHeight - bottomMargin - (canvasHeight - bottomMargin - 20) * i / steps;

                var gridLine = new Line
                {
                    X1 = leftMargin - 5,
                    Y1 = y,
                    X2 = canvasWidth - 20,
                    Y2 = y,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E4E7EB")),
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection { 4, 4 }
                };
                canvas.Children.Add(gridLine);

                var label = new TextBlock
                {
                    Text = Math.Round(value).ToString(),
                    FontSize = 9,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D"))
                };
                canvas.Children.Add(label);
                Canvas.SetLeft(label, 5);
                Canvas.SetTop(label, y - 7);
            }

            // Рисуем оси
            var axisY = new Line
            {
                X1 = leftMargin - 5,
                Y1 = 10,
                X2 = leftMargin - 5,
                Y2 = canvasHeight - bottomMargin,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                StrokeThickness = 1.5
            };
            canvas.Children.Add(axisY);

            var axisX = new Line
            {
                X1 = leftMargin - 5,
                Y1 = canvasHeight - bottomMargin,
                X2 = canvasWidth - 20,
                Y2 = canvasHeight - bottomMargin,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                StrokeThickness = 1.5
            };
            canvas.Children.Add(axisX);

            // Рисуем столбцы
            for (int i = 0; i < salesData.Count; i++)
            {
                double height = (salesData[i].TotalSum / maxValue) * (canvasHeight - bottomMargin - 20);
                if (height < 3) height = 3;

                double x = leftMargin + 5 + i * barWidth;
                double y = canvasHeight - bottomMargin - height;

                // Основной столбец с прозрачностью
                var rect = new Rectangle
                {
                    Width = barWidth - 4,
                    Height = height,
                    Fill = new SolidColorBrush(Color.FromArgb(100, 52, 152, 219)), // Голубой с прозрачностью
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2980B9")), // Темно-синяя окантовка
                    StrokeThickness = 1,
                    RadiusX = 3,
                    RadiusY = 3
                };
                canvas.Children.Add(rect);
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);

                // Подписи дат под столбцами
                var dateText = new TextBlock
                {
                    Text = salesData[i].Date.ToString("dd.MM"),
                    FontSize = 9,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50")),
                    FontWeight = FontWeights.Medium
                };
                canvas.Children.Add(dateText);
                Canvas.SetLeft(dateText, x + (barWidth - 4) / 2 - 15);
                Canvas.SetTop(dateText, canvasHeight - bottomMargin + 8);
            }
        }

        private void DrawQuantityChart(Canvas canvas, List<SalesData> salesData)
        {
            canvas.Children.Clear();

            if (!salesData.Any() || salesData.All(d => d.TotalQuantity == 0))
            {
                var noDataText = new TextBlock
                {
                    Text = "Нет данных для отображения",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#95A5A6")),
                    FontSize = 12
                };
                canvas.Children.Add(noDataText);
                Canvas.SetLeft(noDataText, canvas.Width / 2 - 70);
                Canvas.SetTop(noDataText, canvas.Height / 2 - 10);
                return;
            }

            double maxValue = salesData.Max(d => d.TotalQuantity);
            if (maxValue == 0) maxValue = 1;

            double canvasWidth = canvas.Width;
            double canvasHeight = canvas.Height;
            double barWidth = (canvasWidth - 80) / salesData.Count;
            double bottomMargin = 40;
            double leftMargin = 50;

            // Рисуем горизонтальные линии сетки
            int steps = 4;
            for (int i = 0; i <= steps; i++)
            {
                double value = maxValue * i / steps;
                double y = canvasHeight - bottomMargin - (canvasHeight - bottomMargin - 20) * i / steps;

                var gridLine = new Line
                {
                    X1 = leftMargin - 5,
                    Y1 = y,
                    X2 = canvasWidth - 20,
                    Y2 = y,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E4E7EB")),
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection { 4, 4 }
                };
                canvas.Children.Add(gridLine);

                var label = new TextBlock
                {
                    Text = Math.Round(value).ToString(),
                    FontSize = 9,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D"))
                };
                canvas.Children.Add(label);
                Canvas.SetLeft(label, 5);
                Canvas.SetTop(label, y - 7);
            }

            // Рисуем оси
            var axisY = new Line
            {
                X1 = leftMargin - 5,
                Y1 = 10,
                X2 = leftMargin - 5,
                Y2 = canvasHeight - bottomMargin,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                StrokeThickness = 1.5
            };
            canvas.Children.Add(axisY);

            var axisX = new Line
            {
                X1 = leftMargin - 5,
                Y1 = canvasHeight - bottomMargin,
                X2 = canvasWidth - 20,
                Y2 = canvasHeight - bottomMargin,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                StrokeThickness = 1.5
            };
            canvas.Children.Add(axisX);

            // Рисуем столбцы
            for (int i = 0; i < salesData.Count; i++)
            {
                double height = (salesData[i].TotalQuantity / maxValue) * (canvasHeight - bottomMargin - 20);
                if (height < 3) height = 3;

                double x = leftMargin + 5 + i * barWidth;
                double y = canvasHeight - bottomMargin - height;

                var rect = new Rectangle
                {
                    Width = barWidth - 4,
                    Height = height,
                    Fill = new SolidColorBrush(Color.FromArgb(100, 52, 152, 219)),
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2980B9")),
                    StrokeThickness = 1,
                    RadiusX = 3,
                    RadiusY = 3
                };
                canvas.Children.Add(rect);
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);

                var dateText = new TextBlock
                {
                    Text = salesData[i].Date.ToString("dd.MM"),
                    FontSize = 9,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50")),
                    FontWeight = FontWeights.Medium
                };
                canvas.Children.Add(dateText);
                Canvas.SetLeft(dateText, x + (barWidth - 4) / 2 - 15);
                Canvas.SetTop(dateText, canvasHeight - bottomMargin + 8);
            }
        }

        private void LoadNews()
        {
            try
            {
                var news = new List<NewsItem>
                {
                    new NewsItem { Date = "29.01.25", Title = "Терминалы KiPops получили эквайринг от Сберга" },
                    new NewsItem { Date = "31.12.24", Title = "Новогоднее поздравление от KIT Vending / KIT Shop" },
                    new NewsItem { Date = "28.12.24", Title = "Ставки НДС 5% и 7% для УСН" },
                    new NewsItem { Date = "04.12.24", Title = "Релиз новой CRM-системы KIT Shop" },
                    new NewsItem { Date = "27.11.24", Title = "Новые модели снековых автоматов от KIT Vending" },
                    new NewsItem { Date = "20.11.24", Title = "Получение сертификата PCI DSS 4.0.1" }
                };

                NewsListBox.ItemsSource = news;
            }
            catch (Exception ex)
            {
                NewsListBox.ItemsSource = new List<NewsItem>();
            }
        }
    }

    public class SalesData
    {
        public DateTime Date { get; set; }
        public double TotalSum { get; set; }
        public double TotalQuantity { get; set; }
    }

    public class NewsItem
    {
        public string Date { get; set; }
        public string Title { get; set; }
    }
}