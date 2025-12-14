using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AdsService.Pages
{
    public partial class PublicAdsPage : Page
    {
        private bool _isLoaded;

        public PublicAdsPage()
        {
            InitializeComponent();
            LoadFilters();
            UpdateAdsList();
            _isLoaded = true;
        }

        private void LoadFilters()
        {
            try
            {
                var context = Entities.GetContext();

                // Города
                List<Cities> cities = context.Cities
                    .OrderBy(c => c.Name)
                    .ToList();

                cities.Insert(0, new Cities { CityId = 0, Name = "Все города" });

                CmbCity.ItemsSource = cities;
                CmbCity.SelectedIndex = 0;

                // Категории
                List<Categories> categories = context.Categories
                    .OrderBy(c => c.Name)
                    .ToList();

                categories.Insert(0, new Categories { CategoryId = 0, Name = "Все категории" });

                CmbCategory.ItemsSource = categories;
                CmbCategory.SelectedIndex = 0;

                // Типы объявлений
                List<AdTypes> types = context.AdTypes
                    .OrderBy(t => t.Name)
                    .ToList();

                types.Insert(0, new AdTypes { AdTypeId = 0, Name = "Все типы" });

                CmbType.ItemsSource = types;
                CmbType.SelectedIndex = 0;

                // Статусы
                List<AdStatuses> statuses = context.AdStatuses
                    .OrderBy(s => s.Name)
                    .ToList();

                statuses.Insert(0, new AdStatuses { AdStatusId = 0, Name = "Все статусы" });

                CmbStatus.ItemsSource = statuses;
                CmbStatus.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при загрузке фильтров: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void UpdateAdsList()
        {
            try
            {
                var context = Entities.GetContext();

                IQueryable<Ads> query = context.Ads
                    .Include(a => a.Cities)
                    .Include(a => a.Categories)
                    .Include(a => a.AdTypes)
                    .Include(a => a.AdStatuses);

                // Поиск
                string searchText = TxtSearch.Text;

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string lowered = searchText.ToLower();

                    query = query.Where(a =>
                        a.Title.ToLower().Contains(lowered) ||
                        (a.Description != null && a.Description.ToLower().Contains(lowered)));
                }

                // Фильтр по городу
                Cities selectedCity = CmbCity.SelectedItem as Cities;

                if (selectedCity != null && selectedCity.CityId != 0)
                {
                    int cityId = selectedCity.CityId;
                    query = query.Where(a => a.CityId == cityId);
                }

                // Фильтр по категории
                Categories selectedCategory = CmbCategory.SelectedItem as Categories;

                if (selectedCategory != null && selectedCategory.CategoryId != 0)
                {
                    int categoryId = selectedCategory.CategoryId;
                    query = query.Where(a => a.CategoryId == categoryId);
                }

                // Фильтр по типу
                AdTypes selectedType = CmbType.SelectedItem as AdTypes;

                if (selectedType != null && selectedType.AdTypeId != 0)
                {
                    int typeId = selectedType.AdTypeId;
                    query = query.Where(a => a.AdTypeId == typeId);
                }

                // Фильтр по статусу
                AdStatuses selectedStatus = CmbStatus.SelectedItem as AdStatuses;

                if (selectedStatus != null && selectedStatus.AdStatusId != 0)
                {
                    int statusId = selectedStatus.AdStatusId;
                    query = query.Where(a => a.AdStatusId == statusId);
                }

                List<Ads> ads = query.ToList();

                AdsListView.ItemsSource = ads;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при загрузке объявлений: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded)
            {
                return;
            }

            UpdateAdsList();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded)
            {
                return;
            }

            UpdateAdsList();
        }

        private void BtnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Text = string.Empty;

            if (CmbCity.Items.Count > 0)
            {
                CmbCity.SelectedIndex = 0;
            }

            if (CmbCategory.Items.Count > 0)
            {
                CmbCategory.SelectedIndex = 0;
            }

            if (CmbType.Items.Count > 0)
            {
                CmbType.SelectedIndex = 0;
            }

            if (CmbStatus.Items.Count > 0)
            {
                CmbStatus.SelectedIndex = 0;
            }

            UpdateAdsList();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }

        private void BtnMyAds_Click(object sender, RoutedEventArgs e)
        {
            if (!AdsService.Services.SessionManager.IsAuthorized)
            {
                AdsService.Services.MessageService.ShowWarning("Сначала выполните вход в систему.");
                NavigationService.Navigate(new LoginPage());
                return;
            }

            NavigationService.Navigate(new UserAdsPage());
        }

    }
}
