using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using DeliveryService.Data;
using DeliveryService.Helpers;
using DeliveryService.Models;
using DeliveryService.Views;

namespace DeliveryService.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private DatabaseHelper _db;
        
        public ObservableCollection<OrderDisplay> Orders { get; set; } = new ObservableCollection<OrderDisplay>();

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); LoadData(); }
        }

        private string _sortBy = "Id";
        public string SortBy
        {
            get => _sortBy;
            set { _sortBy = value; OnPropertyChanged(); LoadData(); }
        }
        public ObservableCollection<string> SortOptions { get; set; } = new ObservableCollection<string> { "Id", "Address", "Courier" };

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set { _currentPage = value; OnPropertyChanged(); LoadData(); }
        }

        private int _pageSize = 10;
        private int _totalItems = 0;
        public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)_totalItems / _pageSize));

        public OrderDisplay? SelectedOrder { get; set; }

        public RelayCommand NextPageCommand { get; }
        public RelayCommand PrevPageCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ExportCommand { get; }

        public MainViewModel()
        {
            _db = new DatabaseHelper();
            
            try
            {
                _db.InitializeDatabase();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных:\n{ex.Message}\n\nПроверьте строку подключения в DatabaseHelper.cs", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            NextPageCommand = new RelayCommand(_ => { if (CurrentPage < TotalPages) CurrentPage++; });
            PrevPageCommand = new RelayCommand(_ => { if (CurrentPage > 1) CurrentPage--; });
            
            AddCommand = new RelayCommand(_ => AddOrder());
            EditCommand = new RelayCommand(_ => EditOrder(), _ => SelectedOrder != null);
            DeleteCommand = new RelayCommand(_ => DeleteOrder(), _ => SelectedOrder != null);
            ExportCommand = new RelayCommand(_ => ExportToCsv());

            try
            {
                LoadData();
            }
            catch (System.Exception)
            {
                // Игнорируем ошибку загрузки, если отвалилась база
            }
        }

        private void LoadData()
        {
            _totalItems = _db.GetTotalOrdersCount(SearchText);
            OnPropertyChanged(nameof(TotalPages));
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;
            if (CurrentPage < 1) CurrentPage = 1;

            Orders.Clear();
            var list = _db.GetOrders(CurrentPage, _pageSize, SortBy, SearchText);
            foreach (var item in list) Orders.Add(item);
        }

        private void AddOrder()
        {
            var vm = new OrderEditViewModel();
            var win = new OrderEditWindow { DataContext = vm };
            if (win.ShowDialog() == true)
            {
                int courierId = _db.GetOrCreateCourier(vm.CourierName, vm.CourierTransport);
                _db.AddOrder(vm.Address, vm.Status, courierId);
                LoadData();
            }
        }

        private void EditOrder()
        {
            if (SelectedOrder == null) return;
            var vm = new OrderEditViewModel
            {
                Address = SelectedOrder.Address,
                Status = SelectedOrder.Status,
                CourierName = SelectedOrder.CourierName,
                CourierTransport = SelectedOrder.CourierTransport
            };
            var win = new OrderEditWindow { DataContext = vm };
            if (win.ShowDialog() == true)
            {
                int courierId = _db.GetOrCreateCourier(vm.CourierName, vm.CourierTransport);
                _db.UpdateOrder(SelectedOrder.OrderId, vm.Address, vm.Status, courierId);
                LoadData();
            }
        }

        private void DeleteOrder()
        {
            if (SelectedOrder == null) return;
            if (MessageBox.Show("Удалить заказ?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _db.DeleteOrder(SelectedOrder.OrderId);
                LoadData();
            }
        }

        private void ExportToCsv()
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("ID;Адрес;Статус;Курьер;Транспорт");
                foreach (var o in Orders)
                {
                    sb.AppendLine($"{o.OrderId};{o.Address};{o.Status};{o.CourierName};{o.CourierTransport}");
                }
                
                string filePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "OrdersExport.csv");
                System.IO.File.WriteAllText(filePath, sb.ToString(), System.Text.Encoding.UTF8);
                
                MessageBox.Show($"Данные успешно экспортированы на Рабочий стол!\nФайл: {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class OrderEditViewModel : ObservableObject
    {
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = "Ожидает";
        public string CourierName { get; set; } = string.Empty;
        public string CourierTransport { get; set; } = string.Empty;

        public RelayCommand SaveCommand { get; }

        public OrderEditViewModel()
        {
            SaveCommand = new RelayCommand(o => {
                if(o is Window win) win.DialogResult = true;
            }, _ => !string.IsNullOrEmpty(Address) && !string.IsNullOrEmpty(CourierName));
        }
    }
}