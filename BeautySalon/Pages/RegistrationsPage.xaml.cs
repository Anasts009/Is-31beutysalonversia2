using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BeautySalon.Model;
using BeautySalon.Models;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Pages
{
    public partial class RegistrationsPage : Page
    {
        private readonly BeautySalonContext _context = new();
        private List<RegistrationViewModel> _all = new();

        // быстрый диапазон по датам (если активирован кнопками)
        private DateTime? _rangeFrom;
        private DateTime? _rangeTo;

        public RegistrationsPage()
        {
            InitializeComponent();
            Loaded += RegistrationsPage_Loaded;
        }

        private void RegistrationsPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Проверяем, авторизован ли пользователь и является ли он администратором
            bool isAdmin = App.CurrentUser != null && App.CurrentUser.Role?.RoleName == "Администратор";

            // Для отладки: показываем текущую роль
            //MessageBox.Show($"Текущий пользователь: {App.CurrentUser?.Login}, Роль: {App.CurrentUser?.Role?.RoleName}");

            AddButton.Visibility = isAdmin ? Visibility.Collapsed : Visibility.Visible;

            // Если у вас есть колонка "Действия" (удаление), можно тоже скрыть её для обычных пользователей
            if (ActionsColumn != null)
                ActionsColumn.Visibility = isAdmin ? Visibility.Collapsed : Visibility.Visible;

            LoadRegistrations();
            ApplyFilterAndSort();
        }

        private void LoadRegistrations()
        {
            var regs = _context.Registrations
                .Include(r => r.Client)
                .Include(r => r.Employee)
                .Include(r => r.Services)
                .Include(r => r.Payment)
                .AsNoTracking()
                .OrderByDescending(r => r.Date)
                .ToList();

            _all = regs.Select(r => new RegistrationViewModel
            {
                RegistrationId = r.RegistrationId,
                ClientName = $"{r.Client.Lastname} {r.Client.Firstname}",
                EmployeeName = $"{r.Employee.Lastname} {r.Employee.Firstname}",
                Number = r.Number,
                Date = r.Date,
                Services = string.Join(", ", r.Services.Select(s => s.NameServices)),
                TotalSum = r.Payment?.Summ ?? r.Services.Sum(s => s.Price)
            }).ToList();
        }

        private void ApplyFilterAndSort()
        {
            if (RegistrationsGrid == null || StatusText == null) return;

            IEnumerable<RegistrationViewModel> filtered = _all;

            // Поиск
            var search = (SearchBox.Text ?? "").Trim();
            if (!string.IsNullOrEmpty(search) && search != "Поиск по клиенту, сотруднику или номеру...")
            {
                var s = search.ToLower();
                filtered = filtered.Where(x =>
                    (x.ClientName?.ToLower().Contains(s) ?? false) ||
                    (x.EmployeeName?.ToLower().Contains(s) ?? false) ||
                    (x.Number?.ToLower().Contains(s) ?? false));
            }

            // Быстрый диапазон дат имеет приоритет
            if (_rangeFrom.HasValue && _rangeTo.HasValue)
            {
                filtered = filtered.Where(x => x.Date >= _rangeFrom.Value && x.Date < _rangeTo.Value);
            }
            else if (FilterDatePicker.SelectedDate is DateTime date) // одиночная дата
            {
                var start = date.Date;
                var end = start.AddDays(1);
                filtered = filtered.Where(x => x.Date >= start && x.Date < end);
            }

            // Фильтр по сумме
            if (decimal.TryParse(SumFromBox.Text, out var from)) filtered = filtered.Where(x => x.TotalSum >= from);
            if (decimal.TryParse(SumToBox.Text, out var to)) filtered = filtered.Where(x => x.TotalSum <= to);

            // Сортировка
            var sort = (SortBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            filtered = sort switch
            {
                "По дате (новые)" => filtered.OrderByDescending(x => x.Date),
                "По дате (старые)" => filtered.OrderBy(x => x.Date),
                "По клиенту (А-Я)" => filtered.OrderBy(x => x.ClientName),
                "По клиенту (Я-А)" => filtered.OrderByDescending(x => x.ClientName),
                "По сумме (возрастание)" => filtered.OrderBy(x => x.TotalSum),
                "По сумме (убывание)" => filtered.OrderByDescending(x => x.TotalSum),
                _ => filtered.OrderByDescending(x => x.Date)
            };

            RegistrationsGrid.ItemsSource = filtered.ToList();
            StatusText.Text = $"Найдено записей: {filtered.Count()}";
        }

        // Поиск/фильтры
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilterAndSort();
        private void SortBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilterAndSort();
        private void FilterDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // если выбрана конкретная дата — сбросим быстрый диапазон
            _rangeFrom = _rangeTo = null;
            ApplyFilterAndSort();
        }
        private void SumFilter_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilterAndSort();

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Поиск по клиенту, сотруднику или номеру...") SearchBox.Text = "";
        }
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text)) SearchBox.Text = "Поиск по клиенту, сотруднику или номеру...";
        }

        // ЭТО НОВЫЕ обработчики под твой XAML:
        // Кнопки быстрого фильтра по датам: Tag=\"today\" | \"7\" | \"30\" | \"all\"
        private void QuickDateFilter_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button)?.Tag?.ToString();
            switch (tag)
            {
                case "today":
                    _rangeFrom = DateTime.Today;
                    _rangeTo = DateTime.Today.AddDays(1);
                    break;
                case "7":
                    _rangeFrom = DateTime.Today.AddDays(-7);
                    _rangeTo = DateTime.Today.AddDays(1);
                    break;
                case "30":
                    _rangeFrom = DateTime.Today.AddDays(-30);
                    _rangeTo = DateTime.Today.AddDays(1);
                    break;
                default: // "all" или что-то иное
                    _rangeFrom = _rangeTo = null;
                    break;
            }
            // сбрасываем одиночную дату, чтобы не конфликтовала с быстрым диапазоном
            FilterDatePicker.SelectedDate = null;
            ApplyFilterAndSort();
        }

        // Сброс всех фильтров (если есть кнопка "Очистить фильтры")
        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            FilterDatePicker.SelectedDate = null;
            SumFromBox.Text = "";
            SumToBox.Text = "";
            SortBox.SelectedIndex = 0;
            _rangeFrom = _rangeTo = null;
            ApplyFilterAndSort();
        }

        // Удаление записи (кнопка в гриде: Click=\"DeleteButton_Click\" Tag=\"{Binding RegistrationId}\")
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int id = 0;

            if (sender is Button btn && btn.Tag is int tagId)
                id = tagId;
            else if (RegistrationsGrid.SelectedItem is RegistrationViewModel vm)
                id = vm.RegistrationId;

            if (id == 0) return;

            if (MessageBox.Show("Удалить запись?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                var reg = _context.Registrations
                    .Include(r => r.Services)
                    .Include(r => r.Payment)
                    .FirstOrDefault(r => r.RegistrationId == id);

                if (reg == null)
                {
                    MessageBox.Show("Запись не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // убираем связи many-to-many
                reg.Services.Clear();

                // удаляем оплату, если есть
                if (reg.Payment != null)
                    _context.Payments.Remove(reg.Payment);

                _context.Registrations.Remove(reg);
                _context.SaveChanges();

                LoadRegistrations();
                ApplyFilterAndSort();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось удалить запись: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditRegistrationPage());
        }
    }
}