using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MOBM_App.Models;
using MOBM_App.Services;

namespace MOBM_App.Views
{
    public partial class OrderDetailsWindow : Window
    {
        private Order _order;
        private List<OrderItemView> _itemsView;

        public OrderDetailsWindow(int orderId)
        {
            InitializeComponent();
            LoadOrderDetails(orderId);
        }

        private void LoadOrderDetails(int orderId)
        {
            try
            {
                _order = DatabaseService.GetOrderById(orderId);

                if (_order == null)
                {
                    MessageBox.Show("Заказ не найден!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                var orderItems = DatabaseService.GetOrderDetails(orderId);
                _itemsView = orderItems.Select(item => new OrderItemView
                {
                    ProductName = item.Product?.Name ?? "Неизвестно",
                    Price = item.Product?.Price ?? 0,
                    Quantity = item.Quantity
                }).ToList();

                TxtOrderId.Text = $"№{_order.Id}";
                TxtOrderDate.Text = _order.Date.ToString("dd.MM.yyyy HH:mm");
                TxtUserName.Text = _order.UserName;
                TxtStatus.Text = _order.Status;
                TxtTotal.Text = $"{_order.TotalPrice:N0} BYN";

                ItemsGrid.ItemsSource = _itemsView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказа: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }
        private void BtnTextReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (_order == null || _itemsView == null) return;

            string receiptText = "═══════════════════════════════════════\n";
            receiptText += "           ЧЕК ЗАКАЗА\n";
            receiptText += "═══════════════════════════════════════\n\n";
            receiptText += $"Номер заказа: №{_order.Id}\n";
            receiptText += $"Дата: {_order.Date:dd.MM.yyyy HH:mm}\n";
            receiptText += $"Оформил: {_order.UserName}\n";
            receiptText += $"Статус: {_order.Status}\n\n";
            receiptText += "───────────────────────────────────────\n";
            receiptText += "ТОВАРЫ:\n";
            receiptText += "───────────────────────────────────────\n";

            int itemNumber = 1;
            foreach (var item in _itemsView)
            {
                receiptText += $"{itemNumber}. {item.ProductName}\n";
                receiptText += $"   Цена: {item.Price:N0} BYN\n";
                receiptText += $"   Кол-во: {item.Quantity} шт.\n";
                receiptText += $"   Сумма: {item.Sum:N0} BYN\n\n";
                itemNumber++;
            }

            receiptText += "───────────────────────────────────────\n";
            receiptText += $"ИТОГО: {_order.TotalPrice:N0} BYN\n";
            receiptText += "═══════════════════════════════════════\n";
            receiptText += "Спасибо за покупку!\n";
            receiptText += "═══════════════════════════════════════";

            var textWindow = new Window
            {
                Title = $"Текстовый чек заказа №{_order.Id}",
                Width = 500,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(30, 30, 30))
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.Margin = new Thickness(20, 20, 20, 20);

            var textBox = new TextBox
            {
                Text = receiptText,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 13,
                Foreground = System.Windows.Media.Brushes.White,
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(40, 40, 40)),
                BorderBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(80, 80, 80)),
                BorderThickness = new Thickness(1, 1, 1, 1),
                Padding = new Thickness(15, 15, 15, 15),
                TextWrapping = TextWrapping.Wrap,
                IsReadOnly = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Margin = new Thickness(0, 0, 0, 15)
            };
            Grid.SetRow(textBox, 0);

            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var copyBtn = new Button
            {
                Content = "📋 Копировать",
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(41, 121, 255)),
                Foreground = System.Windows.Media.Brushes.White,
                Padding = new Thickness(20, 10, 20, 10),
                Margin = new Thickness(0, 0, 10, 0),
                BorderThickness = new Thickness(0, 0, 0, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontSize = 13
            };

            copyBtn.Click += (s, args) =>
            {
                Clipboard.SetText(receiptText);
                MessageBox.Show("Чек скопирован в буфер обмена!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            };

            var closeBtn = new Button
            {
                Content = "Закрыть",
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(255, 23, 68)),
                Foreground = System.Windows.Media.Brushes.White,
                Padding = new Thickness(20, 10, 20, 10),
                BorderThickness = new Thickness(0, 0, 0, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontSize = 13
            };

            closeBtn.Click += (s, args) => textWindow.Close();

            buttonsPanel.Children.Add(copyBtn);
            buttonsPanel.Children.Add(closeBtn);
            Grid.SetRow(buttonsPanel, 1);

            grid.Children.Add(textBox);
            grid.Children.Add(buttonsPanel);

            textWindow.Content = grid;
            textWindow.ShowDialog();
        }
        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (_order != null && _itemsView != null)
            {
                var items = _itemsView.Select(v => new OrderItem
                {
                    Product = new Product { Name = v.ProductName, Price = v.Price },
                    Quantity = v.Quantity
                }).ToList();

                PrintService.PrintOrderDetails(_order, items);
            }
        }

        private void BtnEmail_Click(object sender, RoutedEventArgs e)
        {
            if (_order != null && _itemsView != null)
            {
                var items = _itemsView.Select(v => new OrderItem
                {
                    Product = new Product { Name = v.ProductName, Price = v.Price },
                    Quantity = v.Quantity
                }).ToList();

                EmailService.SendOrderDetails(_order, items);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}