using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using PowerGridHaYTooL.Models;
using PowerGridHaYTooL.Services;

namespace PowerGridHaYTooL.UI
{
    public class UserPlanOption
    {
        public string Guid { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;
    }

    public partial class UserStartupManagerWindow : Window
    {
        private List<PowerPlanItem> _allPlans = new();
        public ObservableCollection<UserStartupItem> Users { get; set; } = new();
        public List<UserPlanOption> PlanOptions { get; set; } = new();

        public UserStartupManagerWindow()
        {
            InitializeComponent();
            ThemeService.ApplyTheme(ConfigService.CurrentTheme, this);
            DataContext = this;
            ApplyLocalization();
            LoadUsersAndPlans();
        }

        private void ApplyLocalization()
        {
            bool isEn = ConfigService.CurrentLanguage == "English";
            if (isEn)
            {
                Title = "Windows User Startup Power Plans";
                TxtHeaderTitle.Text = "Windows User Startup Power Plans";
                TxtHeaderSubtitle.Text = "Configure which power plan automatically activates when each Windows user logs in.";
                BtnSave.Content = "💾 Save & Apply";
                BtnCancel.Content = "Close";
            }
        }

        private void LoadUsersAndPlans()
        {
            try
            {
                _allPlans = PowerManagerService.GetPowerPlans();

                // Plan Seçenekleri
                bool isEn = ConfigService.CurrentLanguage == "English";
                PlanOptions = new List<UserPlanOption>
                {
                    new UserPlanOption
                    {
                        Guid = "",
                        DisplayText = isEn ? "(None / System Default)" : "(Yok / Sistem Varsayılanı)"
                    }
                };

                foreach (var p in _allPlans)
                {
                    PlanOptions.Add(new UserPlanOption
                    {
                        Guid = p.Guid,
                        DisplayText = $"{p.PlanIcon} {p.Name}"
                    });
                }

                // Kullanıcıları yükle
                var userList = UserStartupService.GetLocalUsers(_allPlans);
                Users.Clear();
                foreach (var u in userList)
                {
                    Users.Add(u);
                }

                IcUsers.ItemsSource = Users;

                int configuredCount = Users.Count(u => !string.IsNullOrEmpty(u.SelectedPlanGuid));
                TxtStatus.Text = isEn
                    ? $"Found {Users.Count} user account(s). {configuredCount} user(s) configured."
                    : $"{Users.Count} kullanıcı hesabı bulundu. {configuredCount} kullanıcı için başlangıç planı tanımlı.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kullanıcılar yüklenirken hata oluştu: {ex.Message}", "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            bool isEn = ConfigService.CurrentLanguage == "English";
            int savedCount = 0;
            int failedCount = 0;

            foreach (var user in Users)
            {
                if (user.IsModified)
                {
                    bool ok = UserStartupService.SaveUserStartupPlan(user, user.SelectedPlanGuid, _allPlans);
                    if (ok) savedCount++;
                    else failedCount++;
                }
            }

            if (savedCount > 0 && failedCount == 0)
            {
                string msg = isEn
                    ? $"Successfully updated startup power plans for {savedCount} user(s)!"
                    : $"{savedCount} kullanıcı için başlangıç güç planları başarıyla kaydedildi ve uygulandı!";
                MessageBox.Show(msg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else if (savedCount > 0 && failedCount > 0)
            {
                string msg = isEn
                    ? $"{savedCount} user(s) updated, but {failedCount} user(s) could not be modified (check administrator permissions)."
                    : $"{savedCount} kullanıcı güncellendi, ancak {failedCount} kullanıcı için izin yetersizliği nedeniyle kaydedilemedi (Yönetici olarak çalıştırmayı deneyin).";
                MessageBox.Show(msg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Warning);
                DialogResult = true;
                Close();
            }
            else if (failedCount > 0)
            {
                string msg = isEn
                    ? "Could not update user startup plans. Please run Power Grid HaYTooL as Administrator."
                    : "Kullanıcı başlangıç planları kaydedilemedi. Lütfen uygulamayı Yönetici Olarak Çalıştırın.";
                MessageBox.Show(msg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
