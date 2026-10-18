using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using PowerGridHaYTooL.Helpers;
using PowerGridHaYTooL.Models;
using PowerGridHaYTooL.Services;

namespace PowerGridHaYTooL.UI
{
    public class PlanComparisonRow
    {
        public string Category { get; set; } = string.Empty;
        public string SettingName { get; set; } = string.Empty;
        public string SettingKey { get; set; } = string.Empty;
        public List<string> Values { get; set; } = new List<string>();
        public bool HasDifferences { get; set; }
        public string StatusText => HasDifferences ? "Farklı" : "Aynı";
        public Brush StatusBg => HasDifferences 
            ? new SolidColorBrush(Color.FromArgb(45, 230, 126, 34)) 
            : new SolidColorBrush(Color.FromArgb(35, 46, 204, 113));
        public Brush StatusFg => HasDifferences 
            ? new SolidColorBrush(Color.FromRgb(243, 156, 18)) 
            : new SolidColorBrush(Color.FromRgb(46, 204, 113));
    }

    public partial class ComparePlansWindow : Window
    {
        private List<PowerPlanItem> _plans = new List<PowerPlanItem>();
        private List<PowerPlanSettings> _planSettings = new List<PowerPlanSettings>();
        private readonly List<PlanComparisonRow> _allRows = new List<PlanComparisonRow>();
        private readonly ObservableCollection<PlanComparisonRow> _filteredRows = new ObservableCollection<PlanComparisonRow>();
        private bool _isInitializing = true;
        private PlanComparisonRow? _currentQuickRow;

        public ComparePlansWindow(string? preferredPlanGuid = null, string? ignoredGuid = null)
        {
            InitializeComponent();
            ThemeService.ApplyTheme(ConfigService.CurrentTheme, this);
            LstComparison.ItemsSource = _filteredRows;
            ApplyLocalization();
            LoadAndCompareAllPlans();
        }

        private void ApplyLocalization()
        {
            bool isEnglish = ConfigService.CurrentLanguage == "English";
            if (isEnglish)
            {
                Title = "Compare & Manage All Power Plans";
                TxtHeaderTitle.Text = "Compare & Manage All Power Plans";
                TxtHeaderSubtitle.Text = "Compare all power plans side-by-side, inspect differences, and double-click to edit settings directly in-place.";
                BtnRefresh.Content = "🔄 Refresh";
                ChkOnlyDifferences.Content = "Show Only Differences";
                BtnQuickEditSelected.Content = "⚡ Quick Edit Setting...";
                BtnClose.Content = "Close";
                TxtQuickEditHeader.Text = "Quick Edit Setting In-Place";
                ChkApplyToAllPlans.Content = "Apply this new value to ALL power plans simultaneously";
            }
        }

