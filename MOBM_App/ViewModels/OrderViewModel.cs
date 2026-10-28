using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using MOBM_App.Helpers;
using MOBM_App.Models;
using MOBM_App.Services;

namespace MOBM_App.ViewModels
{
    public class OrderViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<OrderItem> _cartItems;
        private string _promoCodeInput;
        private decimal _subTotal;
        private decimal _discountAmount;
        private decimal _total;
        private string _appliedPromoCode;
        private bool _hasDiscounts;
        private string _discountAmountText = "0,00 BYN";
        private string _totalText = "0,00 BYN";
        private ObservableCollection<DiscountDisplay> _discountsList;

        public ObservableCollection<OrderItem> CartItems
        {
            get => _cartItems;
            set
            {
                _cartItems = value;
                OnPropertyChanged();
                CalculateTotals();
            }
        }

        public string PromoCodeInput
        {
            get => _promoCodeInput;
            set
            {
                _promoCodeInput = value;
                OnPropertyChanged();
            }
        }

        public decimal SubTotal
        {
            get => _subTotal;
            set
            {
                _subTotal = value;
                OnPropertyChanged();
            }
        }

        public decimal DiscountAmount
        {
            get => _discountAmount;
            set
            {
                _discountAmount = value;
                OnPropertyChanged();
            }
        }

        public decimal Total
        {
            get => _total;
            set
            {
                _total = value;
                OnPropertyChanged();
            }
        }

        public string AppliedPromoCode
        {
            get => _appliedPromoCode;
            set
            {
                _appliedPromoCode = value;
                OnPropertyChanged();
            }
        }

        public bool HasDiscounts
        {
            get => _hasDiscounts;
            set
            {
                _hasDiscounts = value;
                OnPropertyChanged();
            }
        }

        public string DiscountAmountText
        {
            get => _discountAmountText;
            set
            {
                _discountAmountText = value;
                OnPropertyChanged();
            }
        }

