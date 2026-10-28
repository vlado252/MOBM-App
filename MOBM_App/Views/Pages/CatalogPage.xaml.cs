using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MOBM_App.Helpers;
using MOBM_App.Models;
using MOBM_App.ViewModels;
using MOBM_App;
using MOBM_App.Services;

namespace MOBM_App.Views.Pages
{
    public partial class CatalogPage : Page
    {
        private MainViewModel _vm;

        public CatalogPage()
        {
            InitializeComponent();
            _vm = DataContext as MainViewModel;

            // ✅ Подписка на обновление склада
            if (App.GlobalOrderVM != null)
            {
                App.GlobalOrderVM.OnStockUpdated += RefreshCatalog;
            }

            this.Unloaded += CatalogPage_Unloaded;

            SetupRoleBasedAccess();
        }

        private void CatalogPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (App.GlobalOrderVM != null)
            {
                App.GlobalOrderVM.OnStockUpdated -= RefreshCatalog;
            }
        }

        // ✅ Метод обновления каталога
        private void RefreshCatalog()
        {
            System.Diagnostics.Debug.WriteLine("[Catalog] RefreshCatalog called");

            if (_vm != null)
            {
                Dispatcher.Invoke(() =>
                {
                    _vm.LoadAsync();
                    System.Diagnostics.Debug.WriteLine("[Catalog] Data reloaded");
                });
            }
        }

        private void SetupRoleBasedAccess()
        {
            var currentUser = DataStorage.CurrentUser;
            if (currentUser == null)
            {
                MessageBox.Show("❌ Ошибка системы: Пользователь не найден.\nПожалуйста, перезапустите приложение и войдите заново.",
                    "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var userRole = currentUser.UserRole;
            System.Diagnostics.Debug.WriteLine($"[CATALOG] Роль пользователя: {userRole}");

            // Кнопка добавления товара
            if (!Permissions.CanAddProduct(userRole))
            {
                if (BtnAddProduct != null)
                    BtnAddProduct.Visibility = Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine("[CATALOG] ❌ Кнопка добавления товара скрыта");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[CATALOG] ✅ Кнопка добавления товара видна");
            }
        }

        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_vm != null && sender is ComboBox cb && cb.SelectedItem is ComboBoxItem item)
            {
                if (int.TryParse(item.Tag?.ToString(), out int categoryId))
                {
                    _vm.SelectedCategory = categoryId;
                }
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = DataStorage.CurrentUser;
            if (currentUser == null)
            {
                MessageBox.Show("❌ Ошибка системы: Пользователь не найден.",
                    "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Permissions.CanAddProduct(currentUser.UserRole))
            {
                MessageBox.Show("❌ У вас нет прав для добавления товаров!",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var addWindow = new AddProductWindow();
            addWindow.Owner = Application.Current.MainWindow;

            if (addWindow.ShowDialog() == true)
            {
                _vm?.LoadAsync();
                MessageBox.Show("Товар успешно добавлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddToOrder_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = DataStorage.CurrentUser;
            if (currentUser == null)
            {
                MessageBox.Show("❌ Ошибка системы: Пользователь не найден.",
                    "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Permissions.CanCreateOrder(currentUser.UserRole))
            {
                MessageBox.Show("❌ У вас нет прав для создания заказов!",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ProductsGrid.SelectedItem is Product selectedProduct)
            {
                try
                {
                    // ✅ Получаем АКТУАЛЬНЫЙ товар из БД
                    var freshProduct = DatabaseService.GetProducts()
                        .FirstOrDefault(p => p.Id == selectedProduct.Id);

                    if (freshProduct == null)
                    {
                        MessageBox.Show("Товар не найден в базе данных!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var dialog = new QuantityInputDialog(
                        productName: freshProduct.Name,
                        currentStock: freshProduct.Stock
                    );

                    dialog.Owner = Application.Current.MainWindow;
                    bool? result = dialog.ShowDialog();

                    if (result == true && dialog.ResultQuantity.HasValue)
                    {
                        if (App.GlobalOrderVM == null)
                        {
                            MessageBox.Show("Ошибка: Система заказов не инициализирована!",
                                "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        bool success = App.GlobalOrderVM.AddToCart(freshProduct, dialog.ResultQuantity.Value);

                        if (success)
                        {
                            MessageBox.Show($"✅ Добавлено: {dialog.ResultQuantity.Value} шт.\n" +
                                $"Остаток на складе: {freshProduct.Stock} шт.",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                            // ✅ Обновляем каталог
                            RefreshCatalog();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении товара:\n" + ex.Message,
                        "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("️ Выберите товар из таблицы!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}