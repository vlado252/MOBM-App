using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MOBM_App.Views
{
    public partial class StockOperationWindow : Window
    {
        public int? ResultQuantity { get; private set; }
        public string Comment { get; private set; }
        private int _currentStock;
        private string _operationType;

        public StockOperationWindow(string productName, int currentStock, string operationType)
        {
            InitializeComponent();
            _currentStock = currentStock;
            _operationType = operationType;

            // Настраиваем окно в зависимости от типа операции
            if (operationType == "Приход")
            {
                // Цвета для прихода (зелёные)
                TxtIcon.Text = "📥";
                TxtTitle.Text = "Приход товара";
                TxtOperationType.Text = "ПРИХОД";
                TxtOperationType.Foreground = new SolidColorBrush(Color.FromRgb(0, 230, 118));
                BorderColor.Color = Color.FromRgb(0, 230, 118);
                IconBgColor.Color = Color.FromRgb(0, 230, 118);
                BtnConfirm.Background = new SolidColorBrush(Color.FromRgb(0, 230, 118));
                TxtHint.Text = "💡 Приход увеличивает остаток на складе. Используйте при получении товара от поставщика.";
            }
            else
            {
                // Цвета для расхода (оранжевые)
                TxtIcon.Text = "📤";
                TxtTitle.Text = "Расход товара";
                TxtOperationType.Text = "РАСХОД";
                TxtOperationType.Foreground = new SolidColorBrush(Color.FromRgb(255, 145, 0));
                BorderColor.Color = Color.FromRgb(255, 145, 0);
                IconBgColor.Color = Color.FromRgb(255, 145, 0);
                BtnConfirm.Background = new SolidColorBrush(Color.FromRgb(255, 145, 0));
                TxtHint.Text = "💡 Расход уменьшает остаток на складе. Используйте при списании или использовании товара.";
            }

            TxtProductName.Text = productName;
            TxtCurrentStock.Text = currentStock + " шт.";
            UpdateNewStockPreview();

            Loaded += (s, e) => TxtQuantity.Focus();
        }

        private void TxtQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateNewStockPreview();
        }

        private void UpdateNewStockPreview()
        {
            if (int.TryParse(TxtQuantity.Text, out int qty) && qty > 0)
            {
                int newStock = _operationType == "Приход"
                    ? _currentStock + qty
                    : _currentStock - qty;

                if (newStock < 0)
                {
                    TxtNewStock.Text = "⚠️ Недостаточно на складе!";
                    TxtNewStock.Foreground = new SolidColorBrush(Color.FromRgb(255, 23, 68));
                    BtnConfirm.IsEnabled = false;
                }
                else
                {
                    string sign = _operationType == "Приход" ? "+" : "";
                    TxtNewStock.Text = "Новый остаток: " + newStock + " шт. (" + sign + qty + ")";
                    TxtNewStock.Foreground = new SolidColorBrush(
                        Color.FromRgb(_operationType == "Приход" ? (byte)0 : (byte)255,
                                     _operationType == "Приход" ? (byte)230 : (byte)145,
                                     (byte)0));
                    BtnConfirm.IsEnabled = true;
                }
            }
            else
            {
                TxtNewStock.Text = "Новый остаток: " + _currentStock + " шт.";
                TxtNewStock.Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 176));
                BtnConfirm.IsEnabled = false;
            }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtQuantity.Text, out int qty) && qty > 0)
            {
                ResultQuantity = qty;
                Comment = TxtComment.Text != null ? TxtComment.Text.Trim() : "";
                DialogResult = true;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}