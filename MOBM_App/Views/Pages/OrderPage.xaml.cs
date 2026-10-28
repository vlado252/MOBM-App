using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MOBM_App.Helpers;
using MOBM_App.Models;
using MOBM_App.Services;
using MOBM_App.ViewModels;

namespace MOBM_App.Views.Pages
{
    public partial class OrderPage : Page
    {
        public OrderPage()
        {
            InitializeComponent();

            // ✅ УСТАНАВЛИВАЕМ DataContext на глобальный ViewModel
            // Это главная причина почему товары не появлялись!
            if (App.GlobalOrderVM != null)
            {
                this.DataContext = App.GlobalOrderVM;
            }
            else
            {
                MessageBox.Show("Ошибка: Система заказов не инициализирована!\nПерезапустите приложение.",
                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ Обработчик печати (имя должно совпадать с XAML: Click="Print_Click")
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if (App.GlobalOrderVM == null || App.GlobalOrderVM.CartItems.Count == 0)
            {
                MessageBox.Show("Нет товаров для печати", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var items = App.GlobalOrderVM.CartItems.ToList();

                var order = new Order
                {
                    Id = 0,
                    UserId = DataStorage.CurrentUser?.Id ?? 0,
                    Date = DateTime.Now,
                    TotalPrice = items.Sum(i => i.Sum),
                    Status = "Черновик",
                    UserName = DataStorage.CurrentUser?.Name ?? "Неизвестно",
                    Items = items
                };

                // ✅ Передаем оба параметра: order и items
                PrintService.PrintOrderDetails(order, items);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка печати: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}