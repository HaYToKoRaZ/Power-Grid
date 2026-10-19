using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using PowerGridHaYTooL.Models;
using PowerGridHaYTooL.Services;

namespace PowerGridHaYTooL.UI
{
    public partial class SmartAutoSwitchWindow : Window
    {
        private List<PowerPlanItem> _plans = new();

        public SmartAutoSwitchWindow()
        {
            InitializeComponent();
            ThemeService.ApplyTheme(ConfigService.CurrentTheme, this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyLocalization();
            LoadPlansAndSettings();
            SmartSwitchService.StatusUpdated += OnStatusUpdated;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SmartSwitchService.StatusUpdated -= OnStatusUpdated;
        }

        private void ApplyLocalization()
        {
            bool isEn = ConfigService.CurrentLanguage == "English";
            if (isEn)
            {
                Title = "🎮 3-Tier Smart GPU Auto Power Switch";
                TxtTitle.Text = "3-Tier Smart GPU Auto Power Switch";
                TxtSubTitle.Text = "Automatically switches between 3 power plans based on real-time GPU load (%0-10 Idle, %10-30 Medium, %30-100 High).";
                TxtTip.Text = "💡 Background monitoring runs seamlessly via System Tray mode.";
                ChkEnableService.Content = "Active";

                TxtLiveHeader.Text = "⚡ Live GPU Telemetry & Active Tier";
                TxtForegroundGpuLabel.Text = "Foreground GPU Load:";
                TxtForegroundAppLabel.Text = "Foreground App: ";
                TxtCurrentPlanLabel.Text = "Active Plan: ";
                TxtLiveHzLabel.Text = "Display: ";
                TxtHzSwitchLabel.Text = "Automatically Adjust Monitor Refresh Rate (Hz) Per Tier";

                TxtLowPlanLabel.Text = "Tier 1: Idle / Low Load Power Plan";
                TxtMediumPlanLabel.Text = "Tier 2: Standard / Medium Load Plan";
                TxtHighPlanLabel.Text = "Tier 3: High GPU / Gaming Power Plan";

                TxtSldLowLabel.Text = "Tier 1-2 Threshold:";
                TxtSldHighLabel.Text = "Tier 2-3 Threshold:";
                TxtSldTriggerLabel.Text = "⏱️ Scale-Up Delay:";
                TxtSldRevertLabel.Text = "⏳ Scale-Down Delay:";

                ChkNotification.Content = "Show Notification & Play Sound on Power Plan Change";
                BtnReset.Content = "Reset to Defaults";
                BtnClose.Content = "Close";
                BtnSave.Content = "Save & Apply";
            }
            else
            {
                Title = "🎮 3 Kademeli Akıllı GPU Güç Anahtarı";
                TxtTitle.Text = "3 Kademeli Akıllı GPU Güç Anahtarı";
                TxtSubTitle.Text = "GPU yüküne göre (%0-10 Boşta, %10-30 Orta, %30-100 Yüksek) 3 ayrı güç planı arasında otomatik geçiş yapar.";
                TxtTip.Text = "💡 Arka planda kesintisiz izleme için Sistem Tepsisi (Tray) modu kullanılır.";
                ChkEnableService.Content = "Aktif";

                TxtLiveHeader.Text = "⚡ Canlı GPU Telemetrisi & Aktif Kademe";
                TxtForegroundGpuLabel.Text = "Ön Plan GPU Kullanımı:";
                TxtForegroundAppLabel.Text = "Ön Plan Süreci: ";
                TxtCurrentPlanLabel.Text = "Mevcut Güç Planı: ";
                TxtLiveHzLabel.Text = "Ekran: ";
                TxtHzSwitchLabel.Text = "Kademelere Göre Monitör Yenileme Hızını (Hz) Otomatik Ayarla";

                TxtLowPlanLabel.Text = "1. Kademe: Boşta / Düşük Kullanım Planı";
                TxtMediumPlanLabel.Text = "2. Kademe: Standart / Orta Seviye Plan";
                TxtHighPlanLabel.Text = "3. Kademe: Yüksek GPU / Oyun Güç Planı";

                TxtSldLowLabel.Text = "1-2. Kademe Eşiği:";
                TxtSldHighLabel.Text = "2-3. Kademe Eşiği:";
                TxtSldTriggerLabel.Text = "⏱️ Yükselme Gecikmesi:";
                TxtSldRevertLabel.Text = "⏳ Düşme Gecikmesi:";

                ChkNotification.Content = "Güç Planı Değiştiğinde Bildirim Göster ve Ses Çal";
                BtnReset.Content = "Varsayılanlara Sıfırla";
                BtnClose.Content = "Kapat";
                BtnSave.Content = "Kaydet ve Uygula";
            }
        }

        private void LoadPlansAndSettings()
        {
            bool isEn = ConfigService.CurrentLanguage == "English";
            _plans = PowerManagerService.GetPowerPlans();
            CmbLowPlan.ItemsSource = _plans;
            CmbMediumPlan.ItemsSource = _plans;
            CmbHighPlan.ItemsSource = _plans;

            // Desteklenen Monitör Yenileme Hızlarını (Hz) Yükle
            var supportedHz = DisplayService.GetSupportedRefreshRates();
            var hzItems = new List<DisplayHzItem>
            {
                new DisplayHzItem { Hz = 0, Display = isEn ? "⚡ Keep Current" : "⚡ Değiştirme (Mevcut)" }
            };

            foreach (var hz in supportedHz)
            {
                hzItems.Add(new DisplayHzItem { Hz = hz, Display = $"{hz} Hz" });
            }

            CmbLowHz.ItemsSource = hzItems;
            CmbMediumHz.ItemsSource = new List<DisplayHzItem>(hzItems);
            CmbHighHz.ItemsSource = new List<DisplayHzItem>(hzItems);

            int currentHz = DisplayService.GetCurrentRefreshRate();
            TxtLiveHzVal.Text = $"{currentHz} Hz";

            var settings = SmartSwitchService.GetSettings();

            ChkEnableService.IsChecked = settings.IsEnabled;

            // 1. Kademe: Boşta / Düşük Plan (Öncelik: Kayıtlı -> Tasarruf -> Dengeli -> İlk)
            if (!string.IsNullOrWhiteSpace(settings.LowPlanGuid) && _plans.Any(p => p.Guid == settings.LowPlanGuid))
            {
                CmbLowPlan.SelectedValue = settings.LowPlanGuid;
            }
            else
            {
                var saver = _plans.FirstOrDefault(p => p.Name.Contains("Eco Stealth", StringComparison.OrdinalIgnoreCase))
                         ?? _plans.FirstOrDefault(p => p.Name.Contains("Tasarruf", StringComparison.OrdinalIgnoreCase) ||
                                                       p.Name.Contains("Saver", StringComparison.OrdinalIgnoreCase) ||
                                                       p.Name.Contains("Sessiz", StringComparison.OrdinalIgnoreCase) ||
                                                       p.Name.Contains("Quiet", StringComparison.OrdinalIgnoreCase));
                CmbLowPlan.SelectedItem = saver ?? _plans.FirstOrDefault();
            }

            // 2. Kademe: Orta Seviye Plan (Öncelik: Kayıtlı -> Dengeli -> İlk)
            if (!string.IsNullOrWhiteSpace(settings.MediumPlanGuid) && _plans.Any(p => p.Guid == settings.MediumPlanGuid))
            {
                CmbMediumPlan.SelectedValue = settings.MediumPlanGuid;
            }
            else
            {
                var balanced = _plans.FirstOrDefault(p => p.Name.Contains("Smart Balanced", StringComparison.OrdinalIgnoreCase))
                            ?? _plans.FirstOrDefault(p => p.Name.Contains("Dengeli", StringComparison.OrdinalIgnoreCase) ||
                                                          p.Name.Contains("Balanced", StringComparison.OrdinalIgnoreCase));
                CmbMediumPlan.SelectedItem = balanced ?? _plans.FirstOrDefault();
            }

            // 3. Kademe: Yüksek GPU / Oyun Planı (Öncelik: Kayıtlı -> Apex Performance -> Nihai -> Yüksek -> İlk)
            if (!string.IsNullOrWhiteSpace(settings.HighPlanGuid) && _plans.Any(p => p.Guid == settings.HighPlanGuid))
            {
                CmbHighPlan.SelectedValue = settings.HighPlanGuid;
            }
            else
            {
                var highPerf = _plans.FirstOrDefault(p => p.Name.Contains("Apex Performance", StringComparison.OrdinalIgnoreCase))
                            ?? _plans.FirstOrDefault(p => p.Name.Contains("Nihai", StringComparison.OrdinalIgnoreCase) ||
                                                          p.Name.Contains("Ultimate", StringComparison.OrdinalIgnoreCase) ||
                                                          p.Name.Contains("Yüksek", StringComparison.OrdinalIgnoreCase) ||
                                                          p.Name.Contains("High", StringComparison.OrdinalIgnoreCase));
                CmbHighPlan.SelectedItem = highPerf ?? _plans.FirstOrDefault();
            }

            // Hz Ayarlarını Seç
            ChkEnableHzSwitch.IsChecked = settings.IsHzSwitchEnabled;

            CmbLowHz.SelectedValue = settings.LowHz;
            if (CmbLowHz.SelectedIndex < 0) CmbLowHz.SelectedIndex = 0;

            CmbMediumHz.SelectedValue = settings.MediumHz;
            if (CmbMediumHz.SelectedIndex < 0) CmbMediumHz.SelectedIndex = 0;

            CmbHighHz.SelectedValue = settings.HighHz;
            if (CmbHighHz.SelectedIndex < 0) CmbHighHz.SelectedIndex = 0;

            UpdateHzCombosState();

            SldLowThreshold.Value = settings.LowThresholdPercent > 0 ? settings.LowThresholdPercent : 10;
            SldHighThreshold.Value = settings.HighThresholdPercent > 0 ? settings.HighThresholdPercent : 30;
            SldTriggerSeconds.Value = settings.TriggerSeconds > 0 ? settings.TriggerSeconds : 3;
            SldRevertSeconds.Value = settings.RevertSeconds > 0 ? settings.RevertSeconds : 6;
            ChkNotification.IsChecked = settings.ShowNotification;

            UpdateThresholdLabels();
            UpdateDurationLabels();
        }

        private void UpdateHzCombosState()
        {
            bool enabled = ChkEnableHzSwitch.IsChecked == true;
            if (CmbLowHz != null) CmbLowHz.IsEnabled = enabled;
            if (CmbMediumHz != null) CmbMediumHz.IsEnabled = enabled;
            if (CmbHighHz != null) CmbHighHz.IsEnabled = enabled;
        }

        private void ChkEnableHzSwitch_CheckedChanged(object sender, RoutedEventArgs e)
        {
            UpdateHzCombosState();
        }

        private void Thresholds_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SldLowThreshold == null || SldHighThreshold == null) return;

