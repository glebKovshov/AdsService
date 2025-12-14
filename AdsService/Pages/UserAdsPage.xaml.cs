using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AdsService.Services;

namespace AdsService.Pages
{
    public partial class UserAdsPage : Page
    {
        private bool _isLoaded;

        public UserAdsPage()
        {
            InitializeComponent();

            CheckAccess();
            LoadUserInfo();
            UpdateAds();

            IsVisibleChanged += UserAdsPage_IsVisibleChanged;
        }

        private void UserAdsPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                UpdateAds();
            }
        }


        private void CheckAccess()
        {
            if (!SessionManager.IsAuthorized)
            {
                MessageService.ShowWarning("Доступ к разделу «Мои объявления» возможен только после входа.\nВыполните авторизацию.");
                NavigationService.Navigate(new LoginPage());
            }
        }

        private void LoadUserInfo()
        {
            if (!SessionManager.IsAuthorized)
            {
                return;
            }

            TxtUserInfo.Text = "Пользователь: " + SessionManager.CurrentUser.Login;
        }

        private void UpdateAds()
        {
            try
            {
                if (!SessionManager.IsAuthorized)
                {
                    return;
                }

                int userId = SessionManager.CurrentUser.UserId;

                var context = Entities.GetContext();

                List<Ads> ads = context.Ads
                    .Include(a => a.AdStatuses)
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.PostDate)
                    .ToList();

                AdsListView.ItemsSource = ads;
            }
            catch (Exception ex)
            {
                MessageService.ShowError("Ошибка загрузки списка объявлений.\n" + ex.Message);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdEditPage(null));
        }

        private void AdsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded)
            {
                return;
            }

            Ads selected = AdsListView.SelectedItem as Ads;

            if (selected == null)
            {
                return;
            }

            AdsListView.SelectedItem = null;
            NavigationService.Navigate(new AdEditPage(selected.AdId));
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                Ads ad = button.DataContext as Ads;

                if (ad == null)
                {
                    return;
                }

                bool ok = MessageService.Confirm("Удалить объявление \"" + ad.Title + "\"?\nОперация необратима.");

                if (!ok)
                {
                    return;
                }

                var context = Entities.GetContext();

                Ads dbAd = context.Ads.FirstOrDefault(a => a.AdId == ad.AdId);

                if (dbAd == null)
                {
                    MessageService.ShowWarning("Объявление уже удалено или не найдено в базе.\nОбновите список.");
                    UpdateAds();
                    return;
                }

                context.Ads.Remove(dbAd);
                context.SaveChanges();

                MessageService.ShowInfo("Объявление удалено.");
                UpdateAds();
            }
            catch (Exception ex)
            {
                MessageService.ShowError("Ошибка удаления объявления.\n" + ex.Message);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new PublicAdsPage());
            }
        }

        private void BtnSignOut_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.SignOut();
            NavigationService.Navigate(new LoginPage());
        }

        private void BtnCompleted_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CompletedAdsPage());
        }
        private void AdTile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            Ads ad = border?.DataContext as Ads;

            if (ad == null)
            {
                return;
            }

            NavigationService.Navigate(new AdEditPage(ad.AdId));
        }

        private void AdsItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Если кликнули по кнопке "Удалить" (или по её содержимому) — НЕ открываем редактирование
                DependencyObject source = e.OriginalSource as DependencyObject;

                if (FindParent<Button>(source) != null)
                {
                    return;
                }

                ListViewItem item = sender as ListViewItem;
                Ads ad = item?.DataContext as Ads;

                if (ad == null)
                {
                    return;
                }

                // На всякий случай: если NavigationService вдруг null, покажем понятную ошибку
                if (NavigationService == null)
                {
                    MessageService.ShowError(
                        "Навигация недоступна (NavigationService = null).\n" +
                        "Проверь, что страница открыта через Frame.Navigate().");
                    return;
                }

                NavigationService.Navigate(new AdEditPage(ad.AdId));
                e.Handled = true;
            }
            catch (Exception ex)
            {
                AdsService.Services.MessageService.ShowError("Ошибка открытия формы редактирования.\n" + ex.Message);
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T typed)
                {
                    return typed;
                }

                child = VisualTreeHelper.GetParent(child);
            }

            return null;
        }

    }
}
