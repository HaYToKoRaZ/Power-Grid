using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using PowerGridHaYTooL.Helpers;
using PowerGridHaYTooL.Models;
using PowerGridHaYTooL.Services;
using PowerGridHaYTooL.UI;

namespace PowerGridHaYTooL
{
    public partial class MainWindow : Window
    {
        private List<PowerPlanItem> _plans = new();
        private bool _isExplicitExit = false;
        private uint _restoreWindowMessage = 0;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            try
            {
                _restoreWindowMessage = ProcessHelper.RegisterWindowMessage(ProcessHelper.RestoreMessageName);
                var source = System.Windows.Interop.HwndSource.FromHwnd(new System.Windows.Interop.WindowInteropHelper(this).Handle);
                source?.AddHook(WndProc);
            }
            catch { }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (_restoreWindowMessage != 0 && msg == _restoreWindowMessage)
            {
                BringToFront();
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void BringToFront()
        {
            try
            {
                Show();
                if (WindowState == WindowState.Minimized)
                {
                    WindowState = WindowState.Normal;
                }
                Activate();
                Topmost = true;
                Topmost = false;
                Focus();
            }
            catch { }
        }

        public MainWindow()
        {
            InitializeComponent();
            ConfigService.Load();
            ApplySettingsAndLanguage();
            RefreshData();
            UpdateMenuOptions();
            AutoBackupService.TriggerAutoBackupAsync(_plans);

            // Uygulama açılışında arka planda sessizce yeni sürüm denetimi yap (2.5 sn gecikmeli)
            Task.Delay(2500).ContinueWith(_ => UpdateCheckService.CheckForUpdatesAsync());
        }

        private void ApplySettingsAndLanguage()
        {
            // Title & Version
            var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string verStr = ver != null ? $"{ver.Major}.{ver.Minor}" : "1.0";
            string titlePrefix = ConfigService.Get("Title", "Power Grid HaYTooL");
            Title = $"{titlePrefix} v{verStr}";
            TxtVersion.Text = $"{titlePrefix} v{verStr}";

            // Settings
            Topmost = ConfigService.AlwaysOnTop;
            MenuAlwaysOnTop.IsChecked = ConfigService.AlwaysOnTop;
            MenuHideMinimized.IsChecked = ConfigService.HideWhenMinimized;
            MenuCloseToTray.IsChecked = ConfigService.CloseToTray;
            MenuShowSystemTray.IsChecked = ConfigService.ShowSystemTray;
            MyNotifyIcon.Visibility = ConfigService.ShowSystemTray ? Visibility.Visible : Visibility.Collapsed;

            // Menus
            MnuFile.Header = ConfigService.Get("FileMenu", "Dosya");
            MnuApply.Header = ConfigService.Get("ApplySelected", "Seçileni Uygula");
            MnuNewPlan.Header = ConfigService.Get("NewPlan", "Yeni Güç Düzeni Oluştur");
            MnuDelete.Header = ConfigService.Get("DeleteSelected", "Listeden Seçileni Sil");
            MnuImport.Header = ConfigService.Get("ImportPlan", "Dosyadan Güç Düzeni Al...");
            MnuExport.Header = ConfigService.Get("ExportPlan", "Dışarıya Aktar...");
            MnuBackupAll.Header = ConfigService.Get("BackupAll", "Tüm Güç Düzenlerini Yedekle (Toplu)...");
            MnuRestoreAutoZip.Header = ConfigService.CurrentLanguage == "English" ? "📦 Restore from Automatic ZIP Backup..." : "📦 Otomatik ZIP Yedeğinden Geri Yükle...";
            MnuRestoreAll.Header = ConfigService.Get("RestoreAll", "Yedek Klasöründen Toplu İçe Aktar...");
            MnuExit.Header = ConfigService.Get("Exit", "Kapat");

            MnuEdit.Header = ConfigService.Get("EditMenu", "Düzen");
            MnuEditSettings.Header = ConfigService.CurrentLanguage == "English" ? "Edit Plan Settings..." : "Plan Ayarlarını Düzenle...";
            MnuToggleStartupPlan.Header = ConfigService.CurrentLanguage == "English" ? "📌 Set as User Startup Plan" : "📌 Oturum Açılış Planı Olarak Ayarla";
            MnuRename.Header = ConfigService.Get("Rename", "Yeniden Adlandır");
            MnuCreateShortcut.Header = ConfigService.Get("CreateShortcut", "Masaüstüne Kısayol Oluştur");
            MnuCopy.Header = ConfigService.Get("Copy", "Kopyala");
            MnuCopyGuid.Header = ConfigService.Get("CopyGuid", "GUID Kopyala");
            MnuCopyName.Header = ConfigService.Get("CopyName", "Plan İsmini Kopyala");

            MnuTemplates.Header = ConfigService.Get("TemplatesMenu", "Şablonlar");
            MnuAddAllCustom.Header = ConfigService.CurrentLanguage == "English" ? "🚀 Add All Custom Templates (HaYTooL & AMD)" : "🚀 Tüm Özel Şablonları Ekle (HaYTooL & AMD)";
            MnuAddHaYTooLAll.Header = ConfigService.CurrentLanguage == "English" ? "⚡ Add HaYTooL Power Profiles (3 Profiles)" : "⚡ Özel HaYTooL Profillerini Ekle (3 Profil)";
            MnuAddAmdAll.Header = ConfigService.CurrentLanguage == "English" ? "🔴 Add AMD Ryzen™ Profiles (2 Profiles)" : "🔴 AMD Ryzen™ Profillerini Ekle (2 Profil)";
            MnuUltimatePerf.Header = ConfigService.Get("UltimatePerf", "⚡ Nihai Performans Ekle");
            MnuIndividualTemplates.Header = ConfigService.CurrentLanguage == "English" ? "🔍 Add Individually..." : "🔍 Tek Tek Seçerek Ekle...";
            MnuHaYTooLApex.Header = ConfigService.CurrentLanguage == "English" ? "⚡ HaYTooL Apex Performance" : "⚡ HaYTooL Apex Performance Ekle";
            MnuHaYTooLBalanced.Header = ConfigService.CurrentLanguage == "English" ? "⚖️ HaYTooL Smart Balanced" : "⚖️ HaYTooL Smart Balanced Ekle";
            MnuHaYTooLEco.Header = ConfigService.CurrentLanguage == "English" ? "🌱 HaYTooL Eco Stealth" : "🌱 HaYTooL Eco Stealth Ekle";
            MnuAmdBalanced.Header = ConfigService.Get("AmdBalanced", "🔴 AMD Ryzen™ Dengeli Ekle");
            MnuAmdHighPerf.Header = ConfigService.Get("AmdHighPerf", "🚀 AMD Ryzen™ Yüksek Performans Ekle");
            MnuTemplatesRestoreZip.Header = ConfigService.CurrentLanguage == "English" ? "📦 Restore from Automatic ZIP Backup..." : "📦 Otomatik ZIP Yedeğinden Geri Yükle...";
            MnuRestore.Header = ConfigService.CurrentLanguage == "English" ? "🔄 Restore Windows Power Schemes to Defaults" : "🔄 Windows Güç Düzenlerini Varsayılana Döndür";

            MnuTools.Header = ConfigService.Get("ToolsMenu", "Araçlar");
            MnuSmartAutoSwitch.Header = ConfigService.CurrentLanguage == "English" ? "🎮 Smart Game & GPU Auto-Switch..." : "🎮 Akıllı GPU ve Oyun Otomatik Güç Anahtarı...";
            MnuComparePlans.Header = ConfigService.CurrentLanguage == "English" ? "⚖ Compare Power Plans..." : "⚖ Güç Düzenlerini Karşılaştır...";
            UpdateUnlockHiddenMenuState();
            MnuPowerCpl.Header = ConfigService.Get("PowerOptions", "Güç Seçenekleri");
            MnuMobility.Header = ConfigService.Get("MobilityCenter", "Windows Mobility Center");
            MnuPowerSleep.Header = ConfigService.Get("PowerSleepSettings", "Güç ve Uyku Ayarları");

            MnuOptions.Header = ConfigService.Get("OptionsMenu", "Seçenekler");
            MenuDesktopContext.Header = ConfigService.Get("DesktopContextMenu", "Masaüstü Sağ Tuş Menüsüne Ekle");
            MenuShowSystemTray.Header = ConfigService.Get("ShowSystemTrayOpt", "Sistem Tepsisinde Göster (System Tray)");
            MenuAlwaysOnTop.Header = ConfigService.Get("AlwaysOnTopOpt", "Pencere Her Zaman Üstte");
            MenuHideMinimized.Header = ConfigService.Get("HideWhenMinimizedOpt", "Küçültüldüğünde Pencereyi Gizle");
            MenuCloseToTray.Header = ConfigService.CurrentLanguage == "English" ? "Close to System Tray" : "Kapatıldığında Sistem Tepsisine Küçült";
            MenuAutoStart.Header = ConfigService.Get("AutoStartOpt", "Oturum Başlangıcında Çalıştır");
            MnuUserStartupInfo.Header = ConfigService.CurrentLanguage == "English" ? "👥 User Startup Power Plans Manager..." : "👥 Kullanıcı Başlangıç Planları Yöneticisi...";
            MnuLanguage.Header = ConfigService.Get("LanguageOpt", "Dil (Language)");
            MnuLangAuto.Header = ConfigService.CurrentLanguage == "English" ? "Windows Auto (System)" : "Windows Otomatik (Sistem)";
            bool isEnglish = ConfigService.CurrentLanguage == "English";
            MnuTheme.Header = isEnglish ? "Theme" : "Tema";
            MnuThemeAuto.Header = isEnglish ? "Windows Auto (System)" : "Windows Otomatik (Sistem)";
            MnuThemeDark.Header = isEnglish ? "🌅 Sunset Boulevard (Default Dark)" : "🌅 Gün Batımı (Varsayılan Koyu)";
            MnuThemeLight.Header = isEnglish ? "🌿 Botanical Garden (Default Light)" : "🌿 Botanik Bahçesi (Varsayılan Açık)";

            MnuThemeNordic.Header = isEnglish ? "🌌 Nordic Slate Navy" : "🌌 Nordic Gece Mavisi";
            MnuThemeObsidian.Header = isEnglish ? "⬛ Obsidian Black" : "⬛ Obsidyen Siyah";
            MnuThemeSilverBlue.Header = isEnglish ? "☀️ Silver Blue (Classic Light)" : "☀️ Gümüş Mavi (Klasik Açık)";

            MnuThemeOceanDepths.Header = isEnglish ? "🌊 Ocean Depths" : "🌊 Okyanus Derinliği (Ocean Depths)";
            MnuThemeMidnightGalaxy.Header = isEnglish ? "✨ Midnight Galaxy" : "✨ Gece Galaksisi (Midnight Galaxy)";
            MnuThemeTechInnovation.Header = isEnglish ? "⚡ Tech Innovation" : "⚡ Teknoloji & Siber (Tech Innovation)";
            MnuThemeForestCanopy.Header = isEnglish ? "🌲 Forest Canopy" : "🌲 Orman Yeşili (Forest Canopy)";
            MnuThemeArcticFrost.Header = isEnglish ? "❄️ Arctic Frost" : "❄️ Arktik Buzul (Arctic Frost)";
            MnuThemeDesertRose.Header = isEnglish ? "🌹 Desert Rose" : "🌹 Çöl Gülü (Desert Rose)";
            MnuThemeGoldenHour.Header = isEnglish ? "🍁 Golden Hour" : "🍁 Altın Saat (Golden Hour)";
            MnuThemeModernMinimalist.Header = isEnglish ? "🏢 Modern Minimalist" : "🏢 Modern Minimalist (Monochrome)";

            MnuHelp.Header = ConfigService.CurrentLanguage == "English" ? "Help" : "Yardım";
            MnuWebsite.Header = ConfigService.CurrentLanguage == "English" ? "🌐 Power Grid Website" : "🌐 Power Grid Web Sitesi";
            MnuPortal.Header = ConfigService.CurrentLanguage == "English" ? "🏛️ HaYTooL Portal" : "🏛️ HaYTooL Portalı";
            MnuGithub.Header = ConfigService.CurrentLanguage == "English" ? "🐙 GitHub Repository" : "🐙 GitHub Deposu";
            MnuFeedback.Header = ConfigService.CurrentLanguage == "English" ? "💬 Send Feedback..." : "💬 Geri Bildirim Gönder...";
            MnuCheckUpdate.Header = ConfigService.CurrentLanguage == "English" ? "🔄 Check for Updates..." : "🔄 Güncellemeleri Denetle...";
            MnuAbout.Header = ConfigService.CurrentLanguage == "English" ? "ℹ️ About Power Grid HaYTooL..." : "ℹ️ Hakkında (About)...";
            if (TrayCheckUpdateItem != null)
                TrayCheckUpdateItem.Header = ConfigService.CurrentLanguage == "English" ? "🔄 Check for Updates..." : "🔄 Güncellemeleri Denetle...";
            if (TxtFeedbackStatus != null)
                TxtFeedbackStatus.Text = ConfigService.CurrentLanguage == "English" ? "Feedback" : "Geri Bildirim";

            // Action Bar Localization
            if (TxtApplyBtnText != null)
                TxtApplyBtnText.Text = ConfigService.Get("ApplySelected", "Seçileni Uygula");
            if (TxtSettingsBtnText != null)
                TxtSettingsBtnText.Text = ConfigService.CurrentLanguage == "English" ? "Settings" : "Ayarlar";
            if (TxtCompareBtnText != null)
                TxtCompareBtnText.Text = ConfigService.CurrentLanguage == "English" ? "Compare" : "Karşılaştır";

            // List Right-Click Context Menu
            CtxApply.Header = ConfigService.CurrentLanguage == "English" ? "Apply Selected (Set Active)" : "Seçileni Uygula (Aktif Yap)";
            CtxToggleStartupPlan.Header = ConfigService.CurrentLanguage == "English" ? "📌 Set as User Startup Plan" : "📌 Oturum Açılışında Bu Güç Planına Geç";
            CtxCreateShortcut.Header = ConfigService.CurrentLanguage == "English" ? "Create Desktop Shortcut (.cmd)" : "Masaüstüne Geçiş Kısayolu (.cmd) Oluştur";
            CtxNewPlan.Header = ConfigService.CurrentLanguage == "English" ? "Create New Plan (Clone)" : "Yeni Güç Düzeni Oluştur (Klonla)";
            CtxEditSettings.Header = ConfigService.CurrentLanguage == "English" ? "Edit Plan Settings (CPU / Timeouts)..." : "Plan Ayarlarını Düzenle (İşlemci / Süreler)...";
            CtxCompare.Header = ConfigService.CurrentLanguage == "English" ? "Compare Power Plans..." : "Güç Düzenlerini Karşılaştır...";
            CtxRename.Header = ConfigService.Get("Rename", "Yeniden Adlandır");
            CtxExport.Header = ConfigService.Get("ExportPlan", "Dışarıya Aktar (.pow)...");
            CtxCopyGuid.Header = ConfigService.Get("CopyGuid", "GUID Kopyala");
            CtxCopyName.Header = ConfigService.Get("CopyName", "Plan İsmini Kopyala");
            CtxDelete.Header = ConfigService.Get("DeleteSelected", "Sil");

            // Tray Context Menu
            TrayShowItem.Header = ConfigService.CurrentLanguage == "English" ? "Show Main Window" : "Ana Pencereyi Göster";
            MenuPlansContainer.Header = ConfigService.CurrentLanguage == "English" ? "Power Plans List" : "Güç Düzenleri Listesi";
            TrayContextMenuItem.Header = ConfigService.Get("DesktopContextMenu", "Masaüstü Sağ Tuş Menüsüne Ekle");
            TrayPowerCplItem.Header = ConfigService.Get("PowerOptions", "Güç Seçenekleri");
            TrayAboutItem.Header = ConfigService.CurrentLanguage == "English" ? "About & Feedback..." : "Hakkında & Geri Bildirim...";
            TrayExitItem.Header = ConfigService.Get("Exit", "Kapat");
            UpdateTraySmartSwitchMenu();

            // Apply Theme & Checks
            ThemeService.ApplyTheme(ConfigService.CurrentTheme, this);
            UpdateThemeMenuChecks();
            UpdateLanguageMenuChecks();
        }

        private void UpdateLanguageMenuChecks()
        {
            MnuLangAuto.IsChecked = string.Equals(ConfigService.LanguageSetting, "Auto", StringComparison.OrdinalIgnoreCase);
            MnuLangTurkish.IsChecked = string.Equals(ConfigService.LanguageSetting, "Turkish", StringComparison.OrdinalIgnoreCase);
            MnuLangEnglish.IsChecked = string.Equals(ConfigService.LanguageSetting, "English", StringComparison.OrdinalIgnoreCase);
        }

        private void UpdateThemeMenuChecks()
        {
            string cur = ConfigService.CurrentTheme ?? "Auto";
            MnuThemeAuto.IsChecked = string.Equals(cur, "Auto", StringComparison.OrdinalIgnoreCase);
            MnuThemeDark.IsChecked = string.Equals(cur, "Dark", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(cur, "SunsetBoulevard", StringComparison.OrdinalIgnoreCase);
            MnuThemeLight.IsChecked = string.Equals(cur, "Light", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(cur, "BotanicalGarden", StringComparison.OrdinalIgnoreCase);

            MnuThemeNordic.IsChecked = string.Equals(cur, "Nordic", StringComparison.OrdinalIgnoreCase);
            MnuThemeObsidian.IsChecked = string.Equals(cur, "Obsidian", StringComparison.OrdinalIgnoreCase) ||
                                         string.Equals(cur, "Black", StringComparison.OrdinalIgnoreCase);
            MnuThemeSilverBlue.IsChecked = string.Equals(cur, "SilverBlue", StringComparison.OrdinalIgnoreCase);

            MnuThemeOceanDepths.IsChecked = string.Equals(cur, "OceanDepths", StringComparison.OrdinalIgnoreCase);
            MnuThemeMidnightGalaxy.IsChecked = string.Equals(cur, "MidnightGalaxy", StringComparison.OrdinalIgnoreCase);
            MnuThemeTechInnovation.IsChecked = string.Equals(cur, "TechInnovation", StringComparison.OrdinalIgnoreCase);
            MnuThemeForestCanopy.IsChecked = string.Equals(cur, "ForestCanopy", StringComparison.OrdinalIgnoreCase);
            MnuThemeArcticFrost.IsChecked = string.Equals(cur, "ArcticFrost", StringComparison.OrdinalIgnoreCase);
            MnuThemeDesertRose.IsChecked = string.Equals(cur, "DesertRose", StringComparison.OrdinalIgnoreCase);
            MnuThemeGoldenHour.IsChecked = string.Equals(cur, "GoldenHour", StringComparison.OrdinalIgnoreCase);
            MnuThemeModernMinimalist.IsChecked = string.Equals(cur, "ModernMinimalist", StringComparison.OrdinalIgnoreCase);
        }

        private void ThemeAuto_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.CurrentTheme = "Auto";
            ConfigService.Save();
            ThemeService.ApplyTheme("Auto", this);
            UpdateThemeMenuChecks();
        }

        private void ThemeDark_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.CurrentTheme = "Dark";
            ConfigService.Save();
            ThemeService.ApplyTheme("Dark", this);
            UpdateThemeMenuChecks();
        }

        private void ThemeObsidian_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.CurrentTheme = "Obsidian";
            ConfigService.Save();
            ThemeService.ApplyTheme("Obsidian", this);
            UpdateThemeMenuChecks();
        }

