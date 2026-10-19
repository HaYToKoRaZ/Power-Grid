using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PowerGridHaYTooL.Helpers;
using PowerGridHaYTooL.Models;
using PowerGridHaYTooL.Services;

namespace PowerGridHaYTooL.UI
{
    public partial class PlanEditorWindow : Window
    {
        private readonly string _guid;
        private readonly string _name;
        private PowerPlanSettings? _settings;
        private bool _isInitializing = true;

        // Timeout options in minutes: (Minutes, DisplayText)
        private readonly List<Tuple<int, string>> _timeoutOptions = new List<Tuple<int, string>>
        {
            Tuple.Create(0, "Hiçbir zaman"),
            Tuple.Create(1, "1 dakika"),
            Tuple.Create(2, "2 dakika"),
            Tuple.Create(3, "3 dakika"),
            Tuple.Create(5, "5 dakika"),
            Tuple.Create(10, "10 dakika"),
            Tuple.Create(15, "15 dakika"),
            Tuple.Create(20, "20 dakika"),
            Tuple.Create(25, "25 dakika"),
            Tuple.Create(30, "30 dakika"),
            Tuple.Create(45, "45 dakika"),
            Tuple.Create(60, "1 saat"),
            Tuple.Create(90, "1.5 saat"),
            Tuple.Create(120, "2 saat"),
            Tuple.Create(180, "3 saat"),
            Tuple.Create(240, "4 saat"),
            Tuple.Create(300, "5 saat")
        };

        public PlanEditorWindow(string guid, string name)
        {
            InitializeComponent();
            ThemeService.ApplyTheme(ConfigService.CurrentTheme, this);
            _guid = guid;
            _name = name;

            TxtPlanTitle.Text = string.IsNullOrEmpty(name) ? "Güç Planı Ayarları" : name;
            TxtPlanGuid.Text = $"GUID: {guid}";

            ApplyDeviceTypeLayout();
            PopulateTimeoutComboBoxes();
            LoadPlanSettings();
            UpdateUnlockButtonState();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var workArea = SystemParameters.WorkArea;
            // Ayarlar içeriğinin kaydırma çubuğu gerektirmeden tam ve ferah sığması için pencereyi büyüt
            double targetHeight = Math.Min(890, workArea.Height - 40);
            double targetWidth = Math.Min(1050, workArea.Width - 40);

            Height = targetHeight;
            Width = targetWidth;

            Top = workArea.Top + (workArea.Height - Height) / 2;
            Left = workArea.Left + (workArea.Width - Width) / 2;
        }

        private void ApplyDeviceTypeLayout()
        {
            bool isDesktop = SystemInfoHelper.IsDesktopPc();

            if (isDesktop)
            {
                TxtDeviceType.Text = "🖥️ Masaüstü PC (Yalnızca Priz Ayarları)";

                // DC kolon genişliklerini sıfırla (AC kolonu kartı tam doldurur)
                ColDcCpu.Width = new GridLength(0);
                ColDcBoost.Width = new GridLength(0);
                ColDcPcie.Width = new GridLength(0);
                ColDcTimeouts.Width = new GridLength(0);
                ColDcButtons.Width = new GridLength(0);

                // Başlıkları tekil hale getir
                LblAcHeaderCpu.Text = "⚡ Güç Ayarı (Priz)";
                LblAcHeaderTimeouts.Text = "⚡ Süre Ayarı (Priz)";
                LblDcHeaderCpu.Visibility = Visibility.Collapsed;
                LblDcHeaderTimeouts.Visibility = Visibility.Collapsed;

                // Pil kontrollerini gizle ve devre dışı bırak
                GridMinCpuDc.Visibility = Visibility.Collapsed;
                GridMinCpuDc.IsEnabled = false;

                GridMaxCpuDc.Visibility = Visibility.Collapsed;
                GridMaxCpuDc.IsEnabled = false;

                CmbCoolingDc.Visibility = Visibility.Collapsed;
                CmbCoolingDc.IsEnabled = false;

                CmbBoostDc.Visibility = Visibility.Collapsed;
                CmbBoostDc.IsEnabled = false;

                GridCoreParkMinDc.Visibility = Visibility.Collapsed;
                GridCoreParkMinDc.IsEnabled = false;

                GridCoreParkMaxDc.Visibility = Visibility.Collapsed;
                GridCoreParkMaxDc.IsEnabled = false;

                GridEppDc.Visibility = Visibility.Collapsed;
                GridEppDc.IsEnabled = false;

                CmbPcieDc.Visibility = Visibility.Collapsed;
                CmbPcieDc.IsEnabled = false;

                CmbUsbDc.Visibility = Visibility.Collapsed;
                CmbUsbDc.IsEnabled = false;

                CmbWifiDc.Visibility = Visibility.Collapsed;
                CmbWifiDc.IsEnabled = false;

                CmbVideoDc.Visibility = Visibility.Collapsed;
                CmbVideoDc.IsEnabled = false;

                CmbSleepDc.Visibility = Visibility.Collapsed;
                CmbSleepDc.IsEnabled = false;

                CmbHibernateDc.Visibility = Visibility.Collapsed;
                CmbHibernateDc.IsEnabled = false;

                CmbDiskDc.Visibility = Visibility.Collapsed;
                CmbDiskDc.IsEnabled = false;

                CmbWakeTimersDc.Visibility = Visibility.Collapsed;
                CmbWakeTimersDc.IsEnabled = false;

                CmbPowerBtnDc.Visibility = Visibility.Collapsed;
                CmbPowerBtnDc.IsEnabled = false;

                CmbSleepBtnDc.Visibility = Visibility.Collapsed;
                CmbSleepBtnDc.IsEnabled = false;
            }
            else
            {
                TxtDeviceType.Text = "💻 Taşınabilir PC (Priz & Pil Ayarları)";
            }
        }

