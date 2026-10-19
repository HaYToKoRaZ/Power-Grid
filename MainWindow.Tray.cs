using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PowerGridHaYTooL.Models;
using PowerGridHaYTooL.Services;

namespace PowerGridHaYTooL
{
    public partial class MainWindow
    {
        private void TrayContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
            {
                menu.Focus();
            }
            UpdateTraySmartSwitchMenu();
        }

        private void UpdateTraySmartSwitchMenu()
        {
            try
            {
                if (TraySmartSwitchToggle == null || TraySmartSwitchStatus == null) return;

                var settings = SmartSwitchService.GetSettings();
                var status = SmartSwitchService.GetCurrentStatus();
                bool isEng = ConfigService.CurrentLanguage == "English";

                if (TrayPlansHeader != null)
                {
                    TrayPlansHeader.Header = isEng ? "⚡ POWER SCHEMES" : "⚡ GÜÇ DÜZENLERİ";
                }

                TraySmartSwitchToggle.IsChecked = settings.IsEnabled;
                TraySmartSwitchToggle.Header = settings.IsEnabled
                    ? (isEng ? "⚡ Smart GPU Auto Switch: [ON]" : "⚡ Akıllı GPU Güç Anahtarı: [AÇIK]")
                    : (isEng ? "⚡ Smart GPU Auto Switch: [OFF]" : "⚡ Akıllı GPU Güç Anahtarı: [KAPALI]");

                TraySmartSwitchSettings.Header = isEng ? "⚙️ Smart GPU Settings..." : "⚙️ Akıllı GPU Ayarları...";

                if (settings.IsEnabled)
                {
                    TraySmartSwitchStatus.Visibility = Visibility.Visible;
                    TraySmartSwitchStatus.Header = isEng
                        ? $"📊 Status: GPU {status.GpuUsagePercent:F0}% (Tier {status.CurrentTier})"
                        : $"📊 Durum: GPU %{status.GpuUsagePercent:F0} ({status.CurrentTier}. Kademe)";
                }
                else
                {
                    TraySmartSwitchStatus.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }

        private void TraySmartSwitchToggle_Click(object sender, RoutedEventArgs e)
        {
            bool newState = SmartSwitchService.ToggleEnabled();
            RefreshData(false);
            UpdateTraySmartSwitchMenu();

            bool isEng = ConfigService.CurrentLanguage == "English";
            string title = isEng ? "🎮 Smart GPU Auto Switch" : "🎮 Akıllı GPU Otomatik Güç Anahtarı";
            string msg = newState
                ? (isEng ? "Smart GPU Switch is now ENABLED! Monitoring GPU load." : "Akıllı GPU Güç Anahtarı AÇILDI! GPU yükü takip ediliyor.")
                : (isEng ? "Smart GPU Switch is now DISABLED." : "Akıllı GPU Güç Anahtarı KAPATILDI.");

            UI.ToastNotificationWindow.ShowToast(title, newState ? (isEng ? "Active" : "Aktif") : (isEng ? "Off" : "Kapalı"), msg, newState ? "⚡" : "⭕");
        }

        private void OnSmartStatusUpdated(SmartSwitchStatus status)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (MyNotifyIcon != null && ConfigService.ShowSystemTray)
                {
                    var activePlan = _plans.FirstOrDefault(p => p.IsActive);
                    string activeName = activePlan?.Name ?? (ConfigService.CurrentLanguage == "English" ? "None" : "Yok");
                    string label = ConfigService.CurrentLanguage == "English" ? "Active Plan" : "Aktif Plan";
                    string smartInfo = ConfigService.CurrentLanguage == "English"
                        ? $"\n⚡ Smart GPU: {status.GpuUsagePercent:F0}% (Tier {status.CurrentTier})"
                        : $"\n⚡ Akıllı GPU: %{status.GpuUsagePercent:F0} ({status.CurrentTier}. Kademe)";
                    MyNotifyIcon.ToolTipText = $"Power Grid HaYTooL\n{label}: {activeName}{smartInfo}";
                }
            }));
        }

        private void OnSystemTrayStateRequested(bool activate)
        {
            Dispatcher.InvokeAsync(() =>
            {
                MenuShowSystemTray.IsChecked = activate;
                MenuHideMinimized.IsChecked = activate;
                MenuCloseToTray.IsChecked = activate;
                MyNotifyIcon.Visibility = activate ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void OnSmartPlanSwitched(string newPlanGuid, string reason)
        {
            Dispatcher.InvokeAsync(() =>
            {
                RefreshData(false);
                var plan = _plans.FirstOrDefault(p => p.Guid.Equals(newPlanGuid, StringComparison.OrdinalIgnoreCase));
                string planName = plan?.Name ?? newPlanGuid;

                var settings = SmartSwitchService.GetSettings();
                if (settings.ShowNotification)
                {
                    string title = ConfigService.CurrentLanguage == "English"
                        ? "🎮 Smart Power Switch"
                        : "🎮 Akıllı Güç Anahtarı";

                    // Saatin yanında garantili modern Toast Bildirimi göster
                    string icon = planName.Contains("Yüksek", StringComparison.OrdinalIgnoreCase) ||
                                  planName.Contains("Nihai", StringComparison.OrdinalIgnoreCase) ||
                                  planName.Contains("Ultimate", StringComparison.OrdinalIgnoreCase) ||
                                  planName.Contains("High", StringComparison.OrdinalIgnoreCase) ? "🚀" : "🌱";

                    UI.ToastNotificationWindow.ShowToast(title, planName, reason, icon);

                    // Ayrıca Tray Icon balonu da tetikle (varsa)
                    if (MyNotifyIcon != null && ConfigService.ShowSystemTray)
                    {
                        try
                        {
                            string msg = ConfigService.CurrentLanguage == "English"
                                ? $"Switched to '{planName}'\nReason: {reason}"
                                : $"'{planName}' planına geçildi!\nNeden: {reason}";
                            MyNotifyIcon.ShowNotification(title, msg, H.NotifyIcon.Core.NotificationIcon.Info);
                        }
                        catch { }
                    }
                }
            });
        }

        private void MyNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void BtnShow_Click(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }
    }
}