        private void ThemeLight_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.CurrentTheme = "Light";
            ConfigService.Save();
            ThemeService.ApplyTheme("Light", this);
            UpdateThemeMenuChecks();
        }

        private void ThemeCustom_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is string themeName)
            {
                ConfigService.CurrentTheme = themeName;
                ConfigService.Save();
                ThemeService.ApplyTheme(themeName, this);
                UpdateThemeMenuChecks();
            }
        }

        private void LangAuto_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.LanguageSetting = "Auto";
            ConfigService.CurrentLanguage = ConfigService.DetectSystemLanguage();
            ConfigService.Save();
            ApplySettingsAndLanguage();
            RefreshData();
        }

        private void LangTurkish_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.LanguageSetting = "Turkish";
            ConfigService.CurrentLanguage = "Turkish";
            ConfigService.Save();
            ApplySettingsAndLanguage();
            RefreshData();
        }

        private void LangEnglish_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.LanguageSetting = "English";
            ConfigService.CurrentLanguage = "English";
            ConfigService.Save();
            ApplySettingsAndLanguage();
            RefreshData();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Initial render complete
            SmartSwitchService.PlanSwitched += OnSmartPlanSwitched;
            SmartSwitchService.StatusUpdated += OnSmartStatusUpdated;
            SmartSwitchService.SystemTrayStateRequested += OnSystemTrayStateRequested;
            if (SmartSwitchService.GetSettings().IsEnabled)
            {
                SmartSwitchService.Start();
            }
        }

        private void RefreshData(bool triggerBackup = false)
        {
            _plans = PowerManagerService.GetPowerPlans();

            // Kullanıcı başlangıç güç planı senkronizasyonu
            string startupGuid = ConfigService.UserStartupPlanGuid;
            if (string.IsNullOrEmpty(startupGuid))
            {
                startupGuid = RegistryService.GetUserStartupPlanGuidFromRegistry() ?? "";
                if (!string.IsNullOrEmpty(startupGuid))
                {
                    ConfigService.UserStartupPlanGuid = startupGuid;
                    ConfigService.Save();
                }
            }

            foreach (var plan in _plans)
            {
                plan.IsUserStartup = !string.IsNullOrEmpty(startupGuid) &&
                                     string.Equals(plan.Guid, startupGuid, StringComparison.OrdinalIgnoreCase);
            }

            LstPlans.ItemsSource = null;
            LstPlans.ItemsSource = _plans;

            int activeIndex = _plans.FindIndex(p => p.IsActive);
            if (activeIndex >= 0)
            {
                LstPlans.SelectedIndex = activeIndex;
            }

            // Pencere yüksekliğini güç planı sayısına tam uyacak şekilde ayarla
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
            {
                if (WindowState == WindowState.Normal)
                {
                    SizeToContent = SizeToContent.Height;
                }
            }));

            string statusPrefix = ConfigService.Get("StatusReady", "Hazır. Toplam Güç Düzeni:");
            TxtStatus.Text = $"{statusPrefix} {_plans.Count}";

            // Sistem Tepsisi (Tray) menüsünü güncelle
            MenuPlansContainer.Items.Clear();
            foreach (var plan in _plans)
            {
                var mi = new MenuItem
                {
                    Header = $"{plan.PlanIcon} {plan.Name}",
                    IsChecked = plan.IsActive
                };
                string targetGuid = plan.Guid;
                mi.Click += (s, e) =>
                {
                    PowerManagerService.SetActivePlan(targetGuid);
                    RefreshData(true);
                };
                MenuPlansContainer.Items.Add(mi);
            }

            // Sistem Tepsisi (Tray) simgesinin üzerine gelindiğinde aktif güç planının adını göster
            if (MyNotifyIcon != null)
            {
                var activePlan = _plans.FirstOrDefault(p => p.IsActive);
                string activeName = activePlan?.Name ?? (ConfigService.CurrentLanguage == "English" ? "None" : "Yok");
                string label = ConfigService.CurrentLanguage == "English" ? "Active Plan" : "Aktif Plan";
                var smartSettings = SmartSwitchService.GetSettings();
                string smartInfo = "";
                if (smartSettings.IsEnabled)
                {
                    var status = SmartSwitchService.GetCurrentStatus();
                    smartInfo = ConfigService.CurrentLanguage == "English"
                        ? $"\n⚡ Smart GPU: {status.GpuUsagePercent:F0}% (Tier {status.CurrentTier})"
                        : $"\n⚡ Akıllı GPU: %{status.GpuUsagePercent:F0} ({status.CurrentTier}. Kademe)";
                }
                MyNotifyIcon.ToolTipText = $"Power Grid HaYTooL\n{label}: {activeName}{smartInfo}";
            }

            UpdateTraySmartSwitchMenu();

            if (triggerBackup)
            {
                AutoBackupService.TriggerAutoBackupAsync(_plans);
            }

            // Masaüstü sağ tuş menüsü etkinse, güncel plan listesiyle otomatik senkronize et
            if (RegistryService.IsDesktopContextMenuAdded())
            {
                RegistryService.AddDesktopContextMenu(_plans);
            }
        }

        private void UpdateMenuOptions()
        {
            MenuDesktopContext.IsChecked = RegistryService.IsDesktopContextMenuAdded();
            MenuAutoStart.IsChecked = RegistryService.IsAutoStartEnabled();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
            UpdateMenuOptions();
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            if (LstPlans.SelectedItem is PowerPlanItem selected)
            {
                if (PowerManagerService.SetActivePlan(selected.Guid))
                {
                    RefreshData(true);
                    if (RegistryService.IsDesktopContextMenuAdded())
                    {
                        RegistryService.AddDesktopContextMenu(_plans);
                    }
                }
            }
        }

        private void LstPlans_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnApply_Click(sender, e);
        }

        private void LstPlans_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
            {
                BtnDelete_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnApply_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.F2)
            {
                BtnRename_Click(sender, e);
                e.Handled = true;
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F5)
            {
                BtnRefresh_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Delete && LstPlans.SelectedItem != null)
            {
                BtnDelete_Click(sender, e);
                e.Handled = true;
            }
        }

        private void BtnNewPlan_Click(object sender, RoutedEventArgs e)
        {
            string defaultName = ConfigService.CurrentLanguage == "English" ? "New Power Plan" : "Yeni Güç Düzeni";
            if (LstPlans.SelectedItem is PowerPlanItem selected)
            {
                string copySuffix = ConfigService.CurrentLanguage == "English" ? "(Copy)" : "(Kopya)";
                if (PowerManagerService.DuplicatePlan(selected.Guid, $"{selected.Name} {copySuffix}"))
                {
                    RefreshData(true);
                }
            }
            else if (_plans.Count > 0)
            {
                PowerManagerService.DuplicatePlan(_plans[0].Guid, defaultName);
                RefreshData(true);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (LstPlans.SelectedItem is PowerPlanItem selected)
            {
                if (selected.IsActive)
                {
                    string msg = ConfigService.Get("ActiveCannotDelete", "Aktif güç düzeni silinemez!");
                    MessageBox.Show(msg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                string confirmMsg = ConfigService.Get("ConfirmDelete", "Seçilen güç düzenini silmek istediğinizden emin misiniz?");
                var res = MessageBox.Show($"{confirmMsg}\n\n'{selected.Name}'", "Power Grid HaYTooL", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    if (!string.IsNullOrEmpty(ConfigService.UserStartupPlanGuid) &&
                        string.Equals(selected.Guid, ConfigService.UserStartupPlanGuid, StringComparison.OrdinalIgnoreCase))
                    {
                        RegistryService.RemoveUserStartupPlan();
                        ConfigService.UserStartupPlanGuid = "";
                        ConfigService.UserStartupPlanName = "";
                        ConfigService.Save();
                    }

                    PowerManagerService.DeletePlan(selected.Guid);
                    RefreshData(true);
                }
            }
        }

        private void BtnEditSettings_Click(object sender, RoutedEventArgs e)
        {
            var target = LstPlans.SelectedItem as PowerPlanItem ?? _plans.FirstOrDefault(p => p.IsActive);
            if (target != null)
            {
                var editor = new PlanEditorWindow(target.Guid, target.Name) { Owner = this };
                if (editor.ShowDialog() == true)
                {
                    RefreshData(true);
                }
            }
            else
            {
                string msg = ConfigService.CurrentLanguage == "English" ? "Please select a power plan to edit." : "Lütfen düzenlemek için bir güç planı seçin.";
                MessageBox.Show(msg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnSmartAutoSwitch_Click(object sender, RoutedEventArgs e)
        {
            var win = new UI.SmartAutoSwitchWindow { Owner = this };
            win.ShowDialog();
            MenuShowSystemTray.IsChecked = ConfigService.ShowSystemTray;
            MenuHideMinimized.IsChecked = ConfigService.HideWhenMinimized;
            MenuCloseToTray.IsChecked = ConfigService.CloseToTray;
            MyNotifyIcon.Visibility = ConfigService.ShowSystemTray ? Visibility.Visible : Visibility.Collapsed;
            RefreshData(false);
        }

        private void TrayContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            UpdateTraySmartSwitchMenu();
        }

        private void UpdateTraySmartSwitchMenu()
        {
            try
            {
                if (TraySmartSwitchRoot == null || TraySmartSwitchToggle == null) return;

                var settings = SmartSwitchService.GetSettings();
                var status = SmartSwitchService.GetCurrentStatus();
                bool isEng = ConfigService.CurrentLanguage == "English";

                TraySmartSwitchRoot.Header = isEng ? "⚡ Smart GPU Auto Power Switch" : "⚡ Akıllı GPU Otomatik Güç Anahtarı";
                TraySmartSwitchToggle.IsChecked = settings.IsEnabled;
                TraySmartSwitchToggle.Header = settings.IsEnabled
                    ? (isEng ? "✓ Enabled (Click to Turn Off)" : "✓ Açık (Kapatmak İçin Tıkla)")
                    : (isEng ? "○ Disabled (Click to Turn On)" : "○ Kapalı (Açmak İçin Tıkla)");

                TraySmartSwitchSettings.Header = isEng ? "⚙️ Smart GPU Settings..." : "⚙️ Akıllı GPU Ayarları...";
                TraySmartSwitchTiers.Header = isEng ? "📋 Tier Profiles" : "📋 Kademe Güç Profilleri";

                if (settings.IsEnabled)
                {
                    TraySmartSwitchStatus.Header = isEng
                        ? $"📊 Status: Active (GPU: {status.GpuUsagePercent:F0}% - Tier {status.CurrentTier})"
                        : $"📊 Durum: Aktif (GPU: %{status.GpuUsagePercent:F0} - {status.CurrentTier}. Kademe)";
                }
                else
                {
                    TraySmartSwitchStatus.Header = isEng ? "📊 Status: Disabled (Off)" : "📊 Durum: Devre Dışı (Kapalı)";
                }

                // Tier Plan Names
                var plans = PowerManagerService.GetPowerPlans();
                var p1 = plans.FirstOrDefault(p => p.Guid.Equals(settings.LowPlanGuid, StringComparison.OrdinalIgnoreCase));
                var p2 = plans.FirstOrDefault(p => p.Guid.Equals(settings.MediumPlanGuid, StringComparison.OrdinalIgnoreCase));
                var p3 = plans.FirstOrDefault(p => p.Guid.Equals(settings.HighPlanGuid, StringComparison.OrdinalIgnoreCase));

                string name1 = p1?.Name ?? (isEng ? "Auto Low/Eco" : "Otomatik Düşük/Eco");
                string name2 = p2?.Name ?? (isEng ? "Auto Balanced" : "Otomatik Dengeli");
                string name3 = p3?.Name ?? (isEng ? "Auto High/Game" : "Otomatik Yüksek/Oyun");

                TrayTier1Info.Header = $"🌱 1. " + (isEng ? $"Idle (<{settings.LowThresholdPercent}%): " : $"Boşta (<%{settings.LowThresholdPercent}): ") + name1;
                TrayTier2Info.Header = $"⚖️ 2. " + (isEng ? $"Med ({settings.LowThresholdPercent}-{settings.HighThresholdPercent}%): " : $"Orta (%{settings.LowThresholdPercent}-%{settings.HighThresholdPercent}): ") + name2;
                TrayTier3Info.Header = $"🚀 3. " + (isEng ? $"Game (≥{settings.HighThresholdPercent}%): " : $"Oyun (≥%{settings.HighThresholdPercent}): ") + name3;
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

        private void BtnComparePlans_Click(object sender, RoutedEventArgs e)
        {
            var target = LstPlans.SelectedItem as PowerPlanItem ?? _plans.FirstOrDefault(p => p.IsActive);
            var comparator = new ComparePlansWindow(target?.Guid) { Owner = this };
            comparator.ShowDialog();
            RefreshData(false);
        }

        private void ListContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            var plan = LstPlans.SelectedItem as PowerPlanItem;
            bool isEng = ConfigService.CurrentLanguage == "English";

            if (plan == null)
            {
                CtxToggleStartupPlan.Header = isEng
                    ? "📌 Set as User Startup Plan"
                    : "📌 Oturum Açılışında Bu Güç Planına Geç";
                return;
            }

            bool isStartup = string.Equals(ConfigService.UserStartupPlanGuid, plan.Guid, StringComparison.OrdinalIgnoreCase);
            if (isStartup)
            {
                CtxToggleStartupPlan.Header = isEng
                    ? "✓ User Startup Plan (Click to Remove)"
                    : "✓ Oturum Açılış Planı (Kaldırmak İçin Tıkla)";
            }
            else
            {
                CtxToggleStartupPlan.Header = isEng
                    ? "📌 Set as User Startup Plan"
                    : "📌 Oturum Açılışında Bu Güç Planına Geç";
            }
        }

        private void BtnToggleStartupPlan_Click(object sender, RoutedEventArgs e)
        {
            var plan = LstPlans.SelectedItem as PowerPlanItem ?? _plans.FirstOrDefault(p => p.IsActive);
            bool isEng = ConfigService.CurrentLanguage == "English";

            if (plan == null)
            {
                MessageBox.Show(
                    isEng ? "Please select a power plan first." : "Lütfen önce bir güç planı seçin.",
                    "Power Grid HaYTooL",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            string exePath = Environment.ProcessPath ?? System.IO.Path.Combine(AppContext.BaseDirectory, "PowerGridHaYTooL.exe");
            bool isCurrentlyStartup = string.Equals(ConfigService.UserStartupPlanGuid, plan.Guid, StringComparison.OrdinalIgnoreCase);

            if (isCurrentlyStartup)
            {
                var result = MessageBox.Show(
                    isEng
                        ? $"Remove '{plan.Name}' from your user startup power plan?\n(Windows will no longer automatically activate this plan when this user logs in.)"
                        : $"'{plan.Name}' oturum açılış planından kaldırılsın mı?\n(Bu kullanıcı oturum açtığında otomatik olarak bu plana geçilmeyecektir.)",
                    isEng ? "Remove Startup Plan" : "Başlangıç Planını Kaldır",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    RegistryService.RemoveUserStartupPlan();
                    ConfigService.UserStartupPlanGuid = "";
                    ConfigService.UserStartupPlanName = "";
                    ConfigService.Save();
                    RefreshData();
                }
            }
            else
            {
                RegistryService.SetUserStartupPlan(plan.Guid, exePath);
                ConfigService.UserStartupPlanGuid = plan.Guid;
                ConfigService.UserStartupPlanName = plan.Name;
                ConfigService.Save();
                RefreshData();

                MessageBox.Show(
                    isEng
                        ? $"'{plan.Name}' has been set as your user startup power plan!\nWhenever this user logs into Windows, this plan will automatically be activated."
                        : $"'{plan.Name}' bu kullanıcı için oturum açılış güç planı olarak ayarlandı!\nBu kullanıcı Windows oturumunu her açtığında otomatik olarak bu güç planına geçilecektir.",
                    isEng ? "Success" : "Başarılı",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void BtnUserStartupInfo_Click(object sender, RoutedEventArgs e)
        {
            var win = new UserStartupManagerWindow { Owner = this };
            if (win.ShowDialog() == true)
            {
                RefreshData();
            }
        }

        private void UpdateUnlockHiddenMenuState()
        {
            try
            {
                bool isUnlocked = PowerManagerService.AreHiddenSettingsUnlocked();
                bool isEnglish = ConfigService.CurrentLanguage == "English";
                MnuUnlockHidden.Header = isUnlocked
                    ? (isEnglish ? "🔒 Lock Windows Hidden Power Settings" : "🔒 Windows Gizli Güç Ayarlarını Kapat (Gizle)")
                    : (isEnglish ? "🔓 Unlock Windows Hidden Power Settings" : "🔓 Windows Gizli Güç Ayarlarını Aç (Tümünü Göster)");
            }
            catch { }
        }

        private void BtnUnlockHidden_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool currentlyUnlocked = PowerManagerService.AreHiddenSettingsUnlocked();
                bool targetState = !currentlyUnlocked;
                bool isEnglish = ConfigService.CurrentLanguage == "English";

                PowerManagerService.UnlockAllHiddenSettings(targetState);
                UpdateUnlockHiddenMenuState();

                if (targetState)
                {
                    string title = isEnglish ? "Settings Unlocked" : "Kilitler Açıldı";
                    string msg = isEnglish
                        ? "All advanced power settings (Minimum/Maximum Processor State, System Cooling, Turbo Boost, Core Parking, PCIe ASPM, etc.) have been unlocked in Windows Control Panel!\n\nNote: If Windows Control Panel was already open, please close and reopen it to see the updated options."
                        : "Windows Denetim Masasındaki tüm gelişmiş güç ayarlarının (En Düşük/Yüksek İşlemci Durumu, Sistem Soğutma, Turbo Boost, Çekirdek Park Etme, PCIe ASPM vb.) kilidi başarıyla açıldı!\n\nNot: Eğer Denetim Masası penceresi zaten açıksa, ayarların yenilenmesi için pencereyi kapatıp 'Windows Gelişmiş Ayarlar...' menüsünden tekrar açın.";
                    MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string title = isEnglish ? "Settings Locked" : "Ayarlar Gizlendi";
                    string msg = isEnglish
                        ? "All advanced hidden power settings have been hidden and locked in Windows Control Panel."
                        : "Windows Denetim Masasındaki gelişmiş güç ayarları başarıyla gizlendi ve kilitlendi.";
                    MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRename_Click(object sender, RoutedEventArgs e)
        {
            if (LstPlans.SelectedItem is PowerPlanItem selected)
            {
                string prompt = ConfigService.CurrentLanguage == "English" ? "Enter new power plan name:" : "Yeni güç düzeni ismini giriniz:";
                string title = ConfigService.Get("Rename", "Yeniden Adlandır");
                string input = Microsoft.VisualBasic.Interaction.InputBox(prompt, title, selected.Name);
                if (!string.IsNullOrWhiteSpace(input) && input != selected.Name)
                {
                    PowerManagerService.RenamePlan(selected.Guid, input);
                    RefreshData(true);
                }
            }
        }

        private void BtnCreateShortcut_Click(object sender, RoutedEventArgs e)
        {
            if (LstPlans.SelectedItem is PowerPlanItem selected)
            {
                bool isEng = ConfigService.CurrentLanguage == "English";
                if (ProcessHelper.CreateDesktopShortcut(selected.Name, selected.Guid, isEng))
                {
                    string msg = ConfigService.Get("ShortcutCreated", "için masaüstüne tek tık kısayolu başarıyla oluşturuldu!");
                    string fileName = $"{selected.Name} - Power Grid HaYTooL.cmd";
                    MessageBox.Show($"'{selected.Name}' {msg}\n\n📄 {fileName}", ConfigService.Get("CreateShortcut", "Kısayol Oluştur"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnCopyGuid_Click(object sender, RoutedEventArgs e)
        {
            if (LstPlans.SelectedItem is PowerPlanItem selected)
            {
                Clipboard.SetText(selected.Guid);
            }
        }

        private void BtnCopyName_Click(object sender, RoutedEventArgs e)
        {
            if (LstPlans.SelectedItem is PowerPlanItem selected)
            {
                Clipboard.SetText(selected.Name);
            }
        }

        private void BtnOpenPowerCpl_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.OpenUri("control.exe", "powercfg.cpl");
        }

        private void BtnMobilityCenter_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.OpenUri("mblctr.exe");
        }

        private void BtnPowerSleepSettings_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.OpenUri("ms-settings:powersleep");
        }

        private void BtnContextMenuToggle_Click(object sender, RoutedEventArgs e)
        {
            bool exists = RegistryService.IsDesktopContextMenuAdded();
            if (exists)
            {
                RegistryService.RemoveDesktopContextMenu();
                MenuDesktopContext.IsChecked = false;
                string msg = ConfigService.Get("MenuRemoved", "Masaüstü sağ tuş menüsünden kaldırıldı.");
                MessageBox.Show(msg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                RegistryService.AddDesktopContextMenu(_plans);
                MenuDesktopContext.IsChecked = true;
                string msg = ConfigService.Get("MenuAdded", "Masaüstü sağ tuş menüsüne eklendi!");
                MessageBox.Show(msg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuShowSystemTray_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.ShowSystemTray = MenuShowSystemTray.IsChecked;
            ConfigService.Save();
            MyNotifyIcon.Visibility = ConfigService.ShowSystemTray ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MenuAlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            Topmost = MenuAlwaysOnTop.IsChecked;
            ConfigService.AlwaysOnTop = MenuAlwaysOnTop.IsChecked;
            ConfigService.Save();
        }

        private void MenuHideMinimized_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.HideWhenMinimized = MenuHideMinimized.IsChecked;
            ConfigService.Save();
        }

        private void MenuCloseToTray_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.CloseToTray = MenuCloseToTray.IsChecked;
            ConfigService.Save();
        }

        private void MenuAutoStart_Click(object sender, RoutedEventArgs e)
        {
            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
            RegistryService.SetAutoStart(MenuAutoStart.IsChecked, exePath);
            ConfigService.AutoStart = MenuAutoStart.IsChecked;
            ConfigService.Save();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (LstPlans.SelectedItem is PowerPlanItem selected)
            {
                string filter = ConfigService.CurrentLanguage == "English" ? "Power Plan Files (*.pow)|*.pow" : "Güç Düzeni Dosyaları (*.pow)|*.pow";
                var sfd = new SaveFileDialog
                {
                    Filter = filter,
                    FileName = $"{selected.Name}.pow"
                };
                if (sfd.ShowDialog() == true)
                {
                    PowerManagerService.ExportPlan(selected.Guid, sfd.FileName);
                    string msg = ConfigService.Get("SuccessExport", "Güç planı başarıyla dışarıya aktarıldı.");
                    MessageBox.Show(msg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            string filter = ConfigService.CurrentLanguage == "English" ? "Power Plan Files (*.pow)|*.pow" : "Güç Düzeni Dosyaları (*.pow)|*.pow";
            var ofd = new OpenFileDialog
            {
                Filter = filter
            };
            if (ofd.ShowDialog() == true)
            {
                PowerManagerService.ImportPlan(ofd.FileName);
                RefreshData(true);
                string msg = ConfigService.Get("SuccessImport", "Güç planı başarıyla içe aktarıldı.");
                MessageBox.Show(msg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnBackupAll_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = ConfigService.CurrentLanguage == "English" ? "Select Folder for Backup" : "Yedekleme İçin Klasör Seçin"
            };
            if (dialog.ShowDialog() == true)
            {
                string folder = dialog.FolderName;
                var (success, fail) = PowerManagerService.BackupAllPlans(folder, _plans);
                string suffix = ConfigService.Get("SuccessBackupAll", "adet güç düzeni seçilen klasöre başarıyla yedeklendi!");
                string msg = $"{success} {suffix}";
                MessageBox.Show($"{msg}\n\n📁 {folder}", "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnRestoreAll_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = ConfigService.CurrentLanguage == "English" ? "Select Folder Containing .pow Files" : ".pow Dosyalarını İçeren Klasörü Seçin"
            };
            if (dialog.ShowDialog() == true)
            {
                string folder = dialog.FolderName;
                var (success, fail) = PowerManagerService.RestoreAllPlansFromFolder(folder);
                RefreshData(true);
                string suffix = ConfigService.Get("SuccessRestoreAll", "adet güç düzeni klasörden sisteme başarıyla geri yüklendi!");
                string msg = $"{success} {suffix}";
                MessageBox.Show($"{msg}\n\n📁 {folder}", "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnAddAllCustom_Click(object sender, RoutedEventArgs e)
        {
            string promptText = ConfigService.CurrentLanguage == "English"
                ? "🚀 The following 5 custom power plan templates will be added to your system:\n\n" +
                  "  • ⚡ HaYTooL Apex Performance (100% CPU, Active Cooling, Aggressive Boost, 0ms Latency)\n" +
                  "  • ⚖️ HaYTooL Smart Balanced (5-100% Adaptive CPU, Active Cooling, Efficient Boost)\n" +
                  "  • 🌱 HaYTooL Eco Stealth (5-75% Silent CPU, Passive Cooling, Boost Disabled)\n" +
                  "  • 🔴 AMD Ryzen™ Balanced (Optimized 90% Base Throttle)\n" +
                  "  • 🚀 AMD Ryzen™ High Performance (100% Full Clock)\n\n" +
                  "Do you want to add all these templates in a single click?"
                : "🚀 Aşağıdaki 5 özel şablon profilinin tümü tek tıkla sisteminize eklenecek:\n\n" +
                  "  • ⚡ HaYTooL Apex Performance (%100 CPU, Aktif Soğutma, Agresif Boost, Sıfır Gecikme)\n" +
                  "  • ⚖️ HaYTooL Smart Balanced (%5-100% Dinamik CPU, Aktif Soğutma, Verimli Boost)\n" +
                  "  • 🌱 HaYTooL Eco Stealth (%5-75% Soğuk CPU, Pasif Sessiz Fan, Boost Kapalı)\n" +
                  "  • 🔴 AMD Ryzen™ Dengeli (%90 Taban İşlemci Durumu)\n" +
                  "  • 🚀 AMD Ryzen™ Yüksek Performans (%100 Tam Hız)\n\n" +
                  "Tüm bu profilleri tek seferde eklemek istiyor musunuz?";

            var res = MessageBox.Show(promptText, "Power Grid HaYTooL", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;

            int addedCount = 0;

            // 1. HaYTooL Apex Performance
            if (!_plans.Any(p => string.Equals(p.Guid, PowerManagerService.GUID_HAYTOOL_APEX_PERF, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "HaYTooL Apex Performance", StringComparison.OrdinalIgnoreCase)))
            {
                if (PowerManagerService.AddHaYTooLApexPerformance("HaYTooL Apex Performance", out _)) addedCount++;
            }

            // 2. HaYTooL Smart Balanced
            if (!_plans.Any(p => string.Equals(p.Guid, PowerManagerService.GUID_HAYTOOL_SMART_BAL, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "HaYTooL Smart Balanced", StringComparison.OrdinalIgnoreCase)))
            {
                if (PowerManagerService.AddHaYTooLSmartBalanced("HaYTooL Smart Balanced", out _)) addedCount++;
            }

            // 3. HaYTooL Eco Stealth
            if (!_plans.Any(p => string.Equals(p.Guid, PowerManagerService.GUID_HAYTOOL_ECO_STEALTH, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "HaYTooL Eco Stealth", StringComparison.OrdinalIgnoreCase)))
            {
                if (PowerManagerService.AddHaYTooLEcoStealth("HaYTooL Eco Stealth", out _)) addedCount++;
            }

            // 4. AMD Ryzen Balanced
            string amdBalName = ConfigService.CurrentLanguage == "English" ? "AMD Ryzen™ Balanced" : "AMD Ryzen™ Dengeli";
            if (!_plans.Any(p => string.Equals(p.Guid, PowerManagerService.GUID_AMD_BALANCED, StringComparison.OrdinalIgnoreCase) ||
                                (p.Name.Contains("AMD Ryzen", StringComparison.OrdinalIgnoreCase) && p.Name.Contains("Balanced", StringComparison.OrdinalIgnoreCase)) ||
                                (p.Name.Contains("AMD Ryzen", StringComparison.OrdinalIgnoreCase) && p.Name.Contains("Dengeli", StringComparison.OrdinalIgnoreCase))))
            {
                if (PowerManagerService.AddAmdRyzenBalanced(amdBalName)) addedCount++;
            }

            // 5. AMD Ryzen High Performance
            string amdHighName = ConfigService.CurrentLanguage == "English" ? "AMD Ryzen™ High Performance" : "AMD Ryzen™ Yüksek Performans";
            if (!_plans.Any(p => string.Equals(p.Guid, PowerManagerService.GUID_AMD_HIGH_PERF, StringComparison.OrdinalIgnoreCase) ||
                                (p.Name.Contains("AMD Ryzen", StringComparison.OrdinalIgnoreCase) && p.Name.Contains("High", StringComparison.OrdinalIgnoreCase)) ||
                                (p.Name.Contains("AMD Ryzen", StringComparison.OrdinalIgnoreCase) && p.Name.Contains("Yüksek", StringComparison.OrdinalIgnoreCase))))
            {
                if (PowerManagerService.AddAmdRyzenHighPerformance(amdHighName)) addedCount++;
            }

            RefreshData(true);

            string successMsg = ConfigService.CurrentLanguage == "English"
                ? $"🚀 Completed! {addedCount} new custom template(s) added successfully."
                : $"🚀 İşlem tamamlandı! {addedCount} adet yeni özel şablon profili başarıyla eklendi.";
            MessageBox.Show(successMsg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnAddHaYTooLAll_Click(object sender, RoutedEventArgs e)
        {
            string promptText = ConfigService.CurrentLanguage == "English"
                ? "⚡ The following 3 custom HaYTooL power profiles will be added to your system:\n\n" +
                  "  • ⚡ HaYTooL Apex Performance (%100 CPU, Active Cooling, Aggressive Boost, 0ms Latency)\n" +
                  "  • ⚖️ HaYTooL Smart Balanced (%5-100% Dynamic CPU, Active Cooling, Efficient Boost)\n" +
                  "  • 🌱 HaYTooL Eco Stealth (%5-75% Cold CPU, Passive Whisper Cooling, Boost Disabled)\n\n" +
                  "Do you want to add these 3 custom profiles in a single click?"
                : "⚡ Aşağıdaki 3 özel HaYTooL güç profili tek tıkla sisteminize eklenecek:\n\n" +
                  "  • ⚡ HaYTooL Apex Performance (%100 CPU, Aktif Soğutma, Agresif Boost, Sıfır Gecikme)\n" +
                  "  • ⚖️ HaYTooL Smart Balanced (%5-100% Dinamik CPU, Aktif Soğutma, Verimli Boost)\n" +
                  "  • 🌱 HaYTooL Eco Stealth (%5-75% Soğuk CPU, Pasif Sessiz Fan, Boost Kapalı)\n\n" +
                  "Bu 3 özel profili tek seferde eklemek istiyor musunuz?";

            var res = MessageBox.Show(promptText, "Power Grid HaYTooL", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;

            int addedCount = 0;

            if (!_plans.Any(p => string.Equals(p.Guid, PowerManagerService.GUID_HAYTOOL_APEX_PERF, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "HaYTooL Apex Performance", StringComparison.OrdinalIgnoreCase)))
            {
                if (PowerManagerService.AddHaYTooLApexPerformance("HaYTooL Apex Performance", out _)) addedCount++;
            }

            if (!_plans.Any(p => string.Equals(p.Guid, PowerManagerService.GUID_HAYTOOL_SMART_BAL, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "HaYTooL Smart Balanced", StringComparison.OrdinalIgnoreCase)))
            {
                if (PowerManagerService.AddHaYTooLSmartBalanced("HaYTooL Smart Balanced", out _)) addedCount++;
            }

            if (!_plans.Any(p => string.Equals(p.Guid, PowerManagerService.GUID_HAYTOOL_ECO_STEALTH, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "HaYTooL Eco Stealth", StringComparison.OrdinalIgnoreCase)))
            {
                if (PowerManagerService.AddHaYTooLEcoStealth("HaYTooL Eco Stealth", out _)) addedCount++;
            }

            RefreshData(true);

            string successMsg = ConfigService.CurrentLanguage == "English"
                ? $"⚡ Completed! {addedCount} custom HaYTooL profile(s) added successfully."
                : $"⚡ İşlem tamamlandı! {addedCount} adet özel HaYTooL güç profili başarıyla eklendi.";
            MessageBox.Show(successMsg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnAddAmdAll_Click(object sender, RoutedEventArgs e)
        {
            string promptText = ConfigService.CurrentLanguage == "English"
                ? "🔴 The following 2 AMD Ryzen™ power profiles will be added to your system:\n\n" +
                  "  • 🔴 AMD Ryzen™ Balanced (90% Base Throttle)\n" +
                  "  • 🚀 AMD Ryzen™ High Performance (100% Full Clock)\n\n" +
                  "Do you want to add both AMD Ryzen profiles in a single click?"
                : "🔴 Aşağıdaki 2 AMD Ryzen™ güç profili tek tıkla sisteminize eklenecek:\n\n" +
                  "  • 🔴 AMD Ryzen™ Dengeli (%90 Taban İşlemci Durumu)\n" +
                  "  • 🚀 AMD Ryzen™ Yüksek Performans (%100 Tam Hız)\n\n" +
                  "Her iki AMD Ryzen profilini tek seferde eklemek istiyor musunuz?";

            var res = MessageBox.Show(promptText, "Power Grid HaYTooL", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;

            int addedCount = 0;
            string amdBalName = ConfigService.CurrentLanguage == "English" ? "AMD Ryzen™ Balanced" : "AMD Ryzen™ Dengeli";
            if (!_plans.Any(p => string.Equals(p.Guid, PowerManagerService.GUID_AMD_BALANCED, StringComparison.OrdinalIgnoreCase) ||
                                (p.Name.Contains("AMD Ryzen", StringComparison.OrdinalIgnoreCase) && p.Name.Contains("Balanced", StringComparison.OrdinalIgnoreCase)) ||
                                (p.Name.Contains("AMD Ryzen", StringComparison.OrdinalIgnoreCase) && p.Name.Contains("Dengeli", StringComparison.OrdinalIgnoreCase))))
            {
                if (PowerManagerService.AddAmdRyzenBalanced(amdBalName)) addedCount++;
            }

            string amdHighName = ConfigService.CurrentLanguage == "English" ? "AMD Ryzen™ High Performance" : "AMD Ryzen™ Yüksek Performans";
            if (!_plans.Any(p => string.Equals(p.Guid, PowerManagerService.GUID_AMD_HIGH_PERF, StringComparison.OrdinalIgnoreCase) ||
                                (p.Name.Contains("AMD Ryzen", StringComparison.OrdinalIgnoreCase) && p.Name.Contains("High", StringComparison.OrdinalIgnoreCase)) ||
                                (p.Name.Contains("AMD Ryzen", StringComparison.OrdinalIgnoreCase) && p.Name.Contains("Yüksek", StringComparison.OrdinalIgnoreCase))))
            {
                if (PowerManagerService.AddAmdRyzenHighPerformance(amdHighName)) addedCount++;
            }

            RefreshData(true);

            string successMsg = ConfigService.CurrentLanguage == "English"
                ? $"🔴 Completed! {addedCount} AMD Ryzen™ profile(s) added successfully."
                : $"🔴 İşlem tamamlandı! {addedCount} adet AMD Ryzen™ güç profili başarıyla eklendi.";
            MessageBox.Show(successMsg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private delegate bool AddCustomPlanDelegate(string name, out string createdGuid);

        private void PromptAndAddCustomPlan(string planName, string planGuid, string details, AddCustomPlanDelegate addFunc)
        {
            bool alreadyExists = _plans.Any(p =>
                string.Equals(p.Guid, planGuid, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name.Trim(), planName, StringComparison.OrdinalIgnoreCase));

            if (alreadyExists)
            {
                string existsMsg = ConfigService.CurrentLanguage == "English" 
                    ? $"The power plan '{planName}' already exists in Windows!" 
                    : $"'{planName}' güç düzeni Windows sisteminizde zaten mevcut!";
                MessageBox.Show(existsMsg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string question = ConfigService.CurrentLanguage == "English"
                ? $"{details}\n\nDo you want to create and configure '{planName}' with custom optimized settings on your system?"
                : $"{details}\n\n'{planName}' özel güç profilini optimize edilmiş ayarlarıyla sisteminize eklemek istiyor musunuz?";

            var promptRes = MessageBox.Show(question, "Power Grid HaYTooL - Şablon Ekle", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (promptRes != MessageBoxResult.Yes) return;

            if (addFunc(planName, out string newGuid))
            {
                RefreshData(true);
                string addedMsg = ConfigService.CurrentLanguage == "English"
                    ? $"⚡ '{planName}' has been successfully added with custom optimized settings!\n\nDo you want to switch to this new power plan right now?"
                    : $"⚡ '{planName}' profili optimize edilmiş özel ayarlarıyla başarıyla eklendi!\n\nŞimdi bu yeni plana geçmek (hemen aktif yapmak) istiyor musunuz?";

                var activateRes = MessageBox.Show(addedMsg, "Power Grid HaYTooL", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (activateRes == MessageBoxResult.Yes)
                {
                    PowerManagerService.SetActivePlan(newGuid);
                    RefreshData(true);
                }
            }
            else
            {
                string errMsg = ConfigService.CurrentLanguage == "English" 
                    ? $"Failed to create and configure '{planName}'." 
                    : $"'{planName}' güç profili eklenirken bir hata oluştu.";
                MessageBox.Show(errMsg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnAddHaYTooLApex_Click(object sender, RoutedEventArgs e)
        {
            string details = ConfigService.CurrentLanguage == "English"
                ? "⚡ [HaYTooL Apex Performance]\n- Min/Max CPU: 100% (Instant Zero-Lag Frequency Response)\n- Active Fan Cooling (Spins Fans Before Thermal Throttling)\n- Aggressive Turbo Boost & All CPU Cores Awake/Unparked (100%)\n- EPP 0, PCIe ASPM Disabled (0), Zero-Latency USB & Wi-Fi"
                : "⚡ [HaYTooL Apex Performance]\n- Min/Max İşlemci: %100 (Anında Sıfır Gecikmeli Frekans Yanıtı)\n- Aktif Fan Soğutma (Isınma Öncesi Erken Fan Devri)\n- Agresif Turbo Boost ve Tüm Çekirdekler Uyanık/Park Yok (%100)\n- EPP 0, PCIe ASPM Kapalı (0), Sıfır Gecikmeli USB ve Wi-Fi";

            PromptAndAddCustomPlan("HaYTooL Apex Performance", PowerManagerService.GUID_HAYTOOL_APEX_PERF, details, PowerManagerService.AddHaYTooLApexPerformance);
        }

        private void BtnAddHaYTooLBalanced_Click(object sender, RoutedEventArgs e)
        {
            string details = ConfigService.CurrentLanguage == "English"
                ? "⚖️ [HaYTooL Smart Balanced]\n- Min CPU 5%, Max CPU 100% (Dynamic Adaptive Clocking)\n- Active Cooling with Efficient Turbo Boost (Level 3)\n- Microsecond Dynamic Core Scaling (Min 25% Active, Max 100%)\n- EPP 35/50, PCIe ASPM Moderate, Balanced Wi-Fi"
                : "⚖️ [HaYTooL Smart Balanced]\n- Min İşlemci %5, Max İşlemci %100 (Dinamik Adaptif Hız)\n- Aktif Soğutma ve Verimli Turbo Boost (Seviye 3)\n- İhtiyaca Göre Mikro-Saniye Dinamik Çekirdek Yönetimi\n- EPP 35/50, PCIe ASPM Dengeli, Verimli Wi-Fi";

            PromptAndAddCustomPlan("HaYTooL Smart Balanced", PowerManagerService.GUID_HAYTOOL_SMART_BAL, details, PowerManagerService.AddHaYTooLSmartBalanced);
        }

        private void BtnAddHaYTooLEco_Click(object sender, RoutedEventArgs e)
        {
            string details = ConfigService.CurrentLanguage == "English"
                ? "🌱 [HaYTooL Eco Stealth]\n- Min CPU 5%, Max CPU 75% (Eliminates Excessive Heat)\n- Passive Cooling Policy (Silent Whisper Fan Operation)\n- Turbo Boost Disabled (0) for Extended Battery Life & Cold CPU\n- Deep Core Parking, Max ASPM & Wi-Fi Energy Savings"
                : "🌱 [HaYTooL Eco Stealth]\n- Min İşlemci %5, Max İşlemci %75 (Aşırı Isınmayı Keser)\n- Pasif Soğutma İlkesi (Tamamen Sessiz Fan Çalışması)\n- Turbo Boost Devre Dışı (Sıcaklık ve Pil Tasarrufu)\n- Derin Çekirdek Parkı, Maksimum ASPM ve Wi-Fi Tasarrufu";

            PromptAndAddCustomPlan("HaYTooL Eco Stealth", PowerManagerService.GUID_HAYTOOL_ECO_STEALTH, details, PowerManagerService.AddHaYTooLEcoStealth);
        }

        private void BtnAddUltimatePerf_Click(object sender, RoutedEventArgs e)
        {
            string name = ConfigService.CurrentLanguage == "English" ? "Ultimate Performance" : "Nihai Performans";

            bool alreadyExists = _plans.Any(p =>
                string.Equals(p.Guid, PowerManagerService.GUID_ULTIMATE_PERF, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name.Trim(), "Nihai Performans", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name.Trim(), "Ultimate Performance", StringComparison.OrdinalIgnoreCase));

            if (alreadyExists)
            {
                string existsMsg = ConfigService.Get("PlanAlreadyExists", "güç düzeni sistemde zaten mevcut!");
                MessageBox.Show($"'{name}' {existsMsg}", "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (PowerManagerService.AddUltimatePerformance(name))
            {
                RefreshData(true);
                string suffix = ConfigService.Get("TemplateAdded", "şablonu sisteme başarıyla eklendi!");
                MessageBox.Show($"⚡ '{name}' {suffix}", "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                string errMsg = ConfigService.CurrentLanguage == "English" ? "Failed to add Ultimate Performance scheme." : "Nihai Performans şablonu eklenemedi.";
                MessageBox.Show(errMsg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnAddAmdBalanced_Click(object sender, RoutedEventArgs e)
        {
            string name = ConfigService.CurrentLanguage == "English" ? "AMD Ryzen™ Balanced" : "AMD Ryzen™ Dengeli";

            bool alreadyExists = _plans.Any(p =>
                string.Equals(p.Guid, PowerManagerService.GUID_AMD_BALANCED, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name.Trim(), "AMD Ryzen™ Dengeli", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name.Trim(), "AMD Ryzen™ Balanced", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name.Trim(), "AMD Ryzen Dengeli", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name.Trim(), "AMD Ryzen Balanced", StringComparison.OrdinalIgnoreCase));

            if (alreadyExists)
            {
                string existsMsg = ConfigService.Get("PlanAlreadyExists", "güç düzeni sistemde zaten mevcut!");
                MessageBox.Show($"'{name}' {existsMsg}", "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (PowerManagerService.AddAmdRyzenBalanced(name))
            {
                RefreshData(true);
                string suffix = ConfigService.Get("TemplateAdded", "şablonu sisteme başarıyla eklendi!");
                MessageBox.Show($"🔴 '{name}' {suffix}", "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                string errMsg = ConfigService.CurrentLanguage == "English" ? "Failed to add AMD Ryzen Balanced scheme." : "AMD Ryzen Dengeli şablonu eklenemedi.";
                MessageBox.Show(errMsg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnAddAmdHighPerf_Click(object sender, RoutedEventArgs e)
        {
            string name = ConfigService.CurrentLanguage == "English" ? "AMD Ryzen™ High Performance" : "AMD Ryzen™ Yüksek Performans";

            bool alreadyExists = _plans.Any(p =>
                string.Equals(p.Guid, PowerManagerService.GUID_AMD_HIGH_PERF, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name.Trim(), "AMD Ryzen™ Yüksek Performans", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name.Trim(), "AMD Ryzen™ High Performance", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name.Trim(), "AMD Ryzen Yüksek Performans", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name.Trim(), "AMD Ryzen High Performance", StringComparison.OrdinalIgnoreCase));

            if (alreadyExists)
            {
                string existsMsg = ConfigService.Get("PlanAlreadyExists", "güç düzeni sistemde zaten mevcut!");
                MessageBox.Show($"'{name}' {existsMsg}", "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (PowerManagerService.AddAmdRyzenHighPerformance(name))
            {
                RefreshData(true);
                string suffix = ConfigService.Get("TemplateAdded", "şablonu sisteme başarıyla eklendi!");
                MessageBox.Show($"🚀 '{name}' {suffix}", "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                string errMsg = ConfigService.CurrentLanguage == "English" ? "Failed to add AMD Ryzen High Performance scheme." : "AMD Ryzen Yüksek Performans şablonu eklenemedi.";
                MessageBox.Show(errMsg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            string confirmMsg = ConfigService.CurrentLanguage == "English"
                ? "Are you sure you want to restore the standard Windows power schemes (Balanced, High Performance, Power Saver) to their factory default state?\n\nThis will repair any corrupted or missing Windows built-in schemes."
                : "Orijinal Windows güç düzenlerini (Dengeli, Yüksek Performans, Güç Tasarrufu) fabrika ayarlarına döndürmek ve varsayılan durumuna sıfırlamak istediğinizden emin misiniz?\n\nBu işlem bozulmuş veya eksik Windows güç profillerini onarır.";

            var res = MessageBox.Show(confirmMsg, "Power Grid HaYTooL - Varsayılana Döndür", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                if (PowerManagerService.RestoreDefaults())
                {
                    RefreshData(true);
                    string successMsg = ConfigService.CurrentLanguage == "English"
                        ? "Windows power schemes have been successfully restored to factory defaults!"
                        : "Windows güç düzenleri başarıyla varsayılan fabrika ayarlarına döndürüldü!";
                    MessageBox.Show(successMsg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string failMsg = ConfigService.CurrentLanguage == "English"
                        ? "Failed to restore Windows default power schemes."
                        : "Windows varsayılan güç düzenleri döndürülürken bir hata oluştu.";
                    MessageBox.Show(failMsg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnWebSite_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.OpenUri(AboutWindow.WebSiteUrl);
        }

        private void BtnPortal_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.OpenUri(AboutWindow.PortalUrl);
        }

        private void BtnGithub_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.OpenUri(AboutWindow.GithubUrl);
        }

        private void BtnFeedback_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string verStr = ver != null ? $"{ver.Major}.{ver.Minor}" : "8.1";
                string subject = Uri.EscapeDataString($"Power Grid HaYTooL v{verStr} Geri Bildirim");
                string body = Uri.EscapeDataString($"Merhaba,\n\nPower Grid HaYTooL v{verStr} uygulaması hakkındaki geri bildirimim:\n\n- Sistem: {Environment.OSVersion}\n\n");
                string mailto = $"mailto:{AboutWindow.ContactEmail}?subject={subject}&body={body}";
                ProcessHelper.OpenUri(mailto);
            }
            catch
            {
                var win = new AboutWindow { Owner = this };
                win.ShowDialog();
            }
        }

        private void BtnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdateCheckService.CheckForUpdatesAsync(isManual: true);
        }

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            var win = new AboutWindow { Owner = this };
            win.ShowDialog();
        }

        private void TxtVersion_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var win = new AboutWindow { Owner = this };
            win.ShowDialog();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && ConfigService.HideWhenMinimized && ConfigService.ShowSystemTray)
            {
                Hide();
            }
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

        private void BtnRestoreAutoZip_Click(object sender, RoutedEventArgs e)
        {
            var win = new RestoreBackupWindow { Owner = this };
            if (win.ShowDialog() == true)
            {
                RefreshData(true);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExplicitExit && ConfigService.ShowSystemTray && ConfigService.CloseToTray)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            _isExplicitExit = true;
            SmartSwitchService.PlanSwitched -= OnSmartPlanSwitched;
            SmartSwitchService.StatusUpdated -= OnSmartStatusUpdated;
            SmartSwitchService.SystemTrayStateRequested -= OnSystemTrayStateRequested;
            SmartSwitchService.Stop();
            MyNotifyIcon.Dispose();
            Application.Current.Shutdown();
        }
    }
}