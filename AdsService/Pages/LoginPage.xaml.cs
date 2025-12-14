using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AdsService.Services;

namespace AdsService.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.SignOut();
            NavigationService.Navigate(new PublicAdsPage());
        }

        private void BtnSignIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = TxtLogin.Text;
                string password = PwdPassword.Password;

                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    MessageService.ShowWarning("Введите логин и пароль.\nПроверьте поля и повторите попытку.");
                    return;
                }

                var context = Entities.GetContext();

                Users user = context.Users
                    .FirstOrDefault(u => u.Login == login && u.Password == password);

                if (user == null)
                {
                    MessageService.ShowError("Неверный логин или пароль.\nПроверьте введённые данные и повторите попытку.");
                    return;
                }

                SessionManager.SignIn(user);
                NavigationService.Navigate(new UserAdsPage());
            }
            catch (Exception ex)
            {
                MessageService.ShowError("Ошибка авторизации.\n" + ex.Message);
            }
        }
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }

    }
}
