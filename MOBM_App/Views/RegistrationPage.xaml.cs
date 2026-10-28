using System.Windows;
using System.Windows.Controls;
using MOBM_App.Helpers;
using MOBM_App.Models;
using MOBM_App.Services;

namespace MOBM_App.Views
{
    public partial class RegistrationPage : Window
    {
        public RegistrationPage()
        {
            InitializeComponent();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFullName.Text))
            {
                MessageBox.Show("Введите ФИО!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtFullName.Focus();
                return;
            }

            if (!Helpers.Validators.IsValidLogin(TxtLogin.Text))
            {
                MessageBox.Show("Некорректный логин!\nДопустимо: 3-20 символов, латиница и цифры",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtLogin.Focus();
                return;
            }

            if (!Helpers.Validators.IsValidPassword(TxtPassword.Password))
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtPassword.Focus();
                return;
            }

            if (!Helpers.Validators.IsValidEmail(TxtEmail.Text))
            {
                MessageBox.Show("Введите корректный email!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtEmail.Focus();
                return;
            }

            if (!Helpers.Validators.IsValidPhone(TxtPhone.Text))
            {
                MessageBox.Show("Введите корректный телефон!\nФормат: +375 XX XXX-XX-XX",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtPhone.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtPosition.Text))
            {
                MessageBox.Show("Укажите желаемую должность!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtPosition.Focus();
                return;
            }

            try
            {
                int roleId = 2;
                if (CmbRole.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
                {
                    int.TryParse(selectedItem.Tag.ToString(), out roleId);
                }

                DatabaseService.SubmitEmployeeRequest(
                    TxtFullName.Text.Trim(),
                    TxtLogin.Text.Trim(),
                    TxtPassword.Password,
                    TxtEmail.Text.Trim(),
                    TxtPhone.Text.Trim(),
                    TxtPosition.Text.Trim(),
                    roleId
                );

                MessageBox.Show("✅ Заявка успешно отправлена!\n\n" +
                    "Администратор рассмотрит её в ближайшее время.\n" +
                    "После подтверждения вы получите уведомление на email.",
                    "Заявка отправлена", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка при отправке заявки:\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}