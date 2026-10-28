using System;
using System.Windows;
using System.Windows.Controls;
using MOBM_App.Helpers;
using MOBM_App.Models;
using MOBM_App.Services;

namespace MOBM_App.Views
{
    public partial class EditUserWindow : Window
    {
        private User _user;

        public EditUserWindow(User user)
        {
            InitializeComponent();
            _user = user;
            LoadUserData();
        }

        private void LoadUserData()
        {
            TxtUserId.Text = $"ID: {_user.Id}";
            TxtLogin.Text = _user.Login;
            TxtPassword.Password = _user.Password;
            TxtName.Text = _user.Name;
            TxtEmail.Text = _user.Email;

            // Устанавливаем выбранную роль
            CmbRole.SelectedIndex = (int)_user.UserRole;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                string login = TxtLogin.Text?.Trim();
                string password = TxtPassword.Password;
                string name = TxtName.Text?.Trim();
                string email = TxtEmail.Text?.Trim();

                if (string.IsNullOrWhiteSpace(login) || login.Length < 3)
                {
                    MessageBox.Show("Логин должен содержать минимум 3 символа!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(password) || password.Length < 3)
                {
                    MessageBox.Show("Пароль должен содержать минимум 3 символа!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("ФИО не может быть пустым!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Валидация email (если указан)
                if (!string.IsNullOrWhiteSpace(email) && !Helpers.Validators.IsValidEmail(email))
                {
                    MessageBox.Show("Некорректный формат email!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем выбранную роль
                var selectedRole = (ComboBoxItem)CmbRole.SelectedItem;
                int roleId = int.Parse(selectedRole.Tag.ToString());

                // Обновляем пользователя
                _user.Login = login;
                _user.Password = password;
                _user.Name = name;
                _user.Email = email;
                _user.UserRole = (Role)roleId;

                DatabaseService.UpdateUser(_user);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}