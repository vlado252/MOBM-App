using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MOBM_App.Models;
using MOBM_App.Services;

namespace MOBM_App.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Product> _products;
        private ObservableCollection<Product> _filteredProducts;
        private string _search;
        private int _selectedCategory = -1; // -1 = все категории

        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public ObservableCollection<Product> FilteredProducts
        {
            get => _filteredProducts;
            set
            {
                _filteredProducts = value;
                OnPropertyChanged();
            }
        }

        // ✅ Свойство поиска
        public string Search
        {
            get => _search;
            set
            {
                _search = value;
                OnPropertyChanged();
                ApplyFilters(); // ✅ Фильтруем при каждом изменении
            }
        }

        public int SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public ICommand SearchCommand { get; }

        public MainViewModel()
        {
            Products = new ObservableCollection<Product>();
            FilteredProducts = new ObservableCollection<Product>();
            SearchCommand = new RelayCommand(_ => ApplyFilters());

            LoadAsync();
        }

        /// <summary>
        /// Загрузка данных из БД
        /// </summary>
        public async void LoadAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MainViewModel] LoadAsync START");

                var products = await Task.Run(() => DatabaseService.GetProducts());

                Products = new ObservableCollection<Product>(products);

                System.Diagnostics.Debug.WriteLine($"[MainViewModel] Loaded {Products.Count} products");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainViewModel] LoadAsync ERROR: {ex.Message}");
                System.Windows.MessageBox.Show(
                    "Ошибка загрузки данных:\n" + ex.Message,
                    "Ошибка",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Применение фильтров (поиск + категория)
        /// </summary>
        private void ApplyFilters()
        {
            if (_products == null)
            {
                FilteredProducts = new ObservableCollection<Product>();
                return;
            }

            var query = _products.AsEnumerable();

            // ✅ Фильтр по поиску (регистронезависимый)
            if (!string.IsNullOrWhiteSpace(_search))
            {
                string searchLower = _search.Trim().ToLower();
                query = query.Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(searchLower)) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchLower)) ||
                    (p.Material != null && p.Material.ToLower().Contains(searchLower)));
            }

            // ✅ Фильтр по категории
            if (_selectedCategory >= 0)
            {
                query = query.Where(p => (int)p.Category == _selectedCategory);
            }

            FilteredProducts = new ObservableCollection<Product>(query);

            System.Diagnostics.Debug.WriteLine(
                $"[MainViewModel] ApplyFilters: Search='{_search}', Category={_selectedCategory}, Found={FilteredProducts.Count}");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}