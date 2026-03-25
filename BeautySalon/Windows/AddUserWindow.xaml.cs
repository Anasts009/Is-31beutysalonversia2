using BeautySalon.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BeautySalon.Windows
{
    public partial class AddUserWindow : Window
    {
        private BeautySalonContext _context;
        private User editingUser;
        private bool isEditMode = false;

        public AddUserWindow(User user = null)
        {
            InitializeComponent();
            _context = new BeautySalonContext();

            if (user != null)
            {
                isEditMode = true;
                editingUser = user;
                Title = "Редактирование пользователя";
                LoadUserData();
            }
            else
            {
                Title = "Добавление пользователя";
                BlockCheckBox.Visibility = Visibility.Collapsed; // Скрываем для нового пользователя
            }
        }

        private void LoadUserData()
        {
            LoginBox.Text = editingUser.Login;
            PasswordBox.Password = editingUser.Password;

            // Загружаем статус блокировки
            BlockCheckBox.IsChecked = editingUser.Block;

            foreach (ComboBoxItem item in RoleBox.Items)
            {
                if (item.Tag.ToString() == editingUser.RoleId.ToString())
                {
                    RoleBox.SelectedItem = item;
                    break;
                }
            }
        }

        private bool ValidateInputs()
        {
            ErrorText.Text = "";

            if (string.IsNullOrWhiteSpace(LoginBox.Text))
            {
                ErrorText.Text = "Введите логин";
                LoginBox.Focus();
                return false;
            }

            if (LoginBox.Text.Length > 100)
            {
                ErrorText.Text = "Логин не может быть длиннее 100 символов";
                LoginBox.Focus();
                return false;
            }

            if (!isEditMode && _context.Users.Any(u => u.Login == LoginBox.Text.Trim()))
            {
                ErrorText.Text = "Пользователь с таким логином уже существует";
                LoginBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ErrorText.Text = "Введите пароль";
                PasswordBox.Focus();
                return false;
            }

            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
                return;

            try
            {
                int roleId = int.Parse(((ComboBoxItem)RoleBox.SelectedItem).Tag.ToString());
                bool isBlocked = BlockCheckBox.IsChecked ?? false;

                if (isEditMode)
                {
                    var user = _context.Users.Find(editingUser.UserId);
                    if (user != null)
                    {
                        user.Login = LoginBox.Text.Trim();
                        user.Password = PasswordBox.Password;
                        user.RoleId = roleId;
                        user.Block = isBlocked; // Обновляем статус блокировки
                        _context.SaveChanges();

                        // Сообщение о разблокировке
                        if (!isBlocked && editingUser.Block)
                        {
                            MessageBox.Show("Учетная запись разблокирована!",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                else
                {
                    var newUser = new User
                    {
                        Login = LoginBox.Text.Trim(),
                        Password = PasswordBox.Password,
                        RoleId = roleId,
                        Block = false, // Новые пользователи не заблокированы
                        FirstAuth = false
                    };
                    _context.Users.Add(newUser);
                    _context.SaveChanges();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ErrorText.Text = $"Ошибка сохранения: {ex.Message}";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}