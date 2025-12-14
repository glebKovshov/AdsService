using System;
using System.Windows;

namespace AdsService.Windows
{
    public partial class ProfitAmountDialog : Window
    {
        public int? ProfitAmount { get; private set; }

        public ProfitAmountDialog()
        {
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            int value;

            bool ok = int.TryParse(TxtAmount.Text, out value);

            if (!ok || value < 0)
            {
                Random random = new Random();
                MessageBox.Show(
                    $"Введите целое неотрицательное число.\nПример: {random.Next(1000, 100_000)}",
                    "Ошибка ввода",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            ProfitAmount = value;
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
