using System.Windows;
using System.Windows.Input;

namespace MOBM_App.Views
{
    public partial class QuantityInputDialog : Window
    {
        public int? ResultQuantity { get; private set; }

        // Конструктор для ДОБАВЛЕНИЯ из каталога
        public QuantityInputDialog(string productName, int currentStock)
        {
            InitializeComponent();

            TxtTitle.Text = $"Товар: {productName}";
            TxtInfo.Text = $"В заказе сейчас: 0 шт.\nМаксимум доступно: {currentStock} шт.";
            TxtQuantity.Text = "1";

            this.Loaded += (_, __) =>
            {
                TxtQuantity.SelectAll();
                TxtQuantity.Focus();
            };
        }

        // Конструктор для ИЗМЕНЕНИЯ в заказе
        public QuantityInputDialog(string productName, int maxAvailable, string labelOverride)
        {
            InitializeComponent();

            TxtTitle.Text = $"Товар: {productName}";
            TxtInfo.Text = labelOverride;
            TxtQuantity.Text = "1";

            this.Loaded += (_, __) =>
            {
                TxtQuantity.SelectAll();
                TxtQuantity.Focus();
            };
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtQuantity.Text, out int qty) && qty > 0)
            {
                ResultQuantity = qty;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Введите корректное положительное число!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtQuantity.SelectAll();
                TxtQuantity.Focus();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void TxtQuantity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnOk_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}