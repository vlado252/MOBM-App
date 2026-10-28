using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MOBM_App.Models
{
    public class Product : IComparable<Product>, INotifyPropertyChanged
    {
        private int _stock;
        public int Stock
        {
            get => _stock;
            set => Set(ref _stock, value);
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public Category Category { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Material { get; set; }
        public string Dimensions { get; set; }

        public string CategoryName
        {
            get
            {
                switch (Category)
                {
                    case Category.Furniture: return "Мебель";
                    case Category.RawMaterial: return "Сырьё";
                    default: return "Неизвестно";
                }
            }
        }

        public int CompareTo(Product other) => this.Price.CompareTo(other.Price);

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}