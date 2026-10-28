using System;
using System.Windows;
using MOBM_App.Models;
using MOBM_App.Services;

namespace MOBM_App.Views
{
    public partial class AddPromoCodeWindow : Window
    {
        private PromoCode _existingPromo;
        private bool _isEditMode;

        public AddPromoCodeWindow()
        {
            InitializeComponent();
            _isEditMode = false;
            // Устанавливаем дату по умолчанию (через месяц)
            TxtValidUntil.Text = DateTime.Now.AddMonths(1).ToString("dd.MM.yyyy");
        }

        public AddPromoCodeWindow(PromoCode promo)
        {
            InitializeComponent();
            _existingPromo = promo;
            _isEditMode = true;
            TxtTitle.Text = "✏️ Редактирование промокода";
            LoadPromoData();
        }

        private void LoadPromoData()
        {
            TxtCode.Text = _existingPromo.Code;
            TxtDiscount.Text = _existingPromo.DiscountPercent.ToString();
            TxtValidUntil.Text = _existingPromo.ValidUntil.ToString("dd.MM.yyyy");
            TxtDescription.Text = _existingPromo.Description;
            ChkActive.IsChecked = _existingPromo.IsActive;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                string code = TxtCode.Text?.Trim().ToUpper();
                string discountText = TxtDiscount.Text?.Trim();
                string description = TxtDescription.Text?.Trim();

                if (string.IsNullOrWhiteSpace(code))
                {
                    MessageBox.Show("Введите код промокода!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(discountText, out decimal discount) || discount <= 0 || discount > 100)
                {
                    MessageBox.Show("Скидка должна быть числом от 1 до 100!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Парсим дату из текста
                if (!DateTime.TryParseExact(TxtValidUntil.Text, "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime validUntil))
                {
                    MessageBox.Show("Неверный формат даты!\nИспользуйте формат: ДД.ММ.ГГГГ\n(например: 31.12.2026)",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (validUntil < DateTime.Now.Date)
                {
                    MessageBox.Show("Дата окончания не может быть в прошлом!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_isEditMode)
                {
                    // Режим редактирования
                    _existingPromo.Code = code;
                    _existingPromo.DiscountPercent = discount;
                    _existingPromo.ValidUntil = validUntil;
                    _existingPromo.Description = description;
                    _existingPromo.IsActive = ChkActive.IsChecked == true;

                    DatabaseService.UpdatePromoCode(_existingPromo);
                }
                else
                {
                    // Режим добавления
                    DatabaseService.AddPromoCode(code, discount, validUntil, description);
                }

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