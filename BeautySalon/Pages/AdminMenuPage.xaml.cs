using System.Windows;
using System.Windows.Controls;
using BeautySalon.Pages;

namespace BeautySalon.Pages
{
    public partial class AdminMenuPage : Page
    {
        public AdminMenuPage()
        {
            InitializeComponent();

            //if (App.CurrentUser != null)
            //{
            //    string roleName = App.CurrentUser.RoleId == 1 ? "Администратор" : "Пользователь";
            //    TextCurrentUser.Text = $"Добро пожаловать, {App.CurrentUser.Login} ({roleName})";
            //}
            //else
            //{
            //    TextCurrentUser.Text = "Добро пожаловать!";
            //}

            // Для обычных пользователей скрываем кнопки управления пользователями и кабинетами
            if (App.CurrentUser != null && App.CurrentUser.RoleId == 2)
            {
                BtnUser.Visibility = Visibility.Collapsed;
                BtnRoom.Visibility = Visibility.Collapsed;
                EmployeesButton.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnUser_Click(object sender, RoutedEventArgs e)
        {
            // Переход на страницу управления пользователями
            NavigationService.Navigate(new UserPage());
        }
        private void BtnServices_Click(object s, RoutedEventArgs e) =>
     NavigationService.Navigate(new ServicesPage());
        private void BtnRegistrations_Click(object s, RoutedEventArgs e) =>
     NavigationService.Navigate(new RegistrationsPage());

        private void BtnRoom_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Данный функционал находится в разработке!",
                "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из системы?",
                "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                App.CurrentUser = null;
                NavigationService.Navigate(new AuthPage());
            }
        }
        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            // Навигация на страницу сотрудников через NavigationService
            NavigationService.Navigate(new EmployeesPage());
        }
    }
}