        public void LoadAndCompareAllPlans()
        {
            _isInitializing = true;
            try
            {
                _plans = PowerManagerService.GetPowerPlans();
                _planSettings.Clear();
                _allRows.Clear();

                foreach (var p in _plans)
                {
                    var s = PowerManagerService.GetPlanSettings(p.Guid, p.Name);
                    _planSettings.Add(s);
                }

                BuildMatrixRows();
                BuildGridColumns();
                ApplyFilters();
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void BuildMatrixRows()
        {
            bool isDesktop = SystemInfoHelper.IsDesktopPc();
            bool isEn = ConfigService.CurrentLanguage == "English";

            string catCpu = isEn ? "Processor (CPU)" : "İşlemci (CPU)";
            string catSleep = isEn ? "Display & Sleep" : "Ekran & Uyku";
            string catHw = isEn ? "Hardware (PCIe/USB/WiFi)" : "Donanım (PCIe/USB/WiFi)";
            string catButtons = isEn ? "Power Buttons" : "Güç Düğmeleri";

            // 1. İşlemci Ayarları
            AddRow(catCpu, isDesktop ? "En Düşük İşlemci" : "En Düşük İşlemci (Priz)", "PROCTHROTTLEMIN_AC", s => $"{s.MinCpuAc}%");
            if (!isDesktop) AddRow(catCpu, "En Düşük İşlemci (Pil)", "PROCTHROTTLEMIN_DC", s => $"{s.MinCpuDc}%");

            AddRow(catCpu, isDesktop ? "En Yüksek İşlemci" : "En Yüksek İşlemci (Priz)", "PROCTHROTTLEMAX_AC", s => $"{s.MaxCpuAc}%");
            if (!isDesktop) AddRow(catCpu, "En Yüksek İşlemci (Pil)", "PROCTHROTTLEMAX_DC", s => $"{s.MaxCpuDc}%");

            AddRow(catCpu, isDesktop ? "Sistem Soğutma İlkesi" : "Sistem Soğutma (Priz)", "SYSCOOLPOL_AC", s => FormatCooling(s.CoolingPolicyAc));
            if (!isDesktop) AddRow(catCpu, "Sistem Soğutma (Pil)", "SYSCOOLPOL_DC", s => FormatCooling(s.CoolingPolicyDc));

            AddRow(catCpu, isDesktop ? "Turbo Boost Modu" : "Turbo Boost (Priz)", "PERFBOOSTMODE_AC", s => FormatBoost(s.BoostModeAc));
            if (!isDesktop) AddRow(catCpu, "Turbo Boost (Pil)", "PERFBOOSTMODE_DC", s => FormatBoost(s.BoostModeDc));

            AddRow(catCpu, isDesktop ? "Çekirdek Park Etme Min" : "Çekirdek Park Min (Priz)", "CPMINCORES_AC", s => $"{s.CoreParkingMinAc}%");
            if (!isDesktop) AddRow(catCpu, "Çekirdek Park Min (Pil)", "CPMINCORES_DC", s => $"{s.CoreParkingMinDc}%");

            AddRow(catCpu, isDesktop ? "Çekirdek Park Etme Max" : "Çekirdek Park Max (Priz)", "CPMAXCORES_AC", s => $"{s.CoreParkingMaxAc}%");
            if (!isDesktop) AddRow(catCpu, "Çekirdek Park Max (Pil)", "CPMAXCORES_DC", s => $"{s.CoreParkingMaxDc}%");

            AddRow(catCpu, isDesktop ? "Enerji Performans Tercihi EPP" : "Enerji Tercihi EPP (Priz)", "PERFEPP_AC", s => s.EnergyPerfPreferenceAc.ToString());
            if (!isDesktop) AddRow(catCpu, "Enerji Tercihi EPP (Pil)", "PERFEPP_DC", s => s.EnergyPerfPreferenceDc.ToString());

            // 2. Ekran, Uyku & Disk
            AddRow(catSleep, isDesktop ? "Ekranı Kapatma Süresi" : "Ekranı Kapat (Priz)", "VIDEOIDLE_AC", s => FormatTimeout(s.VideoTimeoutAcMinutes));
            if (!isDesktop) AddRow(catSleep, "Ekranı Kapat (Pil)", "VIDEOIDLE_DC", s => FormatTimeout(s.VideoTimeoutDcMinutes));

            AddRow(catSleep, isDesktop ? "Uykuya Geçme Süresi" : "Uyku Modu (Priz)", "STANDBYIDLE_AC", s => FormatTimeout(s.SleepTimeoutAcMinutes));
            if (!isDesktop) AddRow(catSleep, "Uyku Modu (Pil)", "STANDBYIDLE_DC", s => FormatTimeout(s.SleepTimeoutDcMinutes));

            AddRow(catSleep, isDesktop ? "Hazırda Bekletme Süresi" : "Hazırda Beklet (Priz)", "HIBERNATEIDLE_AC", s => FormatTimeout(s.HibernateTimeoutAc));
            if (!isDesktop) AddRow(catSleep, "Hazırda Beklet (Pil)", "HIBERNATEIDLE_DC", s => FormatTimeout(s.HibernateTimeoutDc));

            AddRow(catSleep, isDesktop ? "Sabit Diski Kapatma Süresi" : "Sabit Diski Kapat (Priz)", "DISKIDLE_AC", s => FormatTimeout(s.DiskTimeoutAcMinutes));
            if (!isDesktop) AddRow(catSleep, "Sabit Diski Kapat (Pil)", "DISKIDLE_DC", s => FormatTimeout(s.DiskTimeoutDcMinutes));

            AddRow(catSleep, isDesktop ? "Uyandırma Zamanlayıcıları" : "Uyandırma Zamanlayıcı (Priz)", "RTCWAKE_AC", s => FormatWake(s.AllowWakeTimersAc));
            if (!isDesktop) AddRow(catSleep, "Uyandırma Zamanlayıcı (Pil)", "RTCWAKE_DC", s => FormatWake(s.AllowWakeTimersDc));

            // 3. Donanım & Çevre Birimleri
            AddRow(catHw, isDesktop ? "PCI Express ASPM Tasarrufu" : "PCIe ASPM (Priz)", "ASPM_AC", s => FormatAspm(s.PcieAspmAc));
            if (!isDesktop) AddRow(catHw, "PCIe ASPM (Pil)", "ASPM_DC", s => FormatAspm(s.PcieAspmDc));

            AddRow(catHw, isDesktop ? "USB Seçici Askıya Alma" : "USB Askıya Alma (Priz)", "USBSELECT_AC", s => FormatUsb(s.UsbSelectiveSuspendAc));
            if (!isDesktop) AddRow(catHw, "USB Askıya Alma (Pil)", "USBSELECT_DC", s => FormatUsb(s.UsbSelectiveSuspendDc));

            AddRow(catHw, isDesktop ? "Wi-Fi Güç Tasarrufu" : "Wi-Fi Güç Tasarrufu (Priz)", "WIFIPOWER_AC", s => FormatWifi(s.WifiPowerSavingAc));
            if (!isDesktop) AddRow(catHw, "Wi-Fi Güç Tasarrufu (Pil)", "WIFIPOWER_DC", s => FormatWifi(s.WifiPowerSavingDc));

            // 4. Güç Düğmeleri
            AddRow(catButtons, isDesktop ? "Güç Düğmesi Eylemi" : "Güç Düğmesi (Priz)", "PBUTTONACTION_AC", s => FormatButton(s.PowerButtonActionAc));
            if (!isDesktop) AddRow(catButtons, "Güç Düğmesi (Pil)", "PBUTTONACTION_DC", s => FormatButton(s.PowerButtonActionDc));

            AddRow(catButtons, isDesktop ? "Uyku Düğmesi Eylemi" : "Uyku Düğmesi (Priz)", "SBUTTONACTION_AC", s => FormatButton(s.SleepButtonActionAc));
            if (!isDesktop) AddRow(catButtons, "Uyku Düğmesi (Pil)", "SBUTTONACTION_DC", s => FormatButton(s.SleepButtonActionDc));
        }

        private void AddRow(string category, string settingName, string key, Func<PowerPlanSettings, string> valueSelector)
        {
            var row = new PlanComparisonRow
            {
                Category = category,
                SettingName = settingName,
                SettingKey = key
            };

            foreach (var s in _planSettings)
            {
                string val = valueSelector(s);
                row.Values.Add(val);
            }

            row.HasDifferences = row.Values.Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1;
            _allRows.Add(row);
        }

        private void BuildGridColumns()
        {
            GvComparison.Columns.Clear();
            bool isEn = ConfigService.CurrentLanguage == "English";

            // Sütun 0: Ayar Adı ve Kategori
            string colSettingHeader = isEn ? "Setting / Hardware Feature" : "Ayar / Donanım Özelliği";
            string col0Xaml = $@"
            <DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                <StackPanel Margin=""4,3"">
                    <TextBlock Text=""{{Binding SettingName}}"" FontWeight=""SemiBold"" Foreground=""{{DynamicResource BrushTextPrimary}}"" FontSize=""12""/>
                    <TextBlock Text=""{{Binding Category}}"" FontSize=""10"" Foreground=""{{DynamicResource BrushTextSecondary}}"" Margin=""0,1,0,0""/>
                </StackPanel>
            </DataTemplate>";

            var colSetting = new GridViewColumn
            {
                Header = colSettingHeader,
                Width = 230,
                CellTemplate = (DataTemplate)XamlReader.Parse(col0Xaml)
            };
            GvComparison.Columns.Add(colSetting);

            // Sütun 1..N: Her bir Güç Planı
            for (int i = 0; i < _plans.Count; i++)
            {
                var plan = _plans[i];
                int planIndex = i;

                // Dinamik Plan Başlık Kartı
                var headerPanel = new Border
                {
                    Background = (Brush)FindResource("BrushHeaderBg"),
                    BorderBrush = (Brush)FindResource("BrushBorder"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(8, 6, 8, 6),
                    Margin = new Thickness(2, 0, 2, 0)
                };

                var headerStack = new StackPanel { Orientation = Orientation.Vertical };

                // İkon, İsim ve Aktif Rozeti
                var titleStack = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
                titleStack.Children.Add(new TextBlock
                {
                    Text = plan.PlanIcon,
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 6, 0),
                    VerticalAlignment = VerticalAlignment.Center
                });

                titleStack.Children.Add(new TextBlock
                {
                    Text = plan.Name,
                    FontWeight = FontWeights.Bold,
                    Foreground = (Brush)FindResource("BrushTextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center,
                    MaxWidth = 140,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    ToolTip = plan.Name
                });

                if (plan.IsActive)
                {
                    var badgeBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(40, 46, 204, 113)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(4, 1, 4, 1),
                        Margin = new Thickness(6, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    badgeBorder.Child = new TextBlock
                    {
                        Text = isEn ? "Active" : "Aktif",
                        Foreground = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                        FontSize = 9,
                        FontWeight = FontWeights.Bold
                    };
                    titleStack.Children.Add(badgeBorder);
                }

                headerStack.Children.Add(titleStack);

                // Aksiyon Butonları (✏️ Düzenle & ⚡ Aktif Yap)
                var btnStack = new StackPanel { Orientation = Orientation.Horizontal };

                var btnEdit = new Button
                {
                    Content = isEn ? "✏️ Full Edit" : "✏️ Tümünü Düzenle",
                    Style = (Style)FindResource("ModernBtn"),
                    Padding = new Thickness(6, 2, 6, 2),
                    Margin = new Thickness(0, 0, 6, 0),
                    FontSize = 10,
                    ToolTip = isEn ? "Open full advanced plan editor" : "Tüm gelişmiş ayarlar penceresini aç"
                };
                btnEdit.Click += (s, e) => OpenPlanEditor(plan);
                btnStack.Children.Add(btnEdit);

                if (!plan.IsActive)
                {
                    var btnActivate = new Button
                    {
                        Content = isEn ? "⚡ Activate" : "⚡ Aktif Yap",
                        Style = (Style)FindResource("ModernBtn"),
                        Padding = new Thickness(6, 2, 6, 2),
                        FontSize = 10,
                        Foreground = (Brush)FindResource("BrushAccent"),
                        ToolTip = isEn ? "Make this plan active on Windows" : "Bu planı sistemde aktif yap"
                    };
                    btnActivate.Click += (s, e) => ActivatePlan(plan);
                    btnStack.Children.Add(btnActivate);
                }

                headerStack.Children.Add(btnStack);
                headerPanel.Child = headerStack;

                // Değer Hücresi Şablonu (Values[planIndex])
                string cellXaml = $@"
                <DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                    <Border Background=""Transparent"" Padding=""6,4"" Cursor=""Hand"" ToolTip=""Değeri doğrudan değiştirmek için çift tıklayın"">
                        <TextBlock Text=""{{Binding Values[{planIndex}]}}"" 
                                   Foreground=""{{DynamicResource BrushTextPrimary}}"" 
                                   VerticalAlignment=""Center"" 
                                   TextWrapping=""Wrap""/>
                    </Border>
                </DataTemplate>";

                var planColumn = new GridViewColumn
                {
                    Header = headerPanel,
                    Width = 190,
                    CellTemplate = (DataTemplate)XamlReader.Parse(cellXaml)
                };

                GvComparison.Columns.Add(planColumn);
            }

            // Son Sütun: Durum (Aynı / Farklı Rozeti)
            string colStatusHeader = isEn ? "Status" : "Durum";
            string colStatusXaml = $@"
            <DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                <Border Background=""{{Binding StatusBg}}"" CornerRadius=""4"" Padding=""6,3"" HorizontalAlignment=""Center"" VerticalAlignment=""Center"">
                    <TextBlock Text=""{{Binding StatusText}}"" Foreground=""{{Binding StatusFg}}"" FontSize=""10"" FontWeight=""Bold""/>
                </Border>
            </DataTemplate>";

            var colStatus = new GridViewColumn
            {
                Header = colStatusHeader,
                Width = 85,
                CellTemplate = (DataTemplate)XamlReader.Parse(colStatusXaml)
            };
            GvComparison.Columns.Add(colStatus);
        }

        private void ApplyFilters()
        {
            _filteredRows.Clear();
            string search = TxtSearch?.Text?.Trim().ToLowerInvariant() ?? string.Empty;
            string selectedCat = (CmbCategoryFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Tüm Kategoriler";
            bool onlyDiff = ChkOnlyDifferences?.IsChecked == true;

            int diffCount = _allRows.Count(r => r.HasDifferences);

            foreach (var r in _allRows)
            {
                if (onlyDiff && !r.HasDifferences)
                    continue;

                if (selectedCat != "Tüm Kategoriler" && selectedCat != "All Categories")
                {
                    if (!r.Category.Contains(selectedCat.Split(' ')[0]))
                        continue;
                }

                if (!string.IsNullOrEmpty(search))
                {
                    bool matchName = r.SettingName.ToLowerInvariant().Contains(search);
                    bool matchCat = r.Category.ToLowerInvariant().Contains(search);
                    bool matchVal = r.Values.Any(v => v.ToLowerInvariant().Contains(search));
                    if (!matchName && !matchCat && !matchVal)
                        continue;
                }

                _filteredRows.Add(r);
            }

            bool isEn = ConfigService.CurrentLanguage == "English";
            TxtStatusSummary.Text = isEn
                ? $"{_plans.Count} Plans • {_filteredRows.Count} Settings Listed ({diffCount} differences)"
                : $"{_plans.Count} Güç Planı • {_filteredRows.Count} Ayar Listelendi ({diffCount} ayarda fark var)";
        }

        // --- In-Place Quick Edit Engine ---
        private void ShowQuickEditor(PlanComparisonRow row, int preferredPlanIndex)
        {
            if (row == null || row.SettingKey == "GUID" || _plans.Count == 0) return;

            _currentQuickRow = row;
            TxtQuickSettingTitle.Text = row.SettingName;

            // Plan Listesini Doldur
            CmbQuickPlan.Items.Clear();
            for (int i = 0; i < _plans.Count; i++)
            {
                var p = _plans[i];
                var cbi = new ComboBoxItem
                {
                    Content = $"{p.PlanIcon} {p.Name} {(p.IsActive ? "⚡" : "")}",
                    Tag = p
                };
                CmbQuickPlan.Items.Add(cbi);
            }

            int selIdx = (preferredPlanIndex >= 0 && preferredPlanIndex < _plans.Count) ? preferredPlanIndex : 0;
            CmbQuickPlan.SelectedIndex = selIdx;

            // Kontrol tipini ayarla
            ConfigureQuickInputControls(row.SettingKey, selIdx);

            ChkApplyToAllPlans.IsChecked = false;
            OverlayQuickEdit.Visibility = Visibility.Visible;
        }

        private void ConfigureQuickInputControls(string key, int planIndex)
        {
            var s = (planIndex >= 0 && planIndex < _planSettings.Count) ? _planSettings[planIndex] : _planSettings.FirstOrDefault();
            if (s == null) return;

            CmbQuickValue.Items.Clear();

            // Yüzde (%) Değerleri -> Slider
            if (key.StartsWith("PROCTHROTTLEMIN") || key.StartsWith("PROCTHROTTLEMAX") ||
                key.StartsWith("CPMINCORES") || key.StartsWith("CPMAXCORES") || key.StartsWith("PERFEPP"))
            {
                PanelQuickCombo.Visibility = Visibility.Collapsed;
                PanelQuickSlider.Visibility = Visibility.Visible;

                int currentVal = key switch
                {
                    "PROCTHROTTLEMIN_AC" => s.MinCpuAc,
                    "PROCTHROTTLEMIN_DC" => s.MinCpuDc,
                    "PROCTHROTTLEMAX_AC" => s.MaxCpuAc,
                    "PROCTHROTTLEMAX_DC" => s.MaxCpuDc,
                    "CPMINCORES_AC" => s.CoreParkingMinAc,
                    "CPMINCORES_DC" => s.CoreParkingMinDc,
                    "CPMAXCORES_AC" => s.CoreParkingMaxAc,
                    "CPMAXCORES_DC" => s.CoreParkingMaxDc,
                    "PERFEPP_AC" => s.EnergyPerfPreferenceAc,
                    "PERFEPP_DC" => s.EnergyPerfPreferenceDc,
                    _ => 100
                };

                SldQuickValue.Value = currentVal;
                TxtQuickSliderVal.Text = $"{currentVal}%";
                return;
            }

            // Seçenekli Değerler -> ComboBox
            PanelQuickCombo.Visibility = Visibility.Visible;
            PanelQuickSlider.Visibility = Visibility.Collapsed;

            if (key.StartsWith("SYSCOOLPOL"))
            {
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Etkin (Aktif Fan)", Tag = 1 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Edilgin (Sessiz)", Tag = 0 });
                int current = key.EndsWith("AC") ? s.CoolingPolicyAc : s.CoolingPolicyDc;
                SelectComboByTag(CmbQuickValue, current == 1 ? 1 : 0);
            }
            else if (key.StartsWith("PERFBOOSTMODE"))
            {
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "0 - Devre Dışı (Serin)", Tag = 0 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "1 - Etkin", Tag = 1 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "2 - Agresif (Maks Hız)", Tag = 2 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "3 - Verimli Etkin", Tag = 3 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "4 - Verimli Agresif", Tag = 4 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "5 - Agresif Garantili", Tag = 5 });
                int current = key.EndsWith("AC") ? s.BoostModeAc : s.BoostModeDc;
                SelectComboByTag(CmbQuickValue, current);
            }
            else if (key.StartsWith("ASPM"))
            {
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Kapalı (Maks Performans)", Tag = 0 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Orta Düzey Tasarruf", Tag = 1 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Maksimum Güç Tasarrufu", Tag = 2 });
                int current = key.EndsWith("AC") ? s.PcieAspmAc : s.PcieAspmDc;
                SelectComboByTag(CmbQuickValue, current);
            }
            else if (key.StartsWith("USBSELECT"))
            {
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Devre Dışı (Performans)", Tag = 0 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Etkin (Tasarruf)", Tag = 1 });
                int current = key.EndsWith("AC") ? s.UsbSelectiveSuspendAc : s.UsbSelectiveSuspendDc;
                SelectComboByTag(CmbQuickValue, current);
            }
            else if (key.StartsWith("WIFIPOWER"))
            {
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Maksimum Performans", Tag = 0 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Düşük Tasarruf", Tag = 1 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Orta Tasarruf", Tag = 2 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Maksimum Tasarruf", Tag = 3 });
                int current = key.EndsWith("AC") ? s.WifiPowerSavingAc : s.WifiPowerSavingDc;
                SelectComboByTag(CmbQuickValue, current);
            }
            else if (key.StartsWith("RTCWAKE"))
            {
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Devre Dışı", Tag = 0 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Etkin", Tag = 1 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Yalnızca Önemli", Tag = 2 });
                int current = key.EndsWith("AC") ? s.AllowWakeTimersAc : s.AllowWakeTimersDc;
                SelectComboByTag(CmbQuickValue, current);
            }
            else if (key.StartsWith("PBUTTONACTION") || key.StartsWith("SBUTTONACTION"))
            {
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Hiçbir şey yapma", Tag = 0 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Uyku", Tag = 1 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Hazırda Beklet", Tag = 2 });
                CmbQuickValue.Items.Add(new ComboBoxItem { Content = "Kapat", Tag = 3 });
                int current = 3;
                if (key == "PBUTTONACTION_AC") current = s.PowerButtonActionAc;
                else if (key == "PBUTTONACTION_DC") current = s.PowerButtonActionDc;
                else if (key == "SBUTTONACTION_AC") current = s.SleepButtonActionAc;
                else if (key == "SBUTTONACTION_DC") current = s.SleepButtonActionDc;
                SelectComboByTag(CmbQuickValue, current);
            }
            else
            {
                // Süre Seçenekleri (Dakika)
                int[] timeouts = { 0, 1, 2, 3, 5, 10, 15, 20, 25, 30, 45, 60, 120, 180, 240, 300 };
                foreach (int min in timeouts)
                {
                    string label = min == 0 ? "Hiçbir zaman" : (min < 60 ? $"{min} dakika" : $"{min / 60} saat");
                    CmbQuickValue.Items.Add(new ComboBoxItem { Content = label, Tag = min });
                }

                int currentMin = key switch
                {
                    "VIDEOIDLE_AC" => s.VideoTimeoutAcMinutes,
                    "VIDEOIDLE_DC" => s.VideoTimeoutDcMinutes,
                    "STANDBYIDLE_AC" => s.SleepTimeoutAcMinutes,
                    "STANDBYIDLE_DC" => s.SleepTimeoutDcMinutes,
                    "HIBERNATEIDLE_AC" => s.HibernateTimeoutAc,
                    "HIBERNATEIDLE_DC" => s.HibernateTimeoutDc,
                    "DISKIDLE_AC" => s.DiskTimeoutAcMinutes,
                    "DISKIDLE_DC" => s.DiskTimeoutDcMinutes,
                    _ => 0
                };

                SelectComboByTag(CmbQuickValue, currentMin);
            }
        }

        private static void SelectComboByTag(ComboBox cmb, int targetTag)
        {
            foreach (ComboBoxItem item in cmb.Items)
            {
                if (item.Tag is int val && val == targetTag)
                {
                    cmb.SelectedItem = item;
                    return;
                }
            }
            if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
        }

        private void CmbQuickPlan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_currentQuickRow != null)
            {
                ConfigureQuickInputControls(_currentQuickRow.SettingKey, CmbQuickPlan.SelectedIndex);
            }
        }

        private void SldQuickValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtQuickSliderVal != null)
            {
                TxtQuickSliderVal.Text = $"{(int)Math.Round(e.NewValue)}%";
            }
        }

        private void BtnSaveQuickEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_currentQuickRow == null) return;

            try
            {
                int intValue = 0;
                if (PanelQuickSlider.Visibility == Visibility.Visible)
                {
                    intValue = (int)Math.Round(SldQuickValue.Value);
                }
                else
                {
                    if (CmbQuickValue.SelectedItem is ComboBoxItem cbi && cbi.Tag is int val)
                    {
                        intValue = val;
                    }
                }

                bool applyToAll = ChkApplyToAllPlans.IsChecked == true;
                bool isDesktop = SystemInfoHelper.IsDesktopPc();
                string key = _currentQuickRow.SettingKey;

                List<PowerPlanItem> targetPlans = applyToAll
                    ? _plans.ToList()
                    : new List<PowerPlanItem> { (CmbQuickPlan.SelectedItem as ComboBoxItem)?.Tag as PowerPlanItem ?? _plans[0] };

                foreach (var plan in targetPlans)
                {
                    var s = PowerManagerService.GetPlanSettings(plan.Guid, plan.Name);
                    ApplyValueToSettings(s, key, intValue, isDesktop);
                    PowerManagerService.SavePlanSettings(s);
                }

                OverlayQuickEdit.Visibility = Visibility.Collapsed;
                LoadAndCompareAllPlans();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ayar kaydedilirken hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void ApplyValueToSettings(PowerPlanSettings s, string key, int val, bool isDesktop)
        {
            switch (key)
            {
                case "PROCTHROTTLEMIN_AC":
                    s.MinCpuAc = val;
                    if (isDesktop) s.MinCpuDc = val;
                    break;
                case "PROCTHROTTLEMIN_DC":
                    s.MinCpuDc = val;
                    break;
                case "PROCTHROTTLEMAX_AC":
                    s.MaxCpuAc = val;
                    if (isDesktop) s.MaxCpuDc = val;
                    break;
                case "PROCTHROTTLEMAX_DC":
                    s.MaxCpuDc = val;
                    break;
                case "SYSCOOLPOL_AC":
                    s.CoolingPolicyAc = val;
                    if (isDesktop) s.CoolingPolicyDc = val;
                    break;
                case "SYSCOOLPOL_DC":
                    s.CoolingPolicyDc = val;
                    break;
                case "PERFBOOSTMODE_AC":
                    s.BoostModeAc = val;
                    if (isDesktop) s.BoostModeDc = val;
                    break;
                case "PERFBOOSTMODE_DC":
                    s.BoostModeDc = val;
                    break;
                case "CPMINCORES_AC":
                    s.CoreParkingMinAc = val;
                    if (isDesktop) s.CoreParkingMinDc = val;
                    break;
                case "CPMINCORES_DC":
                    s.CoreParkingMinDc = val;
                    break;
                case "CPMAXCORES_AC":
                    s.CoreParkingMaxAc = val;
                    if (isDesktop) s.CoreParkingMaxDc = val;
                    break;
                case "CPMAXCORES_DC":
                    s.CoreParkingMaxDc = val;
                    break;
                case "PERFEPP_AC":
                    s.EnergyPerfPreferenceAc = val;
                    if (isDesktop) s.EnergyPerfPreferenceDc = val;
                    break;
                case "PERFEPP_DC":
                    s.EnergyPerfPreferenceDc = val;
                    break;
                case "VIDEOIDLE_AC":
                    s.VideoTimeoutAcMinutes = val;
                    if (isDesktop) s.VideoTimeoutDcMinutes = val;
                    break;
                case "VIDEOIDLE_DC":
                    s.VideoTimeoutDcMinutes = val;
                    break;
                case "STANDBYIDLE_AC":
                    s.SleepTimeoutAcMinutes = val;
                    if (isDesktop) s.SleepTimeoutDcMinutes = val;
                    break;
                case "STANDBYIDLE_DC":
                    s.SleepTimeoutDcMinutes = val;
                    break;
                case "HIBERNATEIDLE_AC":
                    s.HibernateTimeoutAc = val;
                    if (isDesktop) s.HibernateTimeoutDc = val;
                    break;
                case "HIBERNATEIDLE_DC":
                    s.HibernateTimeoutDc = val;
                    break;
                case "DISKIDLE_AC":
                    s.DiskTimeoutAcMinutes = val;
                    if (isDesktop) s.DiskTimeoutDcMinutes = val;
                    break;
                case "DISKIDLE_DC":
                    s.DiskTimeoutDcMinutes = val;
                    break;
                case "RTCWAKE_AC":
                    s.AllowWakeTimersAc = val;
                    if (isDesktop) s.AllowWakeTimersDc = val;
                    break;
                case "RTCWAKE_DC":
                    s.AllowWakeTimersDc = val;
                    break;
                case "ASPM_AC":
                    s.PcieAspmAc = val;
                    if (isDesktop) s.PcieAspmDc = val;
                    break;
                case "ASPM_DC":
                    s.PcieAspmDc = val;
                    break;
                case "USBSELECT_AC":
                    s.UsbSelectiveSuspendAc = val;
                    if (isDesktop) s.UsbSelectiveSuspendDc = val;
                    break;
                case "USBSELECT_DC":
                    s.UsbSelectiveSuspendDc = val;
                    break;
                case "WIFIPOWER_AC":
                    s.WifiPowerSavingAc = val;
                    if (isDesktop) s.WifiPowerSavingDc = val;
                    break;
                case "WIFIPOWER_DC":
                    s.WifiPowerSavingDc = val;
                    break;
                case "PBUTTONACTION_AC":
                    s.PowerButtonActionAc = val;
                    if (isDesktop) s.PowerButtonActionDc = val;
                    break;
                case "PBUTTONACTION_DC":
                    s.PowerButtonActionDc = val;
                    break;
                case "SBUTTONACTION_AC":
                    s.SleepButtonActionAc = val;
                    if (isDesktop) s.SleepButtonActionDc = val;
                    break;
                case "SBUTTONACTION_DC":
                    s.SleepButtonActionDc = val;
                    break;
            }
        }

        private void BtnCancelQuickEdit_Click(object sender, RoutedEventArgs e)
        {
            OverlayQuickEdit.Visibility = Visibility.Collapsed;
        }

        private void OverlayBackdrop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OverlayQuickEdit.Visibility = Visibility.Collapsed;
        }

        private void LstComparison_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LstComparison.SelectedItem is PlanComparisonRow row)
            {
                int activeIndex = _plans.FindIndex(p => p.IsActive);
                ShowQuickEditor(row, activeIndex >= 0 ? activeIndex : 0);
            }
        }

        private void BtnQuickEditSelected_Click(object sender, RoutedEventArgs e)
        {
            var row = LstComparison.SelectedItem as PlanComparisonRow ?? _filteredRows.FirstOrDefault();
            if (row != null)
            {
                int activeIndex = _plans.FindIndex(p => p.IsActive);
                ShowQuickEditor(row, activeIndex >= 0 ? activeIndex : 0);
            }
        }

        private void OpenPlanEditor(PowerPlanItem plan)
        {
            var editor = new PlanEditorWindow(plan.Guid, plan.Name) { Owner = this };
            if (editor.ShowDialog() == true)
            {
                LoadAndCompareAllPlans();
            }
        }

        private void ActivatePlan(PowerPlanItem plan)
        {
            try
            {
                bool ok = PowerManagerService.SetActivePlan(plan.Guid);
                if (ok)
                {
                    LoadAndCompareAllPlans();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Plan aktif edilirken hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInitializing) return;
            ApplyFilters();
        }

        private void CmbCategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            ApplyFilters();
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            ApplyFilters();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAndCompareAllPlans();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // --- Format Yardımcıları ---
        private static string FormatTimeout(int min)
        {
            if (min <= 0) return "Hiçbir zaman";
            if (min < 60) return $"{min} dakika";
            double hours = min / 60.0;
            return hours % 1 == 0 ? $"{(int)hours} saat" : $"{hours:0.#} saat";
        }

        private static string FormatCooling(int policy)
        {
            return policy == 1 ? "Etkin (Aktif Fan)" : "Edilgin (Sessiz)";
        }

        private static string FormatBoost(int mode)
        {
            return mode switch
            {
                0 => "0 - Devre Dışı",
                1 => "1 - Etkin",
                2 => "2 - Agresif",
                3 => "3 - Verimli Etkin",
                4 => "4 - Verimli Agresif",
                5 => "5 - Agresif Garantili",
                _ => mode.ToString()
            };
        }

        private static string FormatWake(int wake)
        {
            return wake switch
            {
                0 => "Devre Dışı",
                1 => "Etkin",
                2 => "Yalnızca Önemli",
                _ => wake.ToString()
            };
        }

        private static string FormatAspm(int aspm)
        {
            return aspm switch
            {
                0 => "Kapalı (Maks Performans)",
                1 => "Orta Düzey Tasarruf",
                2 => "Maksimum Güç Tasarrufu",
                _ => aspm.ToString()
            };
        }

        private static string FormatUsb(int usb)
        {
            return usb == 1 ? "Etkin (Tasarruf)" : "Devre Dışı (Performans)";
        }

        private static string FormatWifi(int wifi)
        {
            return wifi switch
            {
                0 => "Maksimum Performans",
                1 => "Düşük Tasarruf",
                2 => "Orta Tasarruf",
                3 => "Maksimum Tasarruf",
                _ => wifi.ToString()
            };
        }

        private static string FormatButton(int btn)
        {
            return btn switch
            {
                0 => "Hiçbir şey yapma",
                1 => "Uyku",
                2 => "Hazırda Beklet",
                3 => "Kapat",
                _ => btn.ToString()
            };
        }
    }
}
