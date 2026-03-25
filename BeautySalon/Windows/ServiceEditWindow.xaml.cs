using System;
using System.Windows;
using BeautySalon.Model;

namespace BeautySalon.Windows
{
    public partial class ServiceEditWindow : Window
    {
        private Service editingService;
        private BeautySalonContext _context;
        private bool isEditMode = false;

        public ServiceEditWindow(Service service = null)
        {
            InitializeComponent();
            _context = new BeautySalonContext();

            if (service != null)
            {
                isEditMode = true;
                editingService = service;
                Title = "Редактирование услуги";
                LoadServiceData();
            }
            else
            {
                isEditMode = false;
                editingService = new Service();
                Title = "Добавление услуги";
            }
        }

        private void LoadServiceData()
        {
            NameBox.Text = editingService.NameServices;
            DescriptionBox.Text = editingService.DescriptionService ?? "";
            PriceBox.Text = editingService.Price.ToString("0.##");

            // Time — минуты
            TimeBox.Text = editingService.Time.ToString();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorBorder.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            ErrorBorder.Visibility = Visibility.Collapsed;
            ErrorText.Text = "";
        }

        private bool ValidateInputs()
        {
            HideError();

            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                ShowError("Введите название услуги.");
                NameBox.Focus();
                return false;
            }

            if (!decimal.TryParse(PriceBox.Text, out var price) || price < 0)
            {
                ShowError("Введите корректную цену (неотрицательное число).");
                PriceBox.Focus();
                return false;
            }

            if (!int.TryParse(TimeBox.Text, out var minutes) || minutes <= 0)
            {
                ShowError("Введите длительность в минутах (целое число > 0).");
                TimeBox.Focus();
                return false;
            }

            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
                return;

            var name = NameBox.Text.Trim();
            var description = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : DescriptionBox.Text.Trim();
            var price = decimal.Parse(PriceBox.Text);
            var minutes = int.Parse(TimeBox.Text); // уже проверено в ValidateInputs

            try
            {
                if (isEditMode)
                {
                    var service = _context.Services.Find(editingService.ServiceId);
                    if (service == null)
                    {
                        ShowError("Услуга не найдена.");
                        return;
                    }

                    service.NameServices = name;
                    service.DescriptionService = description;
                    service.Price = price;
                    service.Time = minutes; // минуты!

                    _context.SaveChanges();
                }
                else
                {
                    var newService = new Service
                    {
                        NameServices = name,
                        DescriptionService = description,
                        Price = price,
                        Time = minutes
                    };
                    _context.Services.Add(newService);
                    _context.SaveChanges();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}