        public string TotalText
        {
            get => _totalText;
            set
            {
                _totalText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DiscountDisplay> DiscountsList
        {
            get => _discountsList;
            set
            {
                _discountsList = value;
                OnPropertyChanged();
            }
        }

        public ICommand ChangeQuantityCmd { get; }
        public ICommand RemoveCmd { get; }
        public ICommand CheckoutCmd { get; }
        public ICommand ApplyPromoCodeCmd { get; }
        public ICommand ClearPromoCodeCmd { get; }

        public event Action OnCartUpdated;
        public event Action OnStockUpdated;

        public OrderViewModel()
        {
            CartItems = new ObservableCollection<OrderItem>();
            DiscountsList = new ObservableCollection<DiscountDisplay>();

            ChangeQuantityCmd = new RelayCommand(ChangeQuantity);
            RemoveCmd = new RelayCommand(RemoveItem);
            CheckoutCmd = new RelayCommand(Checkout);
            ApplyPromoCodeCmd = new RelayCommand(ApplyPromoCode);
            ClearPromoCodeCmd = new RelayCommand(ClearPromoCode);
        }

        private int GetActualStock(int productId)
        {
            try
            {
                var product = DatabaseService.GetProducts().FirstOrDefault(p => p.Id == productId);
                return product?.Stock ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private void CalculateTotals()
        {
            SubTotal = CartItems.Sum(i => i.Sum);

            decimal totalDiscount = 0;
            var discounts = new ObservableCollection<DiscountDisplay>();

            // Скидка от объёма
            decimal volumeDiscountPercent = 0;
            if (SubTotal >= 10000) volumeDiscountPercent = 10;
            else if (SubTotal >= 5000) volumeDiscountPercent = 5;

            if (volumeDiscountPercent > 0)
            {
                decimal volumeDiscount = SubTotal * volumeDiscountPercent / 100;
                totalDiscount += volumeDiscount;
                discounts.Add(new DiscountDisplay
                {
                    Name = $"📦 Скидка за объём ({volumeDiscountPercent}%)",
                    AmountText = $"-{volumeDiscount:N2} BYN"
                });
            }

            // Сезонная скидка (лето: июнь-август = 5%)
            int currentMonth = DateTime.Now.Month;
            if (currentMonth >= 6 && currentMonth <= 8)
            {
                decimal seasonalDiscount = SubTotal * 0.05m;
                totalDiscount += seasonalDiscount;
                discounts.Add(new DiscountDisplay
                {
                    Name = "☀️ Сезонная скидка (5%)",
                    AmountText = $"-{seasonalDiscount:N2} BYN"
                });
            }

            // Промокод
            if (!string.IsNullOrWhiteSpace(AppliedPromoCode))
            {
                var promo = DatabaseService.ValidatePromoCode(AppliedPromoCode);
                if (promo != null)
                {
                    decimal promoDiscount = SubTotal * promo.DiscountPercent / 100;
                    totalDiscount += promoDiscount;
                    discounts.Add(new DiscountDisplay
                    {
                        Name = $"🎫 Промокод {promo.Code} ({promo.DiscountPercent}%)",
                        AmountText = $"-{promoDiscount:N2} BYN"
                    });
                }
            }

            DiscountAmount = totalDiscount;
            Total = SubTotal - totalDiscount;

            HasDiscounts = totalDiscount > 0;
            DiscountAmountText = $"-{totalDiscount:N2} BYN";
            TotalText = $"{Total:N2} BYN";
            DiscountsList = discounts;

            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(DiscountAmount));
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(AppliedPromoCode));
        }

        private void ApplyPromoCode(object parameter)
        {
            if (string.IsNullOrWhiteSpace(PromoCodeInput))
            {
                MessageBox.Show("Введите промокод!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var promo = DatabaseService.ValidatePromoCode(PromoCodeInput.Trim());

            if (promo == null)
            {
                MessageBox.Show("❌ Промокод не найден или истёк!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AppliedPromoCode = promo.Code.ToUpper();
            CalculateTotals();

            MessageBox.Show($"✅ Промокод применён!\n{promo.Description}\nСкидка: {promo.DiscountPercent}%",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            PromoCodeInput = "";
            OnPropertyChanged(nameof(PromoCodeInput));
        }

        private void ClearPromoCode(object parameter)
        {
            AppliedPromoCode = null;
            CalculateTotals();
        }

        public bool AddToCart(Product product, int quantity)
        {
            if (product == null || quantity <= 0)
            {
                System.Diagnostics.Debug.WriteLine("[AddToCart] ERROR: null product or quantity <= 0");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"\n========== [AddToCart] START ==========");
            System.Diagnostics.Debug.WriteLine($"Product: {product.Name} (ID: {product.Id})");
            System.Diagnostics.Debug.WriteLine($"Quantity requested: {quantity}");

            int actualStock = GetActualStock(product.Id);
            System.Diagnostics.Debug.WriteLine($"Actual stock in DB: {actualStock}");

            var existingItem = CartItems.FirstOrDefault(i => i.Product.Id == product.Id);

            if (existingItem != null)
            {
                int currentInCart = existingItem.Quantity;
                int newTotalInCart = currentInCart + quantity;

                System.Diagnostics.Debug.WriteLine($"\n⚠️ Product ALREADY in cart!");
                System.Diagnostics.Debug.WriteLine($"  Current in cart: {currentInCart}");
                System.Diagnostics.Debug.WriteLine($"  New total needed: {newTotalInCart}");

                if (newTotalInCart > actualStock + currentInCart)
                {
                    int available = actualStock;
                    System.Diagnostics.Debug.WriteLine($"  ❌ Not enough stock! Available: {available}");

                    MessageBox.Show(
                        $"Недостаточно товара на складе!\n\n" +
                        $"Уже в корзине: {currentInCart} шт.\n" +
                        $"Хотите добавить: {quantity} шт.\n" +
                        $"Всего нужно: {newTotalInCart} шт.\n" +
                        $"Доступно на складе: {available} шт.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                existingItem.Quantity = newTotalInCart;
                int newStock = actualStock - quantity;
                DatabaseService.UpdateProductStock(product.Id, newStock);
                product.Stock = newStock;

                System.Diagnostics.Debug.WriteLine($"\n✅ Updated cart quantity: {currentInCart} → {newTotalInCart}");
                System.Diagnostics.Debug.WriteLine($"✅ Stock decreased: {actualStock} → {newStock}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"\n✅ NEW product in cart");

                if (quantity > actualStock)
                {
                    System.Diagnostics.Debug.WriteLine($"  ❌ Not enough stock! Available: {actualStock}, Requested: {quantity}");

                    MessageBox.Show(
                        $"Недостаточно товара на складе!\n\n" +
                        $"Доступно: {actualStock} шт.\n" +
                        $"Хотите добавить: {quantity} шт.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                CartItems.Add(new OrderItem
                {
                    Product = product,
                    Quantity = quantity
                });

                int newStock = actualStock - quantity;
                DatabaseService.UpdateProductStock(product.Id, newStock);
                product.Stock = newStock;

                System.Diagnostics.Debug.WriteLine($"\n✅ Added to cart: {quantity}");
                System.Diagnostics.Debug.WriteLine($"✅ Stock decreased: {actualStock} → {newStock}");
            }

            OnPropertyChanged(nameof(CartItems));
            CalculateTotals();
            OnCartUpdated?.Invoke();
            OnStockUpdated?.Invoke();

            System.Diagnostics.Debug.WriteLine($"========== [AddToCart] END ==========\n");
            return true;
        }

        private void ChangeQuantity(object parameter)
        {
            if (parameter is OrderItem item)
            {
                System.Diagnostics.Debug.WriteLine($"\n========== [ChangeQuantity] START ==========");
                System.Diagnostics.Debug.WriteLine($"Product: {item.Product.Name}");
                System.Diagnostics.Debug.WriteLine($"Current quantity: {item.Quantity}");

                int actualStock = GetActualStock(item.Product.Id);
                int totalAvailable = actualStock + item.Quantity;

                var dialog = new Views.QuantityInputDialog(
                    productName: item.Product.Name,
                    currentStock: totalAvailable
                );
                dialog.Owner = Application.Current.MainWindow;

                if (dialog.ShowDialog() == true && dialog.ResultQuantity.HasValue)
                {
                    int newQuantity = dialog.ResultQuantity.Value;
                    int oldQuantity = item.Quantity;
                    int difference = newQuantity - oldQuantity;

                    System.Diagnostics.Debug.WriteLine($"New quantity: {newQuantity}");
                    System.Diagnostics.Debug.WriteLine($"Difference: {difference}");

                    if (difference > 0)
                    {
                        if (actualStock < difference)
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ Not enough stock! Available: {actualStock}, Need: {difference}");

                            MessageBox.Show(
                                $"Недостаточно товара на складе!\n\n" +
                                $"Доступно: {actualStock} шт.\n" +
                                $"Нужно добавить: {difference} шт.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        int newStock = actualStock - difference;
                        DatabaseService.UpdateProductStock(item.Product.Id, newStock);
                        item.Product.Stock = newStock;

                        System.Diagnostics.Debug.WriteLine($"✅ Stock decreased: {actualStock} → {newStock}");
                    }
                    else if (difference < 0)
                    {
                        int newStock = actualStock + Math.Abs(difference);
                        DatabaseService.UpdateProductStock(item.Product.Id, newStock);
                        item.Product.Stock = newStock;

                        System.Diagnostics.Debug.WriteLine($"✅ Stock increased: {actualStock} → {newStock}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("ℹ️ Quantity not changed");
                        return;
                    }

                    item.Quantity = newQuantity;

                    System.Diagnostics.Debug.WriteLine($"✅ Cart updated: {oldQuantity} → {newQuantity}");
                    System.Diagnostics.Debug.WriteLine($"========== [ChangeQuantity] END ==========\n");

                    OnPropertyChanged(nameof(CartItems));
                    CalculateTotals();
                    OnCartUpdated?.Invoke();
                    OnStockUpdated?.Invoke();
                }
            }
        }

        private void RemoveItem(object parameter)
        {
            if (parameter is OrderItem item)
            {
                System.Diagnostics.Debug.WriteLine($"\n========== [RemoveItem] START ==========");
                System.Diagnostics.Debug.WriteLine($"Product: {item.Product.Name}");
                System.Diagnostics.Debug.WriteLine($"Quantity in cart: {item.Quantity}");

                int actualStock = GetActualStock(item.Product.Id);
                int newStock = actualStock + item.Quantity;
                DatabaseService.UpdateProductStock(item.Product.Id, newStock);
                item.Product.Stock = newStock;

                System.Diagnostics.Debug.WriteLine($"✅ Stock increased: {actualStock} → {newStock}");

                CartItems.Remove(item);

                System.Diagnostics.Debug.WriteLine($"✅ Removed from cart");
                System.Diagnostics.Debug.WriteLine($"========== [RemoveItem] END ==========\n");

                OnPropertyChanged(nameof(CartItems));
                CalculateTotals();
                OnCartUpdated?.Invoke();
                OnStockUpdated?.Invoke();
            }
        }

        private void Checkout(object parameter)
        {
            if (CartItems.Count == 0)
            {
                MessageBox.Show("Добавьте товары в заказ перед оформлением!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var order = new Order
                {
                    UserId = DataStorage.CurrentUser?.Id ?? 0,
                    Date = DateTime.Now,
                    SubTotal = SubTotal,
                    DiscountAmount = DiscountAmount,
                    TotalPrice = Total,
                    AppliedPromoCode = AppliedPromoCode,
                    Status = "Выполнен",
                    UserName = DataStorage.CurrentUser?.Name ?? "Неизвестно",
                    Items = CartItems.ToList()
                };

                int orderId = DatabaseService.SaveOrderHistory(order);

                string discountInfo = DiscountAmount > 0
                    ? $"\nСкидки: {DiscountAmount:N2} BYN"
                    : "";

                MessageBox.Show("✅ Заказ №" + orderId + " успешно оформлен!\n\n" +
                    "Сумма до скидок: " + SubTotal.ToString("N2") + " BYN" +
                    discountInfo +
                    "\nИТОГО: " + Total.ToString("N2") + " BYN\n" +
                    "Товаров: " + order.Items.Count,
                    "Заказ оформлен", MessageBoxButton.OK, MessageBoxImage.Information);

                CartItems.Clear();
                AppliedPromoCode = null;
                OnPropertyChanged(nameof(CartItems));
                CalculateTotals();
                OnCartUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка оформления заказа: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ClearCart()
        {
            System.Diagnostics.Debug.WriteLine($"\n========== [ClearCart] START ==========");

            foreach (var item in CartItems.ToList())
            {
                int actualStock = GetActualStock(item.Product.Id);
                int newStock = actualStock + item.Quantity;

                DatabaseService.UpdateProductStock(item.Product.Id, newStock);
                item.Product.Stock = newStock;

                System.Diagnostics.Debug.WriteLine($"  Return {item.Product.Name}: {actualStock} → {newStock}");
            }

            CartItems.Clear();
            AppliedPromoCode = null;
            OnPropertyChanged(nameof(CartItems));
            CalculateTotals();
            OnCartUpdated?.Invoke();
            OnStockUpdated?.Invoke();

            System.Diagnostics.Debug.WriteLine($"========== [ClearCart] END ==========\n");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}