        private void PopulateTimeoutComboBoxes()
        {
            PopulateCombo(CmbVideoAc);
            PopulateCombo(CmbVideoDc);
            PopulateCombo(CmbSleepAc);
            PopulateCombo(CmbSleepDc);
            PopulateCombo(CmbHibernateAc);
            PopulateCombo(CmbHibernateDc);
            PopulateCombo(CmbDiskAc);
            PopulateCombo(CmbDiskDc);
        }

        private void PopulateCombo(ComboBox cmb)
        {
            cmb.Items.Clear();
            foreach (var opt in _timeoutOptions)
            {
                cmb.Items.Add(new ComboBoxItem { Content = opt.Item2, Tag = opt.Item1 });
            }
        }

        private void SetComboValue(ComboBox cmb, int minutes)
        {
            foreach (ComboBoxItem item in cmb.Items)
            {
                if (item.Tag is int val && val == minutes)
                {
                    cmb.SelectedItem = item;
                    return;
                }
            }

            // If not found in standard list, add custom entry
            string label = minutes == 0 ? "Hiçbir zaman" : $"{minutes} dakika";
            var customItem = new ComboBoxItem { Content = label, Tag = minutes };
            cmb.Items.Add(customItem);
            cmb.SelectedItem = customItem;
        }

        private int GetComboValue(ComboBox cmb)
        {
            if (cmb.SelectedItem is ComboBoxItem item && item.Tag is int val)
            {
                return val;
            }
            return 0;
        }

