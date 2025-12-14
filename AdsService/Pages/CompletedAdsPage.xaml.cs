using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using AdsService.Services;

namespace AdsService.Pages
{
    public partial class CompletedAdsPage : Page
    {
        public CompletedAdsPage()
        {
            InitializeComponent();

            CheckAccess();
            LoadCompletedAds();

            IsVisibleChanged += CompletedAdsPage_IsVisibleChanged;
        }

        private void CompletedAdsPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                LoadCompletedAds();
            }
        }

        private void CheckAccess()
        {
            if (!SessionManager.IsAuthorized)
            {
                MessageService.ShowWarning("Доступ к завершённым объявлениям возможен только после входа.\nВыполните авторизацию.");
                NavigationService.Navigate(new LoginPage());
            }
        }

        private void LoadCompletedAds()
        {
            try
            {
                if (!SessionManager.IsAuthorized)
                {
                    return;
                }

                int userId = SessionManager.CurrentUser.UserId;
                var context = Entities.GetContext();

                AdStatuses completedStatus = context.AdStatuses.FirstOrDefault(s => s.Name == "Завершено");

                if (completedStatus == null)
                {
                    MessageService.ShowError("В базе данных отсутствует статус \"Завершено\".\nДобавьте его в таблицу AdStatuses.");
                    AdsListView.ItemsSource = new List<Ads>();
                    TxtTotalProfit.Text = "0 ₽";
                    return;
                }

                int completedId = completedStatus.AdStatusId;

                List<Ads> ads = context.Ads
                    .Include(a => a.AdStatuses)
                    .Where(a => a.UserId == userId && a.AdStatusId == completedId)
                    .OrderByDescending(a => a.PostDate)
                    .ToList();

                AdsListView.ItemsSource = ads;

                int totalProfit = ads.Sum(a => a.ProfitAmount ?? 0);
                TxtTotalProfit.Text = totalProfit + " ₽";
            }
            catch (Exception ex)
            {
                MessageService.ShowError("Ошибка загрузки завершённых объявлений.\n" + ex.Message);
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
                NavigationService.Navigate(new UserAdsPage());
            }
        }
    }
}
