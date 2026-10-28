using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MOBM_App.Models
{
    // Класс для элемента в корзине/заказе
    public class OrderItem : INotifyPropertyChanged
    {
        private Product _product;
        private int _quantity;

        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Sum));
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Sum));
                }
            }
        }

        public decimal Sum => Product != null ? Product.Price * Quantity : 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Класс промокода
    public class PromoCode
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTime ValidUntil { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
    }

    // Класс применённой скидки
    public class AppliedDiscount
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public decimal Percent { get; set; }
        public decimal Amount { get; set; }
    }

    // Класс для отображения скидки в UI
    public class DiscountDisplay
    {
        public string Name { get; set; }
        public string AmountText { get; set; }
    }

    // Класс заказа
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Status { get; set; }
        public string UserName { get; set; }
        public string AppliedPromoCode { get; set; }

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public List<AppliedDiscount> Discounts { get; set; } = new List<AppliedDiscount>();
    }
}