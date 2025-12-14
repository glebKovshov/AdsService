using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AdsService.Services;

namespace AdsService.Pages
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new LoginPage());
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = TxtLogin.Text?.Trim();
                string password = PwdPassword.Password;
                string repeat = PwdRepeat.Password;

                if (string.IsNullOrWhiteSpace(login))
                {
                    MessageService.ShowWarning("Логин обязателен.\nВведите логин и повторите попытку.");
                    return;
                }

                if (login.Length < 3 || login.Length > 50)
                {
                    MessageService.ShowWarning("Логин должен быть от 3 до 50 символов.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageService.ShowWarning("Пароль обязателен.\nВведите пароль и повторите попытку.");
                    return;
                }

                if (password.Length < 4 || password.Length > 50)
                {
                    MessageService.ShowWarning("Пароль должен быть от 4 до 50 символов.");
                    return;
                }

                if (password != repeat)
                {
                    MessageService.ShowWarning("Пароли не совпадают.\nПроверьте ввод и повторите попытку.");
                    return;
                }

                var context = Entities.GetContext();

                bool exists = context.Users.Any(u => u.Login == login);

                if (exists)
                {
                    MessageService.ShowError("Пользователь с таким логином уже существует.\nПридумайте другой логин.");
                    return;
                }

                Users user = new Users
                {
                    Login = login,
                    Password = password
                };

                context.Users.Add(user);
                context.SaveChanges();

                MessageService.ShowInfo("Регистрация выполнена успешно.\nТеперь войдите под своим логином и паролем.");

                NavigationService.Navigate(new LoginPage());
            }
            catch (Exception ex)
            {
                MessageService.ShowError("Ошибка регистрации.\n" + ex.Message);
            }
        }
    }
}
