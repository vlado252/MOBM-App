using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using MOBM_App.Models;
using MOBM_App.Services;

namespace MOBM_App.Views.Pages
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            try
            {
                var orders = DatabaseService.GetOrdersHistory();
                var products = DatabaseService.GetProducts();
                var users = DatabaseService.GetUsers();

                // Статистика
                TxtTotalOrders.Text = orders.Count.ToString();
                TxtRevenue.Text = string.Format("{0:N0} BYN", orders.Sum(o => o.TotalPrice));
                TxtProducts.Text = products.Count.ToString();
                TxtUsers.Text = users.Count.ToString();

                // График заказов по дням (последние 7 дней)
                var ordersByDate = orders
                    .GroupBy(o => o.Date.Date)
                    .OrderBy(g => g.Key)
                    .Take(7)
                    .ToList();

                var dateLabels = ordersByDate.Select(g => g.Key.ToString("dd.MM")).ToArray();
                var orderCounts = ordersByDate.Select(g => (double)g.Count()).ToArray();

                var lineSeries = new LineSeries
                {
                    Title = "Заказы",
                    Values = new ChartValues<double>(orderCounts),
                    PointGeometrySize = 10,
                    StrokeThickness = 3,
                    Fill = Brushes.Transparent,
                    Stroke = new SolidColorBrush(Color.FromRgb(41, 121, 255))
                };

                OrdersChart.Series = new SeriesCollection { lineSeries };
                OrdersChart.AxisX[0].Labels = dateLabels;

                // Круговая диаграмма категорий
                var furnitureCount = products.Count(p => p.Category == Category.Furniture);
                var rawCount = products.Count(p => p.Category == Category.RawMaterial);

                CategoryChart.Series = new SeriesCollection
                {
                    new PieSeries
                    {
                        Title = "Мебель",
                        Values = new ChartValues<double> { furnitureCount },
                        DataLabels = true,
                        Fill = new SolidColorBrush(Color.FromRgb(41, 121, 255))
                    },
                    new PieSeries
                    {
                        Title = "Сырьё",
                        Values = new ChartValues<double> { rawCount },
                        DataLabels = true,
                        Fill = new SolidColorBrush(Color.FromRgb(255, 145, 0))
                    }
                };

                // Топ товаров по продажам
                var allOrderItems = orders
                    .SelectMany(o => DatabaseService.GetOrderDetails(o.Id))
                    .ToList();

                var topProducts = allOrderItems
                    .GroupBy(i => i.Product != null ? i.Product.Name : "Неизвестно")
                    .Select(g => new { Name = g.Key, Qty = g.Sum(i => i.Quantity) })
                    .OrderByDescending(x => x.Qty)
                    .Take(5)
                    .ToList();

                if (topProducts.Count > 0)
                {
                    var columnSeries = new ColumnSeries
                    {
                        Title = "Продано",
                        Values = new ChartValues<double>(topProducts.Select(p => (double)p.Qty)),
                        Fill = new SolidColorBrush(Color.FromRgb(0, 230, 118))
                    };

                    TopProductsChart.Series = new SeriesCollection { columnSeries };
                    TopProductsChart.AxisX[0].Labels = topProducts.Select(p => p.Name).ToArray();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки дашборда: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}