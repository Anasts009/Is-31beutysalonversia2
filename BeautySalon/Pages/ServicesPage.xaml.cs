using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BeautySalon.Model;
using BeautySalon.Models;
using BeautySalon.Windows;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Pages
{
    public partial class ServicesPage : Page
    {
        private readonly BeautySalonContext _context = new();
        private List<Service> _allServices = new();

        public ServicesPage()
        {
            InitializeComponent();
            Loaded += ServicesPage_Loaded;
        }

        private void ServicesPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadServices();
            ApplyFilterAndSort();
        }

        private void LoadServices()
        {
            _allServices = _context.Services
                .AsNoTracking()
                .OrderBy(s => s.NameServices)
                .ToList();
        }

        private void ApplyFilterAndSort()
        {
            // Защита от вызова до инициализации элементов управления
            if (ServicesGrid == null || StatusText == null) return;

            IEnumerable<Service> filtered = _allServices;

            // Поиск
            var search = (SearchBox.Text ?? "").Trim();
            if (!string.IsNullOrEmpty(search) && search != "Поиск по названию или описанию...")
            {
                var s = search.ToLower();
                filtered = filtered.Where(x =>
                    (!string.IsNullOrEmpty(x.NameServices) && x.NameServices.ToLower().Contains(s)) ||
                    (!string.IsNullOrEmpty(x.DescriptionService) && x.DescriptionService!.ToLower().Contains(s)));
            }

            // Фильтр по цене
            if (decimal.TryParse(PriceFromBox.Text, out var priceFrom))
                filtered = filtered.Where(x => x.Price >= priceFrom);
            if (decimal.TryParse(PriceToBox.Text, out var priceTo))
                filtered = filtered.Where(x => x.Price <= priceTo);

            // Сортировка
            var sort = (SortBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            filtered = sort switch
            {
                "По названию (А-Я)" => filtered.OrderBy(x => x.NameServices),
                "По названию (Я-А)" => filtered.OrderByDescending(x => x.NameServices),
                "По цене (возрастание)" => filtered.OrderBy(x => x.Price),
                "По цене (убывание)" => filtered.OrderByDescending(x => x.Price),
                "По времени (возрастание)" => filtered.OrderBy(x => x.Time),
                "По времени (убывание)" => filtered.OrderByDescending(x => x.Time),
                _ => filtered.OrderBy(x => x.NameServices)
            };

            var viewModels = filtered.Select(s => new ServiceViewModel
            {
                ServiceId = s.ServiceId,
                NameServices = s.NameServices,
                DescriptionService = s.DescriptionService,
                Price = s.Price,
                Time = s.Time
            }).ToList();

            ServicesGrid.ItemsSource = viewModels;
            StatusText.Text = $"Найдено услуг: {viewModels.Count}";
        }

        // События UI
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilterAndSort();
        private void PriceFilter_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilterAndSort();
        private void SortBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilterAndSort();

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Поиск по названию или описанию...")
                SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                SearchBox.Text = "Поиск по названию или описанию...";
        }

        private void QuickFilter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            switch (btn.Tag?.ToString())
            {
                case "all":
                    PriceFromBox.Text = "";
                    PriceToBox.Text = "";
                    break;
                case "cheap":
                    PriceFromBox.Text = "";
                    PriceToBox.Text = "1000";
                    break;
                case "medium":
                    PriceFromBox.Text = "1000";
                    PriceToBox.Text = "5000";
                    break;
                case "expensive":
                    PriceFromBox.Text = "5000";
                    PriceToBox.Text = "";
                    break;
            }
            ApplyFilterAndSort();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new ServiceEditWindow();
            if (win.ShowDialog() == true)
            {
                LoadServices();
                ApplyFilterAndSort();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int id) return;

            var service = _context.Services.Find(id);
            if (service == null)
            {
                MessageBox.Show("Услуга не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var win = new ServiceEditWindow(service);
            if (win.ShowDialog() == true)
            {
                LoadServices();
                ApplyFilterAndSort();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int id) return;

            if (MessageBox.Show("Удалить услугу?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var service = _context.Services.Find(id);
            if (service == null)
            {
                MessageBox.Show("Услуга не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                _context.Services.Remove(service);
                _context.SaveChanges();

                LoadServices();
                ApplyFilterAndSort();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось удалить услугу: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            PriceFromBox.Text = "";
            PriceToBox.Text = "";
            SortBox.SelectedIndex = 0;
            ApplyFilterAndSort();
        }
    }
}