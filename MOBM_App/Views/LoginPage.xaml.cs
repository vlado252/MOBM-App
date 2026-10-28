using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MOBM_App.Helpers;
using MOBM_App.Models;
using MOBM_App.Services;

namespace MOBM_App.Views
{
    public partial class LoginPage : Window
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text?.Trim();
            string password = TxtPassword.Password;

            // Валидация
            if (!Helpers.Validators.IsValidLogin(login))
            {
                MessageBox.Show("Некорректный логин!\nДопустимо: 3-20 символов, латиница и цифры",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtLogin.Focus();
                return;
            }

            if (!Helpers.Validators.IsValidPassword(password))
            {
                MessageBox.Show("Пароль должен содержать минимум 3 символа!",
     "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtPassword.Focus();
                return;
            }

            try
            {
                // Поиск пользователя в базе
                var users = DatabaseService.GetUsers();
                var user = users.FirstOrDefault(u => u.Login == login && u.Password == password);

                if (user != null)
                {
                    // ✅ Сохраняем пользователя в глобальное хранилище
                    DataStorage.CurrentUser = user;

                    System.Diagnostics.Debug.WriteLine($"[AUTH] ✅ Вход: {user.Name}, Роль: {user.UserRole}");

                    // Открываем главное окно
                    var mainWindow = new MainWindow();
                    mainWindow.Show();

                    // Закрываем окно входа
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!", "Ошибка входа",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка входа: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ Обработчик нажатия Enter в поле пароля
        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnLogin_Click(sender, e);
            }
        }

        private void BtnRegistration_Click(object sender, RoutedEventArgs e)
        {
            var registrationWindow = new RegistrationPage();
            registrationWindow.Owner = this;
            registrationWindow.ShowDialog();
        }
    }
}