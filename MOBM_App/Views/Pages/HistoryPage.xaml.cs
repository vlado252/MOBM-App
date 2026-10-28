using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using MOBM_App.Helpers;
using MOBM_App.Services;
using MOBM_App.Views;

namespace MOBM_App.Views.Pages
{
    public partial class HistoryPage : Page
    {
        public HistoryPage()
        {
            InitializeComponent();
            LoadHistory();
        }

        private void LoadHistory()
        {
            HistoryGrid.ItemsSource = DatabaseService.GetOrdersHistory();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = DataStorage.CurrentUser;
            if (currentUser == null || !Permissions.CanExportData(currentUser.UserRole))
            {
                MessageBox.Show("❌ У вас нет прав для экспорта данных!",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var orders = DatabaseService.GetOrdersHistory();

                if (orders.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта!", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Экспорт в Excel
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("История заказов");

                    // Заголовки
                    worksheet.Cell(1, 1).Value = "ID";
                    worksheet.Cell(1, 2).Value = "Дата";
                    worksheet.Cell(1, 3).Value = "Сумма, BYN";
                    worksheet.Cell(1, 4).Value = "Статус";
                    worksheet.Cell(1, 5).Value = "Оформил";

                    // Данные
                    int row = 2;
                    foreach (var order in orders)
                    {
                        worksheet.Cell(row, 1).Value = order.Id;
                        worksheet.Cell(row, 2).Value = order.Date;
                        worksheet.Cell(row, 3).Value = (double)order.TotalPrice;
                        worksheet.Cell(row, 4).Value = order.Status;
                        worksheet.Cell(row, 5).Value = order.UserName;
                        row++;
                    }

                    // Стилизация
                    worksheet.Range(1, 1, 1, 5).Style.Font.Bold = true;
                    worksheet.Range(1, 1, 1, 5).Style.Fill.BackgroundColor = XLColor.FromArgb(41, 121, 255);
                    worksheet.Range(1, 1, 1, 5).Style.Font.FontColor = XLColor.White;

                    worksheet.Columns().AdjustToContents();

                    string filePath = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        "История_заказов_" + DateTime.Now.ToString("dd.MM.yyyy") + ".xlsx");

                    workbook.SaveAs(filePath);

                    MessageBox.Show("✅ Экспорт в Excel выполнен!\nФайл сохранён на рабочем столе:\n" + filePath,
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // НОВЫЙ МЕТОД ДЛЯ ПРОСМОТРА ДЕТАЛЕЙ ЗАКАЗА
        private void ViewOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                // Убрали MessageBox.Show для отладки

                if (btn.Tag is int orderId)
                {
                    var detailsWindow = new OrderDetailsWindow(orderId);
                    detailsWindow.Owner = Application.Current.MainWindow;
                    detailsWindow.ShowDialog();
                }
                else if (int.TryParse(btn.Tag?.ToString(), out int parsedId))
                {
                    var detailsWindow = new OrderDetailsWindow(parsedId);
                    detailsWindow.Owner = Application.Current.MainWindow;
                    detailsWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Не удалось получить ID заказа!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}