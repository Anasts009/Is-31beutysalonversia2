using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BeautySalon.Model;
using BeautySalon.Windows;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Pages
{
    public partial class UserPage : Page
    {
        private BeautySalonContext _context;
        private string currentSearch = "";
        private int? currentRoleId = null;
        private bool? currentBlockFilter = null;

        public UserPage()
        {
            InitializeComponent();
            _context = new BeautySalonContext();
            LoadRolesFilter();
            LoadUsers();
        }

        private void LoadRolesFilter()
        {
            var roles = _context.Roles.OrderBy(r => r.RoleName).ToList();
            roles.Insert(0, new Role { RoleId = 0, RoleName = "Все" });
            RoleFilterBox.ItemsSource = roles;
            RoleFilterBox.DisplayMemberPath = "RoleName";
            RoleFilterBox.SelectedValuePath = "RoleId";
            RoleFilterBox.SelectedValue = 0;
        }

        private void LoadUsers()
        {
            var query = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            // Фильтр по роли
            if (currentRoleId.HasValue && currentRoleId != 0)
                query = query.Where(u => u.RoleId == currentRoleId.Value);

            // Фильтр по статусу блокировки (если задан)
            if (currentBlockFilter.HasValue)
                query = query.Where(u => u.Block == currentBlockFilter.Value);

            // Поиск по логину
            if (!string.IsNullOrEmpty(currentSearch))
                query = query.Where(u => u.Login.Contains(currentSearch));

            var users = query
                .Select(u => new Models.UserViewModel
                {
                    UserId = u.UserId,
                    Login = u.Login,
                    RoleId = u.RoleId,
                    RoleName = u.Role.RoleName,
                    Block = u.Block,
                    FirstAuth = u.FirstAuth
                })
                .ToList();

            UsersGrid.ItemsSource = users;
            StatusText.Text = $"Найдено пользователей: {users.Count}";
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Поиск по логину...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Поиск по логину...";
                SearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text != "Поиск по логину...")
            {
                currentSearch = SearchBox.Text;
                LoadUsers();
            }
            else
            {
                currentSearch = "";
                LoadUsers();
            }
        }

        private void RoleFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RoleFilterBox.SelectedValue != null)
            {
                int selectedId = (int)RoleFilterBox.SelectedValue;
                currentRoleId = selectedId == 0 ? (int?)null : selectedId;
                LoadUsers();
            }
        }

        private void QuickFilter_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tag = button.Tag.ToString();

            switch (tag)
            {
                case "all":
                    currentBlockFilter = null;
                    break;
                case "blocked":
                    currentBlockFilter = true;
                    break;
                case "active":
                    currentBlockFilter = false;
                    break;
            }
            LoadUsers();
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "Поиск по логину...";
            SearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            currentSearch = "";
            RoleFilterBox.SelectedValue = 0;
            currentBlockFilter = null;
            LoadUsers();
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddUserWindow();
            window.Owner = Window.GetWindow(this);
            if (window.ShowDialog() == true)
            {
                LoadUsers();
                MessageBox.Show("Пользователь успешно добавлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int userId = (int)button.Tag;

            var user = _context.Users.Find(userId);
            if (user != null)
            {
                var window = new AddUserWindow(user);
                window.Owner = Window.GetWindow(this);
                if (window.ShowDialog() == true)
                {
                    LoadUsers();
                    MessageBox.Show("Пользователь успешно обновлен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void UnblockUserButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int userId = (int)button.Tag;

            var result = MessageBox.Show("Разблокировать пользователя?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var user = _context.Users.Find(userId);
                if (user != null)
                {
                    user.Block = false;
                    _context.SaveChanges();
                    LoadUsers();
                    MessageBox.Show("Пользователь разблокирован!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int userId = (int)button.Tag;

            var result = MessageBox.Show("Вы уверены, что хотите удалить пользователя?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var user = _context.Users
                        .Include(u => u.Client)
                        .Include(u => u.Employee)
                        .FirstOrDefault(u => u.UserId == userId);

                    if (user != null)
                    {
                        if (user.Client != null || user.Employee != null)
                        {
                            MessageBox.Show("Невозможно удалить пользователя, так как он связан с клиентом или сотрудником.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        _context.Users.Remove(user);
                        _context.SaveChanges();
                        LoadUsers();
                        MessageBox.Show("Пользователь удален!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}