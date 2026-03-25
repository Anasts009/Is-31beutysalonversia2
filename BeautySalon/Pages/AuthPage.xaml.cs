using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BeautySalon.Model;

namespace BeautySalon.Pages
{
    public partial class AuthPage : Page
    {
        public int Count = 0;

        public AuthPage()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // 1) Проверка заполнения полей
            if (string.IsNullOrWhiteSpace(TBoxLogin.Text) || string.IsNullOrWhiteSpace(TBoxPassword.Password))
            {
                MessageBox.Show("Все поля должны быть заполнены!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 2) Поиск пользователя
            var user = App.Context.Users
              .FirstOrDefault(p => p.Login == TBoxLogin.Text && p.Password == TBoxPassword.Password);

            // 3) Если пользователь не найден — считаем попытку
            if (user == null)
            {
                MessageBox.Show("Неверный логин или пароль! Пожалуйста, проверьте введенные данные.",
                  "Ошибка авторизации!", MessageBoxButton.OK, MessageBoxImage.Error);

                Count++;

                if (Count >= 3)
                {
                    // Попытаемся заблокировать именно введённый логин (если такой существует)
                    var userToBlock = App.Context.Users.FirstOrDefault(p => p.Login == TBoxLogin.Text);
                    if (userToBlock != null)
                    {
                        // Защита: не блокировать последнего активного администратора
                        bool isAdmin = userToBlock.RoleId == 1;
                        int otherActiveAdmins = App.Context.Users.Count(u => u.RoleId == 1 && !u.Block && u.UserId != userToBlock.UserId);

                        if (isAdmin && otherActiveAdmins == 0)
                        {
                            MessageBox.Show("Это единственная админ‑учётка, её нельзя блокировать. Обратитесь к администратору БД.",
                              "Отказ в блокировке", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            userToBlock.Block = true;
                            App.Context.SaveChanges();
                            MessageBox.Show("Вы ввели неверные данные 3 раза. Учетная запись заблокирована!",
                              "Блокировка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                        Count = 0; // сбрасываем счётчик
                    }
                }

                TBoxLogin.Text = "";
                TBoxPassword.Password = "";
                return;
            }

            // 4) Пользователь найден — сбрасываем счётчик
            Count = 0;

            // 5) Проверка блокировки
            if (user.Block)
            {
                MessageBox.Show("Вы заблокированы! Обратитесь к администратору.",
                  "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                TBoxLogin.Text = "";
                TBoxPassword.Password = "";
                return;
            }

            // 6) Проверка роли
            if (user.RoleId == 1 || user.RoleId == 2)
            {
                string roleName = user.RoleId == 1 ? "Администратор" : "Пользователь";
                MessageBox.Show($"Вы успешно авторизовались как {roleName}",
          "Успешный вход!", MessageBoxButton.OK, MessageBoxImage.Information);

                App.CurrentUser = user;

                // 7) Первый вход
                if (!user.FirstAuth)
                {
                    user.FirstAuth = true;
                    App.Context.SaveChanges();
                    // здесь можно перейти на страницу смены пароля
                    // NavigationService.Navigate(new ChangePasswordPage());
                }

                // 8) Переход в меню
                NavigationService.Navigate(new AdminMenuPage());
            }
            else
            {
                MessageBox.Show("Недостаточно прав для использования системы!",
                  "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                TBoxLogin.Text = "";
                TBoxPassword.Password = "";
            }
        }
    }
}