            // Çakışmayı engelle (Low her zaman High'dan en az 5 birim küçük olmalı)
            if (SldLowThreshold.Value >= SldHighThreshold.Value - 4)
            {
                if (sender == SldLowThreshold)
                    SldLowThreshold.Value = SldHighThreshold.Value - 5;
                else
                    SldHighThreshold.Value = SldLowThreshold.Value + 5;
            }

            UpdateThresholdLabels();
        }

        private void UpdateThresholdLabels()
        {
            if (TxtLowRangeLabel == null) return;

            int low = (int)SldLowThreshold.Value;
            int high = (int)SldHighThreshold.Value;

            TxtValLowThreshold.Text = $"%{low}";
            TxtValHighThreshold.Text = $"%{high}";

            TxtLowRangeLabel.Text = $"[%0 - %{low}]";
            TxtMediumRangeLabel.Text = $"[%{low} - %{high}]";
            TxtHighRangeLabel.Text = $"[%{high} - %100]";
        }

        private void SldTriggerSeconds_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateDurationLabels();
        }

        private void SldRevertSeconds_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateDurationLabels();
        }

        private void UpdateDurationLabels()
        {
            if (TxtTriggerLabel != null && SldTriggerSeconds != null)
            {
                TxtTriggerLabel.Text = $"{(int)SldTriggerSeconds.Value} sn";
            }
            if (TxtRevertLabel != null && SldRevertSeconds != null)
            {
                TxtRevertLabel.Text = $"{(int)SldRevertSeconds.Value} sn";
            }
        }

        private void OnStatusUpdated(SmartSwitchStatus status)
        {
            Dispatcher.Invoke(() =>
            {
                PrgGpu3D.Value = status.GpuUsagePercent;
                TxtGpu3DVal.Text = $"{status.GpuUsagePercent:F0}%";
                TxtForegroundApp.Text = string.IsNullOrEmpty(status.ForegroundProcessName) ? "Masaüstü" : status.ForegroundProcessName;
                TxtCurrentPlanVal.Text = status.ActivePlanName;
                TxtLiveHzVal.Text = $"{status.CurrentHz} Hz";
                TxtLiveStatusMessage.Text = status.StatusMessage;

                bool isEn = ConfigService.CurrentLanguage == "English";
                TxtLiveTierBadge.Text = status.CurrentTier switch
                {
                    3 => isEn ? "Tier 3: High" : "3. Kademe: Yüksek",
                    2 => isEn ? "Tier 2: Medium" : "2. Kademe: Orta",
                    _ => isEn ? "Tier 1: Idle/Low" : "1. Kademe: Düşük"
                };
            });
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            SldLowThreshold.Value = 10;
            SldHighThreshold.Value = 30;
            SldTriggerSeconds.Value = 3;
            SldRevertSeconds.Value = 6;
            ChkNotification.IsChecked = true;
            ChkEnableHzSwitch.IsChecked = false;
            CmbLowHz.SelectedIndex = 0;
            CmbMediumHz.SelectedIndex = 0;
            CmbHighHz.SelectedIndex = 0;
            UpdateHzCombosState();
            UpdateThresholdLabels();
            UpdateDurationLabels();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SmartSwitchSettings
            {
                IsEnabled = ChkEnableService.IsChecked == true,
                LowPlanGuid = CmbLowPlan.SelectedValue?.ToString() ?? "",
                MediumPlanGuid = CmbMediumPlan.SelectedValue?.ToString() ?? "",
                HighPlanGuid = CmbHighPlan.SelectedValue?.ToString() ?? "",
                IsHzSwitchEnabled = ChkEnableHzSwitch.IsChecked == true,
                LowHz = CmbLowHz.SelectedValue is int lhz ? lhz : 0,
                MediumHz = CmbMediumHz.SelectedValue is int mhz ? mhz : 0,
                HighHz = CmbHighHz.SelectedValue is int hhz ? hhz : 0,
                LowThresholdPercent = (int)SldLowThreshold.Value,
                HighThresholdPercent = (int)SldHighThreshold.Value,
                TriggerSeconds = (int)SldTriggerSeconds.Value,
                RevertSeconds = (int)SldRevertSeconds.Value,
                ShowNotification = ChkNotification.IsChecked == true
            };

            SmartSwitchService.UpdateSettings(settings);

            bool isEn = ConfigService.CurrentLanguage == "English";
            string msg = isEn 
                ? "3-Tier Smart GPU power & refresh rate switch settings saved and applied!" 
                : "3 Kademeli Akıllı GPU güç ve yenileme hızı (Hz) ayarları başarıyla kaydedildi ve uygulandı!";

            MessageBox.Show(msg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class DisplayHzItem
    {
        public int Hz { get; set; }
        public string Display { get; set; } = "";
    }
}
