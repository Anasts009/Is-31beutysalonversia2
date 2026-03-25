using BeautySalon.Model;
using BeautySalon.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace BeautySalon.Pages
{
    public partial class EmployeesPage : Page
    {
        private BeautySalonContext _context;
        private string currentSearch = "";
        private int? currentPostId = null;

        public EmployeesPage()
        {
            InitializeComponent();
            _context = new BeautySalonContext();
            LoadPostsFilter();
            LoadEmployees();

            // Устанавливаем текст-заполнитель для поиска
            SearchBox.Text = "Поиск по имени или фамилии...";
            SearchBox.Foreground = Brushes.Gray;
        }

        private void LoadPostsFilter()
        {
            var posts = _context.Posts.OrderBy(p => p.PostName).ToList();
            posts.Insert(0, new Post { PostId = 0, PostName = "Все" });
            PositionFilterBox.ItemsSource = posts;
            PositionFilterBox.DisplayMemberPath = "PostName";
            PositionFilterBox.SelectedValuePath = "PostId";
            PositionFilterBox.SelectedValue = 0;
        }

        private void LoadEmployees()
        {
            var query = _context.Employees
                .Include(e => e.User)
                .Include(e => e.Post)
                .Include(e => e.Cabinet)
                .AsQueryable();

            if (currentPostId.HasValue && currentPostId != 0)
                query = query.Where(e => e.PostId == currentPostId.Value);

            if (!string.IsNullOrEmpty(currentSearch) && currentSearch != "Поиск по имени или фамилии...")
                query = query.Where(e => e.Firstname.Contains(currentSearch) ||
                                         e.Lastname.Contains(currentSearch));

            var employees = query.ToList();
            EmployeesGrid.ItemsSource = employees;
            StatusText.Text = $"Найдено сотрудников: {employees.Count}";
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Поиск по имени или фамилии...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Поиск по имени или фамилии...";
                SearchBox.Foreground = Brushes.Gray;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text != "Поиск по имени или фамилии...")
            {
                currentSearch = SearchBox.Text;
                LoadEmployees();
            }
        }

        private void PositionFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PositionFilterBox.SelectedValue != null)
            {
                int selectedId = (int)PositionFilterBox.SelectedValue;
                currentPostId = selectedId == 0 ? (int?)null : selectedId;
                LoadEmployees();
            }
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "Поиск по имени или фамилии...";
            SearchBox.Foreground = Brushes.Gray;
            currentSearch = "";
            PositionFilterBox.SelectedValue = 0;
            LoadEmployees();
        }

        private void QuickSearch_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tag = button.Tag.ToString();

            switch (tag)
            {
                case "all":
                    currentPostId = null;
                    currentSearch = "";
                    SearchBox.Text = "Поиск по имени или фамилии...";
                    SearchBox.Foreground = Brushes.Gray;
                    PositionFilterBox.SelectedValue = 0;
                    break;
                case "admin":
                    var adminPost = _context.Posts.FirstOrDefault(p => p.PostName.Contains("Админ"));
                    if (adminPost != null)
                        currentPostId = adminPost.PostId;
                    PositionFilterBox.SelectedValue = currentPostId;
                    break;
                case "master":
                    var masterPost = _context.Posts.FirstOrDefault(p => p.PostName.Contains("Мастер"));
                    if (masterPost != null)
                        currentPostId = masterPost.PostId;
                    PositionFilterBox.SelectedValue = currentPostId;
                    break;
                case "blocked":
                    currentPostId = null;
                    PositionFilterBox.SelectedValue = 0;
                    LoadBlockedEmployees();
                    return;
            }
            LoadEmployees();
        }

        private void LoadBlockedEmployees()
        {
            var blockedEmployees = _context.Employees
                .Include(e => e.User)
                .Include(e => e.Post)
                .Include(e => e.Cabinet)
                .Where(e => e.User.Block == true)
                .ToList();
            EmployeesGrid.ItemsSource = blockedEmployees;
            StatusText.Text = $"Найдено заблокированных сотрудников: {blockedEmployees.Count}";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditEmployeePage());
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var employeeId = (int)button.Tag;
            var employee = _context.Employees
                .Include(e => e.User)
                .FirstOrDefault(e => e.EmployeeId == employeeId);

            if (employee != null)
            {
                NavigationService.Navigate(new AddEditEmployeePage(employee));
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var employeeId = (int)button.Tag;
            var employee = _context.Employees
                .Include(e => e.User)
                .FirstOrDefault(e => e.EmployeeId == employeeId);

            if (employee == null) return;

            var result = MessageBox.Show($"Удалить сотрудника {employee.Firstname} {employee.Lastname}?\n" +
                                         "Связанный пользователь также будет удалён.",
                                         "Подтверждение удаления",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Employees.Remove(employee);
                    if (employee.User != null)
                        _context.Users.Remove(employee.User);
                    _context.SaveChanges();
                    LoadEmployees();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
    public class BlockStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBlocked)
                return isBlocked ? "Заблокирован" : "Активен";
            return "Неизвестно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BlockColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBlocked)
            {
                return isBlocked ? new SolidColorBrush(Color.FromRgb(244, 67, 54)) : new SolidColorBrush(Color.FromRgb(76, 175, 80));
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}