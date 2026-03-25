using BeautySalon.Model;
using BeautySalon.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BeautySalon.Pages
{
    public partial class AddEditRegistrationPage : Page
    {
        private readonly BeautySalonContext _context = new();
        private List<Service> _allServices;
        private List<Client> _clients;
        private List<Employee> _employees;
        private List<Service> _selectedServices = new();

        public AddEditRegistrationPage()
        {
            InitializeComponent();
            Loaded += AddEditRegistrationPage_Loaded;
        }

        private async void AddEditRegistrationPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
            ServicesList.SelectionChanged += ServicesList_SelectionChanged;
            DatePicker.SelectedDate = DateTime.Today;
            TimeBox.Text = DateTime.Now.ToString("HH:mm");
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            // Загрузка услуг
            _allServices = await _context.Services.ToListAsync();
            ServicesList.ItemsSource = _allServices;

            // Загрузка клиентов
            _clients = await _context.Clients.ToListAsync();
            ClientBox.ItemsSource = _clients;
            ClientBox.DisplayMemberPath = "FullName";  // Убедитесь, что у Client есть свойство FullName
            ClientBox.SelectedValuePath = "ClientId";

            // Загрузка сотрудников (только активные)
            _employees = await _context.Employees
                .Include(e => e.User)
                .Where(e => e.User != null && e.User.Block == false)
                .ToListAsync();

            // Если в модели Employee нет свойства FullName, используем ItemTemplate в XAML
            EmployeeBox.ItemsSource = _employees;
            // Устанавливаем отображение: если FullName есть — используем его, иначе комбинируем Lastname + Firstname
            EmployeeBox.DisplayMemberPath = "FullName";
            if (!_employees.Any() || _employees.First().GetType().GetProperty("FullName") == null)
            {
                // Если нет свойства FullName, используем ItemTemplate через XAML
                // В XAML нужно добавить:
                // <ComboBox.ItemTemplate>
                //    <DataTemplate>
                //        <TextBlock Text="{Binding Lastname} {Binding Firstname}"/>
                //    </DataTemplate>
                // </ComboBox.ItemTemplate>
                // Либо временно добавить свойство FullName в класс Employee
            }

            // Проверка: если сотрудников нет, показываем сообщение
            if (_employees.Count == 0)
            {
                MessageBox.Show("Нет активных сотрудников. Добавьте сотрудников или снимите блокировку.",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            EmployeeBox.SelectedValuePath = "EmployeeId";
        }

        private void ServicesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedServices = ServicesList.SelectedItems.Cast<Service>().ToList();
            decimal total = _selectedServices.Sum(s => s.Price);
            if (TotalTextBlock != null)
            {
                TotalTextBlock.Text = $"Итого: {total} ₽";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (ClientBox.SelectedItem == null)
            {
                ShowError("Выберите клиента.");
                return;
            }
            if (EmployeeBox.SelectedItem == null)
            {
                ShowError("Выберите сотрудника.");
                return;
            }
            if (DatePicker.SelectedDate == null)
            {
                ShowError("Выберите дату.");
                return;
            }
            if (!TimeSpan.TryParse(TimeBox.Text, out var time))
            {
                ShowError("Некорректное время (формат ЧЧ:ММ).");
                return;
            }

            var selectedServices = ServicesList.SelectedItems.Cast<Service>().ToList();
            if (!selectedServices.Any())
            {
                ShowError("Выберите хотя бы одну услугу.");
                return;
            }

            DateTime dateTime = DatePicker.SelectedDate.Value.Date + time;

            try
            {
                // Создаём запись
                var registration = new Registration
                {
                    ClientId = (int)ClientBox.SelectedValue,
                    EmployeeId = (int)EmployeeBox.SelectedValue,
                    Date = dateTime,
                    Number = GenerateNumber()
                };

                // Добавляем услуги
                registration.Services = selectedServices;

                // Создаём оплату
                var totalSum = selectedServices.Sum(s => s.Price);
                var payment = new Payment
                {
                    Summ = totalSum,
                    Registration = registration
                };
                registration.Payment = payment;

                _context.Registrations.Add(registration);
                _context.SaveChanges();

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка сохранения: {ex.Message}");
            }
        }

        private string GenerateNumber()
        {
            // Простая генерация номера: дата + счётчик за день
            var today = DateTime.Today;
            var count = _context.Registrations.Count(r => r.Date >= today && r.Date < today.AddDays(1)) + 1;
            return $"R-{today:yyyyMMdd}-{count:D4}";
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorBorder.Visibility = Visibility.Visible;
        }

        private void TimeBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TimeBox.Text == TimeBox.Tag.ToString())
            {
                TimeBox.Text = "";
                TimeBox.Foreground = Brushes.Black;
            }
        }

        private void TimeBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TimeBox.Text))
            {
                TimeBox.Text = TimeBox.Tag.ToString();
                TimeBox.Foreground = Brushes.Gray;
            }
        }
    }
}