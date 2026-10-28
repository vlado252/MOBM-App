using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MOBM_App.Helpers;
using MOBM_App.Models;
using MOBM_App.Services;

namespace MOBM_App.Views.Pages
{
    public partial class UsersManagementPage : Page
    {
        public UsersManagementPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                var users = DatabaseService.GetAllUsers();

                // Добавляем RoleName для отображения
                var displayUsers = users.Select(u => new
                {
                    u.Id,
                    u.Login,
                    u.Password,
                    u.Name,
                    u.Email,
                    u.UserRole,
                    RoleName = u.UserRole.ToString()
                }).ToList();

                UsersGrid.ItemsSource = displayUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки пользователей: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedItem = UsersGrid.SelectedItem;
                var userId = (int)selectedItem.GetType().GetProperty("Id").GetValue(selectedItem);

                // Получаем полные данные пользователя
                var user = DatabaseService.GetAllUsers().FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    var editWindow = new EditUserWindow(user);
                    editWindow.Owner = Window.GetWindow(this);

                    if (editWindow.ShowDialog() == true)
                    {
                        LoadUsers();
                        MessageBox.Show("Пользователь успешно обновлен!", "Успех",
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
            if (UsersGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя для удаления!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedItem = UsersGrid.SelectedItem;
                var userId = (int)selectedItem.GetType().GetProperty("Id").GetValue(selectedItem);
                var userName = selectedItem.GetType().GetProperty("Name").GetValue(selectedItem)?.ToString();

                // Проверка: нельзя удалить самого себя
                if (userId == DataStorage.CurrentUser?.Id)
                {
                    MessageBox.Show("Вы не можете удалить свою собственную учетную запись!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить пользователя:\n\n{userName}?\n\n" +
                    "Все связанные заказы также будут удалены!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DatabaseService.DeleteUser(userId);
                    LoadUsers();
                    MessageBox.Show("Пользователь удален!", "Успех",
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