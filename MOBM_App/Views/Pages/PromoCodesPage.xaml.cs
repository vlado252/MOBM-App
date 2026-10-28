using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MOBM_App.Helpers;
using MOBM_App.Models;
using MOBM_App.Services;

namespace MOBM_App.Views.Pages
{
    public partial class PromoCodesPage : Page
    {
        public PromoCodesPage()
        {
            InitializeComponent();
            LoadPromoCodes();
        }

        private void LoadPromoCodes()
        {
            try
            {
                var promos = DatabaseService.GetAllPromoCodes();

                // Добавляем StatusText для отображения
                var displayPromos = promos.Select(p => new
                {
                    p.Id,
                    p.Code,
                    p.DiscountPercent,
                    p.ValidUntil,
                    p.IsActive,
                    p.Description,
                    StatusText = p.IsActive && p.ValidUntil >= DateTime.Now ? "✅ Активен" : "❌ Неактивен"
                }).ToList();

                PromoGrid.ItemsSource = displayPromos;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки промокодов: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadPromoCodes();
        }

        private void AddPromo_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddPromoCodeWindow();
            addWindow.Owner = Window.GetWindow(this);

            if (addWindow.ShowDialog() == true)
            {
                LoadPromoCodes();
                MessageBox.Show("Промокод успешно добавлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (PromoGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите промокод для редактирования!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedItem = PromoGrid.SelectedItem;
                var promoId = (int)selectedItem.GetType().GetProperty("Id").GetValue(selectedItem);

                // Получаем полные данные промокода
                var promo = DatabaseService.GetAllPromoCodes().FirstOrDefault(p => p.Id == promoId);

                if (promo != null)
                {
                    var editWindow = new AddPromoCodeWindow(promo);
                    editWindow.Owner = Window.GetWindow(this);

                    if (editWindow.ShowDialog() == true)
                    {
                        LoadPromoCodes();
                        MessageBox.Show("Промокод успешно обновлён!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка редактирования: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (PromoGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите промокод для удаления!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedItem = PromoGrid.SelectedItem;
                var promoId = (int)selectedItem.GetType().GetProperty("Id").GetValue(selectedItem);
                var promoCode = selectedItem.GetType().GetProperty("Code").GetValue(selectedItem)?.ToString();

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить промокод:\n\n{promoCode}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DatabaseService.DeletePromoCode(promoId);
                    LoadPromoCodes();
                    MessageBox.Show("Промокод удалён!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}