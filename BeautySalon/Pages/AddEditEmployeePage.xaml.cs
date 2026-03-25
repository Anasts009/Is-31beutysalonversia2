using BeautySalon.Model;
using BeautySalon.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BeautySalon.Pages
{
    public partial class AddEditEmployeePage : Page
    {
        private BeautySalonContext _context;
        private Employee _editingEmployee;
        private bool _isEditMode = false;

        public AddEditEmployeePage(Employee employee = null)
        {
            InitializeComponent();
            _context = new BeautySalonContext();
            LoadComboBoxes();

            if (employee != null)
            {
                _isEditMode = true;
                // Важно: загружаем сотрудника с пользователем
                _editingEmployee = _context.Employees
                    .Include(e => e.User)
                    .FirstOrDefault(e => e.EmployeeId == employee.EmployeeId);

                if (_editingEmployee == null)
                {
                    MessageBox.Show("Сотрудник не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService.GoBack();
                    return;
                }

                Title = "Редактирование сотрудника";
                LoadEmployeeData();
                BlockCheckBox.Visibility = Visibility.Visible;
            }
            else
            {
                Title = "Добавление сотрудника";
                BlockCheckBox.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadComboBoxes()
        {
            // Должности
            PostBox.ItemsSource = _context.Posts.OrderBy(p => p.PostName).ToList();
            PostBox.SelectedValuePath = "PostId";
            PostBox.DisplayMemberPath = "PostName";

            // Кабинеты – используем NameCabinet
            CabinetBox.ItemsSource = _context.Cabinets.OrderBy(c => c.NameCabinet).ToList();
            CabinetBox.SelectedValuePath = "CabinetId";
            CabinetBox.DisplayMemberPath = "NameCabinet";

            // Роли
            RoleBox.ItemsSource = _context.Roles.OrderBy(r => r.RoleName).ToList();
            RoleBox.SelectedValuePath = "RoleId";
            RoleBox.DisplayMemberPath = "RoleName";

            // По умолчанию выбираем роль "Сотрудник"
            var defaultRole = _context.Roles.FirstOrDefault(r => r.RoleName == "Сотрудник");
            if (defaultRole != null)
                RoleBox.SelectedValue = defaultRole.RoleId;
        }

        private void LoadEmployeeData()
        {
            FirstnameBox.Text = _editingEmployee.Firstname;
            LastnameBox.Text = _editingEmployee.Lastname;
            PhoneBox.Text = _editingEmployee.Phone;
            PostBox.SelectedValue = _editingEmployee.PostId;
            CabinetBox.SelectedValue = _editingEmployee.CabinetId;

            if (_editingEmployee.User != null)
            {
                LoginBox.Text = _editingEmployee.User.Login;
                PasswordBox.Password = _editingEmployee.User.Password;
                BlockCheckBox.IsChecked = _editingEmployee.User.Block;
                RoleBox.SelectedValue = _editingEmployee.User.RoleId;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstnameBox.Text) ||
                string.IsNullOrWhiteSpace(LastnameBox.Text) ||
                string.IsNullOrWhiteSpace(LoginBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ErrorText.Text = "Заполните все обязательные поля (Имя, Фамилия, Логин, Пароль)";
                return;
            }

            if (RoleBox.SelectedValue == null)
            {
                ErrorText.Text = "Выберите роль";
                return;
            }

            try
            {
                if (_isEditMode)
                {
                    _editingEmployee.Firstname = FirstnameBox.Text;
                    _editingEmployee.Lastname = LastnameBox.Text;
                    _editingEmployee.Phone = PhoneBox.Text;
                    _editingEmployee.PostId = (int?)PostBox.SelectedValue;
                    _editingEmployee.CabinetId = (int?)CabinetBox.SelectedValue;

                    if (_editingEmployee.User != null)
                    {
                        _editingEmployee.User.Login = LoginBox.Text;
                        _editingEmployee.User.Password = PasswordBox.Password;
                        _editingEmployee.User.Block = BlockCheckBox.IsChecked ?? false;
                        _editingEmployee.User.RoleId = (int)RoleBox.SelectedValue;
                        _context.Entry(_editingEmployee.User).State = EntityState.Modified;
                    }
                    _context.Entry(_editingEmployee).State = EntityState.Modified;
                }
                else
                {
                    var user = new User
                    {
                        Login = LoginBox.Text,
                        Password = PasswordBox.Password,
                        Block = false,
                        RoleId = (int)RoleBox.SelectedValue,
                        FirstAuth = true
                    };
                    _context.Users.Add(user);
                    _context.SaveChanges();

                    var employee = new Employee
                    {
                        Firstname = FirstnameBox.Text,
                        Lastname = LastnameBox.Text,
                        Phone = PhoneBox.Text,
                        PostId = (int?)PostBox.SelectedValue,
                        CabinetId = (int?)CabinetBox.SelectedValue,
                        UserId = user.UserId
                    };
                    _context.Employees.Add(employee);
                }

                _context.SaveChanges();
                // Возврат на предыдущую страницу (список сотрудников)
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                ErrorText.Text = "Ошибка сохранения: " + ex.Message;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}