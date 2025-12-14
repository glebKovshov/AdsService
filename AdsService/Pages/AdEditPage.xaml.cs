using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using AdsService.Services;
using AdsService.Windows;
using Microsoft.Win32;

namespace AdsService.Pages
{
    public partial class AdEditPage : Page
    {
        private readonly int? _adId;
        private int _oldStatusId;

        public AdEditPage(int? adId)
        {
            InitializeComponent();

            _adId = adId;

            CheckAccess();
            LoadLookups();

            if (_adId.HasValue)
            {
                TxtHeader.Text = "Редактирование объявления";
                LoadAd(_adId.Value);
            }
            else
            {
                TxtHeader.Text = "Добавление объявления";
                PrepareNewAd();
            }
        }

        private void CheckAccess()
        {
            if (!SessionManager.IsAuthorized)
            {
                MessageService.ShowWarning("Для добавления и редактирования объявлений требуется авторизация.\nВойдите в систему.");
                NavigationService.Navigate(new LoginPage());
            }
        }

        private void LoadLookups()
        {
            try
            {
                var context = Entities.GetContext();

                CmbCity.ItemsSource = context.Cities.OrderBy(c => c.Name).ToList();
                CmbCategory.ItemsSource = context.Categories.OrderBy(c => c.Name).ToList();
                CmbType.ItemsSource = context.AdTypes.OrderBy(t => t.Name).ToList();
                CmbStatus.ItemsSource = context.AdStatuses.OrderBy(s => s.Name).ToList();
            }
            catch (Exception ex)
            {
                MessageService.ShowError("Ошибка загрузки справочников.\n" + ex.Message);
            }
        }

        private void PrepareNewAd()
        {
            DpPostDate.SelectedDate = DateTime.Today;
        }

        private void LoadAd(int adId)
        {
            try
            {
                var context = Entities.GetContext();

                Ads ad = context.Ads.FirstOrDefault(a => a.AdId == adId);

                if (ad == null)
                {
                    MessageService.ShowError("Объявление не найдено.\nВозможно, оно было удалено.");
                    NavigationService.GoBack();
                    return;
                }

                if (ad.UserId != SessionManager.CurrentUser.UserId)
                {
                    MessageService.ShowWarning("Редактирование чужих объявлений запрещено.\nВыберите своё объявление.");
                    NavigationService.GoBack();
                    return;
                }

                TxtTitle.Text = ad.Title;
                TxtDescription.Text = ad.Description;
                TxtPrice.Text = ad.Price.ToString();
                DpPostDate.SelectedDate = ad.PostDate;
                TxtImagePath.Text = ad.ImagePath;

                SelectById(CmbCity, "CityId", ad.CityId);
                SelectById(CmbCategory, "CategoryId", ad.CategoryId);
                SelectById(CmbType, "AdTypeId", ad.AdTypeId);
                SelectById(CmbStatus, "AdStatusId", ad.AdStatusId);

                _oldStatusId = ad.AdStatusId;
            }
            catch (Exception ex)
            {
                MessageService.ShowError("Ошибка загрузки объявления.\n" + ex.Message);
            }
        }

        private void SelectById(ComboBox comboBox, string propertyName, int id)
        {
            foreach (object item in comboBox.Items)
            {
                var prop = item.GetType().GetProperty(propertyName);

                if (prop == null)
                {
                    continue;
                }

                object value = prop.GetValue(item);

                if (value is int intValue && intValue == id)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
        }

        private void BtnPickImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Все файлы (*.*)|*.*";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                TxtImagePath.Text = dialog.FileName;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateForm())
                {
                    return;
                }

                var context = Entities.GetContext();

                AdStatuses completed = context.AdStatuses.FirstOrDefault(s => s.Name == "Завершено");

                if (completed == null)
                {
                    MessageService.ShowError("В базе данных отсутствует статус \"Завершено\".\nДобавьте его в таблицу AdStatuses.");
                    return;
                }

                int completedId = completed.AdStatusId;

                int newStatusId = ((AdStatuses)CmbStatus.SelectedItem).AdStatusId;
                bool setCompletedNow = newStatusId == completedId && _oldStatusId != completedId;

                Ads ad;

                if (_adId.HasValue)
                {
                    ad = context.Ads.FirstOrDefault(a => a.AdId == _adId.Value);

                    if (ad == null)
                    {
                        MessageService.ShowError("Объявление не найдено.\nВозможно, оно было удалено.");
                        return;
                    }
                }
                else
                {
                    ad = new Ads();
                    ad.UserId = SessionManager.CurrentUser.UserId;
                    context.Ads.Add(ad);
                }

                ad.Title = TxtTitle.Text.Trim();
                ad.Description = TxtDescription.Text;
                ad.Price = int.Parse(TxtPrice.Text);
                ad.PostDate = DpPostDate.SelectedDate.Value;
                ad.ImagePath = string.IsNullOrWhiteSpace(TxtImagePath.Text) ? null : TxtImagePath.Text.Trim();

                ad.CityId = ((Cities)CmbCity.SelectedItem).CityId;
                ad.CategoryId = ((Categories)CmbCategory.SelectedItem).CategoryId;
                ad.AdTypeId = ((AdTypes)CmbType.SelectedItem).AdTypeId;
                ad.AdStatusId = newStatusId;

                if (setCompletedNow)
                {
                    ProfitAmountDialog dialog = new ProfitAmountDialog();
                    dialog.Owner = Application.Current.MainWindow;

                    bool? ok = dialog.ShowDialog();

                    if (ok != true || dialog.ProfitAmount == null)
                    {
                        MessageService.ShowWarning("Завершение объявления отменено.\nВыберите другой статус или подтвердите сумму.");
                        return;
                    }

                    ad.ProfitAmount = dialog.ProfitAmount.Value;
                }

                context.SaveChanges();

                MessageService.ShowInfo("Данные объявления сохранены.");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageService.ShowError("Ошибка сохранения объявления.\n" + ex.Message);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(TxtTitle.Text))
            {
                MessageService.ShowWarning("Название объявления обязательно.\nВведите название и повторите сохранение.");
                return false;
            }

            if (TxtTitle.Text.Length > 200)
            {
                MessageService.ShowWarning("Название слишком длинное.\nМаксимум 200 символов.");
                return false;
            }

            int price;

            bool ok = int.TryParse(TxtPrice.Text, out price);

            if (!ok || price < 0)
            {
                MessageService.ShowWarning("Цена должна быть целым неотрицательным числом.\nПример: 12000");
                return false;
            }

            if (DpPostDate.SelectedDate == null)
            {
                MessageService.ShowWarning("Укажите дату публикации.");
                return false;
            }

            if (CmbCity.SelectedItem == null || CmbCategory.SelectedItem == null || CmbType.SelectedItem == null || CmbStatus.SelectedItem == null)
            {
                MessageService.ShowWarning("Выберите город, категорию, тип и статус.\nПроверьте выпадающие списки.");
                return false;
            }

            return true;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