        private void LoadPlanSettings()
        {
            _isInitializing = true;
            try
            {
                _settings = PowerManagerService.GetPlanSettings(_guid, _name);

                // 1. Ekran, Uyku & Disk
                SetComboValue(CmbVideoAc, _settings.VideoTimeoutAcMinutes);
                SetComboValue(CmbVideoDc, _settings.VideoTimeoutDcMinutes);
                SetComboValue(CmbSleepAc, _settings.SleepTimeoutAcMinutes);
                SetComboValue(CmbSleepDc, _settings.SleepTimeoutDcMinutes);
                SetComboValue(CmbHibernateAc, _settings.HibernateTimeoutAc);
                SetComboValue(CmbHibernateDc, _settings.HibernateTimeoutDc);
                SetComboValue(CmbDiskAc, _settings.DiskTimeoutAcMinutes);
                SetComboValue(CmbDiskDc, _settings.DiskTimeoutDcMinutes);

                SetIndexSafe(CmbWakeTimersAc, _settings.AllowWakeTimersAc);
                SetIndexSafe(CmbWakeTimersDc, _settings.AllowWakeTimersDc);

                // 2. CPU Temel
                SldMinCpuAc.Value = _settings.MinCpuAc;
                SldMinCpuDc.Value = _settings.MinCpuDc;
                SldMaxCpuAc.Value = _settings.MaxCpuAc;
                SldMaxCpuDc.Value = _settings.MaxCpuDc;

                CmbCoolingAc.SelectedIndex = _settings.CoolingPolicyAc == 1 ? 0 : 1;
                CmbCoolingDc.SelectedIndex = _settings.CoolingPolicyDc == 1 ? 0 : 1;

                // 2. CPU Gelişmiş (Turbo, Park, EPP)
                SetIndexSafe(CmbBoostAc, _settings.BoostModeAc);
                SetIndexSafe(CmbBoostDc, _settings.BoostModeDc);

                SldCoreParkMinAc.Value = _settings.CoreParkingMinAc;
                SldCoreParkMinDc.Value = _settings.CoreParkingMinDc;
                SldCoreParkMaxAc.Value = _settings.CoreParkingMaxAc;
                SldCoreParkMaxDc.Value = _settings.CoreParkingMaxDc;

                SldEppAc.Value = _settings.EnergyPerfPreferenceAc;
                SldEppDc.Value = _settings.EnergyPerfPreferenceDc;

                // 3. Donanım & Çevre Birimleri
                SetIndexSafe(CmbPcieAc, _settings.PcieAspmAc);
                SetIndexSafe(CmbPcieDc, _settings.PcieAspmDc);

                SetIndexSafe(CmbUsbAc, _settings.UsbSelectiveSuspendAc);
                SetIndexSafe(CmbUsbDc, _settings.UsbSelectiveSuspendDc);

                SetIndexSafe(CmbWifiAc, _settings.WifiPowerSavingAc);
                SetIndexSafe(CmbWifiDc, _settings.WifiPowerSavingDc);

                // 4. Güç Düğmeleri
                SetIndexSafe(CmbPowerBtnAc, _settings.PowerButtonActionAc);
                SetIndexSafe(CmbPowerBtnDc, _settings.PowerButtonActionDc);

                SetIndexSafe(CmbSleepBtnAc, _settings.SleepButtonActionAc);
                SetIndexSafe(CmbSleepBtnDc, _settings.SleepButtonActionDc);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ayarlar okunurken hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void SetIndexSafe(ComboBox cmb, int index)
        {
            if (index >= 0 && index < cmb.Items.Count)
                cmb.SelectedIndex = index;
            else if (cmb.Items.Count > 0)
                cmb.SelectedIndex = 0;
        }

        private void SldMinCpuAc_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isInitializing) return;
            if (SldMaxCpuAc != null && SldMinCpuAc.Value > SldMaxCpuAc.Value)
            {
                SldMaxCpuAc.Value = SldMinCpuAc.Value;
            }
        }

