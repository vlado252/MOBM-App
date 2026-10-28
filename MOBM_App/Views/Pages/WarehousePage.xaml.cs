using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MOBM_App.Helpers;
using MOBM_App.Models;
using MOBM_App.Services;


namespace MOBM_App.Views.Pages
{
    public partial class WarehousePage : Page
    {
        private List<Product> _allMaterials;
        private List<Product> _filteredMaterials;

        public WarehousePage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _allMaterials = DatabaseService.GetRawMaterials();
                _filteredMaterials = new List<Product>(_allMaterials);

                var stats = DatabaseService.GetWarehouseStats();
                TxtTotalItems.Text = stats.TotalItems.ToString();
                TxtTotalStock.Text = stats.TotalStock.ToString();
                TxtTotalValue.Text = string.Format("{0:N0} BYN", stats.TotalValue);
                TxtLowStock.Text = stats.LowStockItems.ToString();

                if (stats.LowStockItems > 0)
                    TxtLowStock.Foreground = new SolidColorBrush(Color.FromRgb(255, 23, 68));

                WarehouseGrid.ItemsSource = _filteredMaterials;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных склада: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ChkLowStock_Changed(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var search = TxtSearch.Text != null ? TxtSearch.Text.ToLower() : "";
            var onlyLow = ChkLowStock.IsChecked == true;

            _filteredMaterials = _allMaterials.Where(p =>
            {
                bool matchSearch = string.IsNullOrEmpty(search) ||
                    (p.Name != null && p.Name.ToLower().Contains(search)) ||
                    (p.Material != null && p.Material.ToLower().Contains(search));

                bool matchLow = !onlyLow || p.Stock < 100;

                return matchSearch && matchLow;
            }).ToList();

            WarehouseGrid.ItemsSource = _filteredMaterials;
        }

        private void BtnIncome_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = DataStorage.CurrentUser;
            if (currentUser == null || !Permissions.CanManageStock(currentUser.UserRole))
            {
                MessageBox.Show("❌ У вас нет прав для проведения операций со складом!\n" +
                    "Только администратор и кладовщик могут выполнять эту операцию.",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(WarehouseGrid.SelectedItem is Product selected))
            {
                MessageBox.Show("Выберите товар из таблицы!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new StockOperationWindow(selected.Name, selected.Stock, "Приход");
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true && dialog.ResultQuantity.HasValue)
            {
                try
                {
                    DatabaseService.UpdateStockWithLog(
                        selected.Id,
                        dialog.ResultQuantity.Value,
                        "Приход",
                        dialog.Comment);

                    MessageBox.Show(
                        "Приход оформлен: +" + dialog.ResultQuantity.Value + " шт.\n" +
                        "Новый остаток: " + (selected.Stock + dialog.ResultQuantity.Value) + " шт.",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnExpense_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = DataStorage.CurrentUser;
            if (currentUser == null || !Permissions.CanManageStock(currentUser.UserRole))
            {
                MessageBox.Show("❌ У вас нет прав для проведения операций со складом!",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(WarehouseGrid.SelectedItem is Product selected))
            {
                MessageBox.Show("Выберите товар из таблицы!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new StockOperationWindow(selected.Name, selected.Stock, "Расход");
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true && dialog.ResultQuantity.HasValue)
            {
                if (dialog.ResultQuantity.Value > selected.Stock)
                {
                    MessageBox.Show(
                        "Нельзя списать больше, чем есть на складе!\n" +
                        "Доступно: " + selected.Stock + " шт.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    DatabaseService.UpdateStockWithLog(
                        selected.Id,
                        -dialog.ResultQuantity.Value,
                        "Расход",
                        dialog.Comment);

                    MessageBox.Show(
                        "Расход оформлен: -" + dialog.ResultQuantity.Value + " шт.\n" +
                        "Новый остаток: " + (selected.Stock - dialog.ResultQuantity.Value) + " шт.",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CheckWarehouseAccess()
        {
            var user = DataStorage.CurrentUser;
            if (user == null) return false;

            if (user.UserRole == Role.Manager)
            {
                MessageBox.Show(
                    "Менеджер может только просматривать склад.\n" +
                    "Для операций приход/расход войдите как Кладовщик или Администратор.",
                    "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}