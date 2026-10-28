using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using MOBM_App.Models;

namespace MOBM_App.Services
{
    public static class PrintService
    {
        public static void PrintOrder(Order order, User user)
        {
            string itemsText = string.Join("\n",
                order.Items.Select(i => $"{i.Product.Name} x{i.Quantity} = {i.Sum:N2} BYN"));

            TextBlock printContent = new TextBlock
            {
                Text = $"ЗАКАЗ №{order.Id}\nДата: {order.Date.ToShortDateString()}\n" +
                       $"Менеджер: {user?.Name ?? "Неизвестно"}\n\n" +
                       $"ТОВАРЫ:\n{itemsText}\n\n" +
                       $"ИТОГО: {order.TotalPrice:N2} BYN",
                FontFamily = new System.Windows.Media.FontFamily("Arial"),
                Padding = new Thickness(20),
                Foreground = System.Windows.Media.Brushes.Black
            };

            if (MessageBox.Show("Отправить заказ на печать?", "Печать", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                PrintDialog pd = new PrintDialog();
                if (pd.ShowDialog() == true)
                {
                    pd.PrintVisual(printContent, $"Заказ MOBM #{order.Id}");
                }
            }
        }

        // НОВЫЙ МЕТОД ДЛЯ ПЕЧАТИ ДЕТАЛЕЙ ЗАКАЗА ИЗ ИСТОРИИ
        public static void PrintOrderDetails(Order order, List<OrderItem> items)
        {
            string itemsText = string.Join("\n",
                items.Select(i => $"{i.Product.Name,-30} x{i.Quantity,-5} = {i.Sum:N2} BYN"));
            if (items == null) items = order.Items;
            TextBlock printContent = new TextBlock
            {
                Text = $"═══════════════════════════════════\n" +
                       $"         ЧЕК ЗАКАЗА №{order.Id}\n" +
                       $"═══════════════════════════════════\n\n" +
                       $"Дата: {order.Date:dd.MM.yyyy HH:mm}\n" +
                       $"Оформил: {order.UserName}\n" +
                       $"Статус: {order.Status}\n\n" +
                       $"───────────────────────────────────\n" +
                       $"ТОВАРЫ:\n" +
                       $"───────────────────────────────────\n" +
                       $"{itemsText}\n" +
                       $"───────────────────────────────────\n\n" +
                       $"ИТОГО: {order.TotalPrice:N2} BYN\n" +
                       $"═══════════════════════════════════\n" +
                       $"Спасибо за покупку!\n",
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 12,
                Padding = new Thickness(30),
                Foreground = System.Windows.Media.Brushes.Black
            };

            if (MessageBox.Show("Отправить чек на печать?", "Печать чека", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                PrintDialog pd = new PrintDialog();
                if (pd.ShowDialog() == true)
                {
                    pd.PrintVisual(printContent, $"Чек заказа #{order.Id}");
                    MessageBox.Show("Чек успешно отправлен на печать!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}