        private void SldMinCpuDc_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isInitializing) return;
            if (SldMaxCpuDc != null && SldMinCpuDc.Value > SldMaxCpuDc.Value)
            {
                SldMaxCpuDc.Value = SldMinCpuDc.Value;
            }
        }

        private void SldMaxCpuAc_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isInitializing) return;
            if (SldMinCpuAc != null && SldMaxCpuAc.Value < SldMinCpuAc.Value)
            {
                SldMinCpuAc.Value = SldMaxCpuAc.Value;
            }
        }

        private void SldMaxCpuDc_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isInitializing) return;
            if (SldMinCpuDc != null && SldMaxCpuDc.Value < SldMinCpuDc.Value)
            {
                SldMinCpuDc.Value = SldMaxCpuDc.Value;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_settings == null)
                    _settings = new PowerPlanSettings { PlanGuid = _guid, PlanName = _name };

                // 1. Ekran & Uyku
                _settings.VideoTimeoutAcMinutes = GetComboValue(CmbVideoAc);
                _settings.VideoTimeoutDcMinutes = GetComboValue(CmbVideoDc);
                _settings.SleepTimeoutAcMinutes = GetComboValue(CmbSleepAc);
                _settings.SleepTimeoutDcMinutes = GetComboValue(CmbSleepDc);
                _settings.HibernateTimeoutAc = GetComboValue(CmbHibernateAc);
                _settings.HibernateTimeoutDc = GetComboValue(CmbHibernateDc);
                _settings.DiskTimeoutAcMinutes = GetComboValue(CmbDiskAc);
                _settings.DiskTimeoutDcMinutes = GetComboValue(CmbDiskDc);

                _settings.AllowWakeTimersAc = CmbWakeTimersAc.SelectedIndex >= 0 ? CmbWakeTimersAc.SelectedIndex : 1;
                _settings.AllowWakeTimersDc = CmbWakeTimersDc.SelectedIndex >= 0 ? CmbWakeTimersDc.SelectedIndex : 1;

                // 2. CPU
                _settings.MinCpuAc = (int)Math.Round(SldMinCpuAc.Value);
                _settings.MinCpuDc = (int)Math.Round(SldMinCpuDc.Value);
                _settings.MaxCpuAc = (int)Math.Round(SldMaxCpuAc.Value);
                _settings.MaxCpuDc = (int)Math.Round(SldMaxCpuDc.Value);

                _settings.CoolingPolicyAc = CmbCoolingAc.SelectedIndex == 0 ? 1 : 0;
                _settings.CoolingPolicyDc = CmbCoolingDc.SelectedIndex == 0 ? 1 : 0;

                _settings.BoostModeAc = CmbBoostAc.SelectedIndex >= 0 ? CmbBoostAc.SelectedIndex : 2;
                _settings.BoostModeDc = CmbBoostDc.SelectedIndex >= 0 ? CmbBoostDc.SelectedIndex : 2;

                _settings.CoreParkingMinAc = (int)Math.Round(SldCoreParkMinAc.Value);
                _settings.CoreParkingMinDc = (int)Math.Round(SldCoreParkMinDc.Value);
                _settings.CoreParkingMaxAc = (int)Math.Round(SldCoreParkMaxAc.Value);
                _settings.CoreParkingMaxDc = (int)Math.Round(SldCoreParkMaxDc.Value);

                _settings.EnergyPerfPreferenceAc = (int)Math.Round(SldEppAc.Value);
                _settings.EnergyPerfPreferenceDc = (int)Math.Round(SldEppDc.Value);

                // 3. Donanım & Çevre Birimleri
                _settings.PcieAspmAc = CmbPcieAc.SelectedIndex >= 0 ? CmbPcieAc.SelectedIndex : 0;
                _settings.PcieAspmDc = CmbPcieDc.SelectedIndex >= 0 ? CmbPcieDc.SelectedIndex : 1;

                _settings.UsbSelectiveSuspendAc = CmbUsbAc.SelectedIndex >= 0 ? CmbUsbAc.SelectedIndex : 0;
                _settings.UsbSelectiveSuspendDc = CmbUsbDc.SelectedIndex >= 0 ? CmbUsbDc.SelectedIndex : 1;

                _settings.WifiPowerSavingAc = CmbWifiAc.SelectedIndex >= 0 ? CmbWifiAc.SelectedIndex : 0;
                _settings.WifiPowerSavingDc = CmbWifiDc.SelectedIndex >= 0 ? CmbWifiDc.SelectedIndex : 2;

                // 4. Güç Düğmeleri
                _settings.PowerButtonActionAc = CmbPowerBtnAc.SelectedIndex >= 0 ? CmbPowerBtnAc.SelectedIndex : 3;
                _settings.PowerButtonActionDc = CmbPowerBtnDc.SelectedIndex >= 0 ? CmbPowerBtnDc.SelectedIndex : 3;

                _settings.SleepButtonActionAc = CmbSleepBtnAc.SelectedIndex >= 0 ? CmbSleepBtnAc.SelectedIndex : 1;
                _settings.SleepButtonActionDc = CmbSleepBtnDc.SelectedIndex >= 0 ? CmbSleepBtnDc.SelectedIndex : 1;

                // Masaüstü PC ise pil (DC) ayarlarını priz (AC) ayarlarıyla senkronize tut
                if (SystemInfoHelper.IsDesktopPc())
                {
                    _settings.VideoTimeoutDcMinutes = _settings.VideoTimeoutAcMinutes;
                    _settings.SleepTimeoutDcMinutes = _settings.SleepTimeoutAcMinutes;
                    _settings.HibernateTimeoutDc = _settings.HibernateTimeoutAc;
                    _settings.DiskTimeoutDcMinutes = _settings.DiskTimeoutAcMinutes;
                    _settings.AllowWakeTimersDc = _settings.AllowWakeTimersAc;
                    _settings.MinCpuDc = _settings.MinCpuAc;
                    _settings.MaxCpuDc = _settings.MaxCpuAc;
                    _settings.CoolingPolicyDc = _settings.CoolingPolicyAc;
                    _settings.BoostModeDc = _settings.BoostModeAc;
                    _settings.CoreParkingMinDc = _settings.CoreParkingMinAc;
                    _settings.CoreParkingMaxDc = _settings.CoreParkingMaxAc;
                    _settings.EnergyPerfPreferenceDc = _settings.EnergyPerfPreferenceAc;
                    _settings.PcieAspmDc = _settings.PcieAspmAc;
                    _settings.UsbSelectiveSuspendDc = _settings.UsbSelectiveSuspendAc;
                    _settings.WifiPowerSavingDc = _settings.WifiPowerSavingAc;
                    _settings.PowerButtonActionDc = _settings.PowerButtonActionAc;
                    _settings.SleepButtonActionDc = _settings.SleepButtonActionAc;
                }

                PowerManagerService.SavePlanSettings(_settings);

                MessageBox.Show($"'{_name}' güç planı gelişmiş ayarları başarıyla uygulandı ve kaydedildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ayarlar kaydedilirken hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUnlockButtonState()
        {
            try
            {
                bool isUnlocked = PowerManagerService.AreHiddenSettingsUnlocked();
                bool isEnglish = ConfigService.CurrentLanguage == "English";

                BtnUnlockHidden.Content = isEnglish ? "🔓 Unlock Hidden Settings" : "🔓 Gizli Ayarları Aç";
                BtnLockHidden.Content = isEnglish ? "🔒 Lock Hidden Settings" : "🔒 Gizli Ayarları Kapat";

                if (isUnlocked)
                {
                    BtnUnlockHidden.Opacity = 1.0;
                    BtnLockHidden.Opacity = 0.65;
                    BtnUnlockHidden.ToolTip = isEnglish
                        ? "Advanced hidden power settings are currently unlocked and visible in Windows Control Panel."
                        : "Gelişmiş gizli güç ayarları şu anda Windows Denetim Masası'nda açık ve görünür durumdadır.";
                    BtnLockHidden.ToolTip = isEnglish
                        ? "Click to hide and lock advanced power settings in Windows Control Panel."
                        : "Windows Denetim Masasındaki gelişmiş güç ayarlarını tekrar gizlemek ve kilitlemek için tıklayın.";
                }
                else
                {
                    BtnUnlockHidden.Opacity = 0.65;
                    BtnLockHidden.Opacity = 1.0;
                    BtnUnlockHidden.ToolTip = isEnglish
                        ? "Click to unlock and make advanced hidden power settings visible in Windows Control Panel."
                        : "Windows Denetim Masasında bu gelişmiş gizli menülerin görünmesini sağlamak için tıklayın.";
                    BtnLockHidden.ToolTip = isEnglish
                        ? "Advanced hidden power settings are currently locked and hidden in Windows Control Panel."
                        : "Gelişmiş güç ayarları şu anda Windows Denetim Masası'nda kilitli ve gizlidir.";
                }
            }
            catch { }
        }

        private void BtnUnlockHidden_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isEnglish = ConfigService.CurrentLanguage == "English";
                PowerManagerService.UnlockAllHiddenSettings(true);
                UpdateUnlockButtonState();

                string msg = isEnglish
                    ? "All advanced power settings (Minimum/Maximum Processor State, System Cooling Policy, Turbo Boost, Core Parking, PCIe ASPM, etc.) have been unlocked in Windows Control Panel!\n\nNote: If Windows Control Panel was already open, please close and click 'Windows Advanced Settings...' to see the updated options."
                    : "Windows Denetim Masasındaki tüm gelişmiş güç ayarlarının (En Düşük/Yüksek İşlemci Durumu, Sistem Soğutma İlkesi, Turbo Boost, Çekirdek Park Etme, PCIe ASPM vb.) kilidi başarıyla açıldı!\n\nNot: Eğer Denetim Masası penceresi zaten açıksa, ayarların yenilenmesi için pencereyi kapatıp 'Windows Gelişmiş Ayarlar...' butonuna tıklayarak tekrar açın.";
                string title = isEnglish ? "Settings Unlocked" : "Kilitler Açıldı";
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İşlem sırasında hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLockHidden_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isEnglish = ConfigService.CurrentLanguage == "English";
                PowerManagerService.UnlockAllHiddenSettings(false);
                UpdateUnlockButtonState();

                string msg = isEnglish
                    ? "All advanced hidden power settings have been hidden and locked in Windows Control Panel."
                    : "Windows Denetim Masasındaki gelişmiş güç ayarları başarıyla gizlendi ve kilitlendi.";
                string title = isEnglish ? "Settings Locked" : "Ayarlar Gizlendi";
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İşlem sırasında hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAdvanced_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProcessHelper.OpenUri("control.exe", "powercfg.cpl,,1");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gelişmiş ayarlar paneli açılamadı:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
