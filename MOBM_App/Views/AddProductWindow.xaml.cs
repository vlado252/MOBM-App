using System;
using System.Windows;
using System.Windows.Controls;
using MOBM_App.Models;
using MOBM_App.Services;

namespace MOBM_App.Views
{
    public partial class AddProductWindow : Window
    {
        public AddProductWindow()
        {
            InitializeComponent();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Введите название товара!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return;
            }

            if (!decimal.TryParse(TxtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtPrice.Focus();
                return;
            }

            if (!int.TryParse(TxtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Введите корректное количество!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtStock.Focus();
                return;
            }

            int categoryId = 0;
            if (CmbCategory.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                int.TryParse(selectedItem.Tag.ToString(), out categoryId);
            }

            try
            {
                var newProduct = new Product
                {
                    Name = TxtName.Text.Trim(),
                    Category = (Category)categoryId,
                    Price = price,
                    Stock = stock,
                    Description = TxtDescription.Text.Trim(),
                    Material = TxtMaterial.Text.Trim(),
                    Dimensions = TxtDimensions.Text.Trim()
                };

                DatabaseService.AddProduct(newProduct);

                MessageBox.Show($"✅ Товар '{newProduct.Name}' успешно добавлен!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка добавления товара:\n{ex.Message}",
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