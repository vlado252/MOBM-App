using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MOBM_App.Helpers;
using MOBM_App.Models;
using MOBM_App.Views.Pages;

namespace MOBM_App.Views
{
    public partial class MainWindow : Window
    {
        private Button _activeButton;

        public MainWindow()
        {
            InitializeComponent();

            // ✅ ПРОВЕРКА: Если пользователь не авторизован, возвращаемся на вход
            if (DataStorage.CurrentUser == null)
            {
                MessageBox.Show("Ошибка: Пользователь не авторизован!\nПеренаправление на страницу входа...",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                var loginPage = new LoginPage();
                loginPage.Show();
                this.Close();
                return;
            }

            SetupRoleBasedNavigation();
            MainFrame.Navigate(new CatalogPage());
            SetActiveButton(BtnCatalog);

            if (AppStatusBar != null)
            {
                AppStatusBar.UpdateStatusByRoleAndSection("Каталог товаров");
            }

            System.Diagnostics.Debug.WriteLine($"[MAIN] ✅ MainWindow открыт для: {DataStorage.CurrentUser.Name}");
        }

        // Настройка видимости элементов по ролям
        private void SetupRoleBasedNavigation()
        {
            var currentUser = DataStorage.CurrentUser;

            System.Diagnostics.Debug.WriteLine($"[MAIN] CurrentUser: {currentUser?.Name ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"[MAIN] UserRole: {currentUser?.UserRole.ToString() ?? "null"}");

            if (currentUser == null)
            {
                System.Diagnostics.Debug.WriteLine("[MAIN] ❌ Пользователь не авторизован!");
                return;
            }

            var userRole = currentUser.UserRole;
            // Показываем кнопку управления пользователями только для админа
            if (userRole == Role.Admin)
            {
                BtnUsersManagement.Visibility = Visibility.Visible;
                System.Diagnostics.Debug.WriteLine("[MAIN] ✅ Кнопка пользователей показана (Admin)");
            }
            else
            {
                BtnUsersManagement.Visibility = Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine($"[MAIN] ❌ Кнопка пользователей скрыта ({userRole})");
            }
            // Показываем кнопку промокодов только для админа
            if (userRole == Role.Admin)
            {
                BtnPromoCodes.Visibility = Visibility.Visible;
                System.Diagnostics.Debug.WriteLine("[MAIN] ✅ Кнопка промокодов показана (Admin)");
            }
            else
            {
                BtnPromoCodes.Visibility = Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine($"[MAIN] ❌ Кнопка промокодов скрыта ({userRole})");
            }
            // Показываем кнопку заявок только для админа
            if (userRole == Role.Admin)
            {
                BtnEmployeeRequests.Visibility = Visibility.Visible;
                System.Diagnostics.Debug.WriteLine("[MAIN] ✅ Кнопка заявок показана (Admin)");
            }
            else
            {
                BtnEmployeeRequests.Visibility = Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine($"[MAIN] ❌ Кнопка заявок скрыта ({userRole})");
            }
        }

        // Подсветка активной кнопки
        private void SetActiveButton(Button button)
        {
            foreach (var btn in new[] { BtnCatalog, BtnOrder, BtnWarehouse, BtnHistory, BtnDashboard, BtnEmployeeRequests, BtnUsersManagement, BtnPromoCodes })
            {
                if (btn != null)
                {
                    btn.Background = Brushes.Transparent;
                }
            }

            if (button != null)
            {
                button.Background = new SolidColorBrush(Color.FromRgb(41, 121, 255));
                _activeButton = button;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                if (MainFrame.Content is Page currentPage)
                {
                    MainFrame.Navigate(currentPage);
                    AppStatusBar?.SetStatus("🔄 Данные обновлены (F5)");
                }
                e.Handled = true;
            }
        }

        private void NavCatalog_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CatalogPage());
            SetActiveButton(BtnCatalog);
            AppStatusBar?.UpdateStatusByRoleAndSection("Каталог товаров");
        }

        private void NavCurrentOrder_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = DataStorage.CurrentUser;
            if (currentUser != null && !Permissions.CanCreateOrder(currentUser.UserRole))
            {
                MessageBox.Show("❌ У вас нет прав для создания заказов!",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ✅ ВАЖНО: Открываем OrderPage (а не CurrentOrderPage)
            MainFrame.Navigate(new OrderPage());
            SetActiveButton(BtnOrder);

            if (AppStatusBar != null)
            {
                AppStatusBar.UpdateStatusByRoleAndSection("Оформление заказа");
            }
        }

        private void NavWarehouse_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new WarehousePage());
            SetActiveButton(BtnWarehouse);
            AppStatusBar?.UpdateStatusByRoleAndSection("Управление складом");
        }

        private void NavHistory_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new HistoryPage());
            SetActiveButton(BtnHistory);
            AppStatusBar?.UpdateStatusByRoleAndSection("История заказов");
        }

        private void NavDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
            SetActiveButton(BtnDashboard);
            AppStatusBar?.UpdateStatusByRoleAndSection("Аналитика и статистика");
        }

        private void NavEmployeeRequests_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = DataStorage.CurrentUser;
            if (currentUser == null || currentUser.UserRole != Role.Admin)
            {
                MessageBox.Show("❌ Только администратор может просматривать заявки!",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MainFrame.Navigate(new EmployeeRequestsPage());
            SetActiveButton(BtnEmployeeRequests);
            AppStatusBar?.UpdateStatusByRoleAndSection("Заявки на трудоустройство");
        }
        private void NavPromoCodes_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = DataStorage.CurrentUser;
            if (currentUser == null || currentUser.UserRole != Role.Admin)
            {
                MessageBox.Show("❌ Только администратор может управлять промокодами!",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MainFrame.Navigate(new Pages.PromoCodesPage());
            SetActiveButton(BtnPromoCodes);
            AppStatusBar?.UpdateStatusByRoleAndSection("Управление промокодами");
        }
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow { Owner = this };
            helpWindow.ShowDialog();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow { Owner = this };
            aboutWindow.ShowDialog();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Выйти из системы?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // ✅ Очищаем данные пользователя при выходе
                DataStorage.CurrentUser = null;

                var loginPage = new LoginPage();
                loginPage.Show();
                this.Close();
            }
        }

        private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AppStatusBar?.SetStatus("🔍 Используйте поле поиска (Ctrl+F)");
            MessageBox.Show("Используйте поле поиска на текущей странице", "Поиск",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AppStatusBar?.SetStatus("💾 Данные сохранены (Ctrl+S)");
            MessageBox.Show("Данные сохранены автоматически", "Сохранение",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NavCurrentOrder_Click(sender, e);
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show("Выйти из приложения?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
        private void NavUsersManagement_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = DataStorage.CurrentUser;
            if (currentUser == null || currentUser.UserRole != Role.Admin)
            {
                MessageBox.Show("❌ Только администратор может управлять пользователями!",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MainFrame.Navigate(new Pages.UsersManagementPage());
            SetActiveButton(BtnUsersManagement);
            AppStatusBar?.UpdateStatusByRoleAndSection("Управление пользователями");
        }
        private void Help_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Help_Click(sender, e);
        }
    }
}