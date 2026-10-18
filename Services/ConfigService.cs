using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PowerGridHaYTooL.Services
{
    public static class ConfigService
    {
        private static string IniPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PowerGridHaYTooL.ini");

        private static readonly Dictionary<string, Dictionary<string, string>> Sections = new(StringComparer.OrdinalIgnoreCase);

        public static string LanguageSetting { get; set; } = "Auto";
        public static string CurrentLanguage { get; set; } = "Turkish";
        public static string CurrentTheme { get; set; } = "Auto";
        public static bool ShowSystemTray { get; set; } = false;
        public static bool AlwaysOnTop { get; set; } = false;
        public static bool HideWhenMinimized { get; set; } = true;
        public static bool CloseToTray { get; set; } = true;
        public static bool AutoStart { get; set; } = false;
        public static string UserStartupPlanGuid { get; set; } = "";
        public static string UserStartupPlanName { get; set; } = "";

        public static string DetectSystemLanguage()
        {
            try
            {
                var culture = System.Globalization.CultureInfo.CurrentUICulture;
                if (culture.TwoLetterISOLanguageName.Equals("tr", StringComparison.OrdinalIgnoreCase) ||
                    culture.Name.StartsWith("tr", StringComparison.OrdinalIgnoreCase))
                {
                    return "Turkish";
                }
            }
            catch { }
            return "English";
        }

        public static void Load()
        {
            Sections.Clear();
            if (!File.Exists(IniPath))
            {
                SaveDefaultIni();
            }

            try
            {
                string currentSection = "Main";
                var lines = File.ReadAllLines(IniPath, Encoding.UTF8);

                foreach (var line in lines)
                {
                    string trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith(";") || trimmed.StartsWith("#"))
                        continue;

                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    {
                        currentSection = trimmed.Substring(1, trimmed.Length - 2).Trim();
                        if (!Sections.ContainsKey(currentSection))
                            Sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                    else if (trimmed.Contains("="))
                    {
                        int eqIndex = trimmed.IndexOf('=');
                        string key = trimmed.Substring(0, eqIndex).Trim();
                        string val = trimmed.Substring(eqIndex + 1).Trim();

                        if (!Sections.ContainsKey(currentSection))
                            Sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        Sections[currentSection][key] = val;
                    }
                }

                if (Sections.TryGetValue("Main", out var mainSec))
                {
                    if (mainSec.TryGetValue("Language", out var lang) && !string.IsNullOrWhiteSpace(lang))
                    {
                        LanguageSetting = lang;
                    }
                    else
                    {
                        LanguageSetting = "Auto";
                    }

                    if (string.Equals(LanguageSetting, "Auto", StringComparison.OrdinalIgnoreCase))
                    {
                        CurrentLanguage = DetectSystemLanguage();
                    }
                    else
                    {
                        CurrentLanguage = LanguageSetting;
                    }

                    if (mainSec.TryGetValue("Theme", out var thm)) CurrentTheme = thm;
                    if (mainSec.TryGetValue("ShowSystemTray", out var sst)) ShowSystemTray = sst == "1";
                    if (mainSec.TryGetValue("AlwaysOnTop", out var aot)) AlwaysOnTop = aot == "1";
                    if (mainSec.TryGetValue("HideWhenMinimized", out var hwm)) HideWhenMinimized = hwm == "1";
                    if (mainSec.TryGetValue("CloseToTray", out var ctt)) CloseToTray = ctt == "1"; else CloseToTray = true;
                    if (mainSec.TryGetValue("AutoStart", out var as_)) AutoStart = as_ == "1";
                    if (mainSec.TryGetValue("UserStartupPlanGuid", out var usg)) UserStartupPlanGuid = usg;
                    if (mainSec.TryGetValue("UserStartupPlanName", out var usn)) UserStartupPlanName = usn;
                }
            }
            catch { }
        }

        public static string Get(string key, string fallback = "")
        {
            return LocalizationService.GetString(key, fallback);
        }

        public static void Save()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("; ==============================================================================");
                sb.AppendLine("; Power Grid HaYTooL - Yapılandırma Dosyası / Configuration File");
                sb.AppendLine("; ==============================================================================");
                sb.AppendLine("; Bu dosya uygulamanın temel çalışma ve otomasyon ayarlarını içerir.");
                sb.AppendLine("; This file contains the core operational and automation settings of the application.");
                sb.AppendLine("; ==============================================================================");
                sb.AppendLine();

                sb.AppendLine("[Main]");
                sb.AppendLine("; [TR] Dil Seçimi: Auto (Windows sistem dilini otomatik algılar), Turkish veya English");
                sb.AppendLine("; [EN] Language: Auto (Automatically detects Windows system language), Turkish, or English");
                sb.AppendLine("; Varsayılan / Default: Auto");
                sb.AppendLine($"Language={LanguageSetting}");
                sb.AppendLine();

                sb.AppendLine("; [TR] Görsel Tema: Auto (Windows açık/koyu modunu izler), SunsetBoulevard, BotanicalGarden,");
                sb.AppendLine(";      NordicSlateNavy, ObsidianBlack, SilverBlue, OceanDepths, MidnightGalaxy, TechInnovation,");
                sb.AppendLine(";      ForestCanopy, ArcticFrost, DesertRose, GoldenHour, ModernMinimalist");
                sb.AppendLine("; [EN] Theme: Auto (Follows Windows light/dark mode) or one of the theme names above.");
                sb.AppendLine("; Varsayılan / Default: Auto");
                sb.AppendLine($"Theme={CurrentTheme}");
                sb.AppendLine();

                sb.AppendLine("; [TR] Sistem Tepsisi: 1 = Bildirim alanında (saatin yanında) simge göster, 0 = Gösterme");
                sb.AppendLine("; [EN] System Tray: 1 = Show tray icon next to the clock, 0 = Do not show");
                sb.AppendLine("; Varsayılan / Default: 0");
                sb.AppendLine($"ShowSystemTray={(ShowSystemTray ? "1" : "0")}");
                sb.AppendLine();

                sb.AppendLine("; [TR] Her Zaman Üstte: 1 = Pencere diğer pencerelerin üzerinde kalsın, 0 = Normal");
                sb.AppendLine("; [EN] Always On Top: 1 = Keep application window on top of others, 0 = Normal");
                sb.AppendLine("; Varsayılan / Default: 0");
                sb.AppendLine($"AlwaysOnTop={(AlwaysOnTop ? "1" : "0")}");
                sb.AppendLine();

                sb.AppendLine("; [TR] Küçültüldüğünde Gizle: 1 = Pencere küçültüldüğünde görev çubuğundan gizlenip tepsiye geçsin, 0 = Normal");
                sb.AppendLine("; [EN] Hide When Minimized: 1 = Hide from taskbar into tray when minimized, 0 = Normal");
                sb.AppendLine("; Varsayılan / Default: 1");
                sb.AppendLine($"HideWhenMinimized={(HideWhenMinimized ? "1" : "0")}");
                sb.AppendLine();

                sb.AppendLine("; [TR] Kapatıldığında Tepsiye Küçült: 1 = 'X' kapatma butonuna basıldığında arka planda çalışmaya devam etsin, 0 = Çıkış yap");
                sb.AppendLine("; [EN] Close To Tray: 1 = Minimize to system tray when 'X' is clicked, 0 = Exit application");
                sb.AppendLine("; Varsayılan / Default: 1");
                sb.AppendLine($"CloseToTray={(CloseToTray ? "1" : "0")}");
                sb.AppendLine();

                sb.AppendLine("; [TR] Windows Başlangıcında Çalıştır: 1 = Bilgisayar açıldığında otomatik başlasın, 0 = Başlamasın");
                sb.AppendLine("; [EN] Start With Windows: 1 = Launch automatically on Windows startup, 0 = Do not launch");
                sb.AppendLine("; Varsayılan / Default: 0");
                sb.AppendLine($"AutoStart={(AutoStart ? "1" : "0")}");
                sb.AppendLine();

                sb.AppendLine("; [TR] Kullanıcı Oturum Açılış Güç Planı GUID'i (Boş bırakılırsa Windows varsayılanını kullanır)");
                sb.AppendLine("; [EN] User Startup Power Plan GUID (Leave empty to use Windows default)");
                sb.AppendLine("; Varsayılan / Default: (boş / empty)");
                sb.AppendLine($"UserStartupPlanGuid={UserStartupPlanGuid}");
                sb.AppendLine();

                sb.AppendLine("; [TR] Kullanıcı Oturum Açılış Güç Planı Adı");
                sb.AppendLine("; [EN] User Startup Power Plan Name");
                sb.AppendLine("; Varsayılan / Default: (boş / empty)");
                sb.AppendLine($"UserStartupPlanName={UserStartupPlanName}");
                sb.AppendLine();

                // SmartSwitch bölümünü de ekle
                AppendSmartSwitchSection(sb);

                File.WriteAllText(IniPath, sb.ToString(), Encoding.UTF8);
            }
            catch { }
        }

        private static void AppendSmartSwitchSection(StringBuilder sb)
        {
            var ss = SmartSwitchService.GetSettings();
            sb.AppendLine("[SmartSwitch]");
            sb.AppendLine("; [TR] Akıllı GPU Otomatik Güç Anahtarı: 1 = Etkin, 0 = Devre Dışı");
            sb.AppendLine("; [EN] Smart GPU Auto Power Switch: 1 = Enabled, 0 = Disabled");
            sb.AppendLine("; Varsayılan / Default: 0");
            sb.AppendLine($"Enabled={(ss.IsEnabled ? "1" : "0")}");
            sb.AppendLine();

            sb.AppendLine("; [TR] 1. Kademe: Düşük Kullanım Güç Planı GUID'i (0-%10 arası boşta/ofis modu)");
            sb.AppendLine("; [EN] Tier 1: Low / Idle Power Plan GUID (0-10% GPU load)");
            sb.AppendLine("; Varsayılan / Default: (boş / empty)");
            sb.AppendLine($"LowPlanGuid={ss.LowPlanGuid}");
            sb.AppendLine();

            sb.AppendLine("; [TR] 2. Kademe: Orta Kullanım Güç Planı GUID'i (%10-%30 arası video/tarayıcı modu)");
            sb.AppendLine("; [EN] Tier 2: Medium Power Plan GUID (10-30% GPU load)");
            sb.AppendLine("; Varsayılan / Default: (boş / empty)");
            sb.AppendLine($"MediumPlanGuid={ss.MediumPlanGuid}");
            sb.AppendLine();

            sb.AppendLine("; [TR] 3. Kademe: Yüksek/Oyun Güç Planı GUID'i (%30-%100 arası oyun/render modu)");
            sb.AppendLine("; [EN] Tier 3: High / Gaming Power Plan GUID (30-100% GPU load)");
            sb.AppendLine("; Varsayılan / Default: (boş / empty)");
            sb.AppendLine($"HighPlanGuid={ss.HighPlanGuid}");
            sb.AppendLine();

            sb.AppendLine("; [TR] Düşük-Orta Eşik Değeri (Yüzde olarak GPU kullanımı, örn: 10)");
            sb.AppendLine("; [EN] Low-Medium Threshold (GPU usage percentage, e.g. 10)");
            sb.AppendLine("; Varsayılan / Default: 10");
            sb.AppendLine($"LowThreshold={ss.LowThresholdPercent}");
            sb.AppendLine();

            sb.AppendLine("; [TR] Orta-Yüksek Eşik Değeri (Yüzde olarak GPU kullanımı, örn: 30)");
            sb.AppendLine("; [EN] Medium-High Threshold (GPU usage percentage, e.g. 30)");
            sb.AppendLine("; Varsayılan / Default: 30");
            sb.AppendLine($"HighThreshold={ss.HighThresholdPercent}");
            sb.AppendLine();

            sb.AppendLine("; [TR] Plan Değişim Tetikleme Süresi (Saniye): GPU eşiği aştığında bu kadar saniye sonra plana geç");
            sb.AppendLine("; [EN] Trigger Delay (Seconds): Switch to higher plan after load exceeds threshold for this duration");
            sb.AppendLine("; Varsayılan / Default: 3");
            sb.AppendLine($"TriggerSeconds={ss.TriggerSeconds}");
            sb.AppendLine();

            sb.AppendLine("; [TR] Normale Dönüş Süresi (Saniye): GPU kullanımı düştüğünde ani dalgalanmayı önlemek için bekleme süresi");
            sb.AppendLine("; [EN] Revert Delay (Seconds): Switch to lower plan after load drops below threshold for this duration");
            sb.AppendLine("; Varsayılan / Default: 8");
            sb.AppendLine($"RevertSeconds={ss.RevertSeconds}");
            sb.AppendLine();

            sb.AppendLine("; [TR] Bildirim Göster: 1 = Güç modu otomatik değiştiğinde sağ altta toast bildirim göster, 0 = Gösterme");
            sb.AppendLine("; [EN] Show Notification: 1 = Show toast notification on plan change, 0 = Silent");
            sb.AppendLine("; Varsayılan / Default: 1");
            sb.AppendLine($"ShowNotification={(ss.ShowNotification ? "1" : "0")}");
        }

        private static void SaveDefaultIni()
        {
            LanguageSetting = "Auto";
            CurrentLanguage = DetectSystemLanguage();
            CurrentTheme = "Auto";
            ShowSystemTray = false;
            AlwaysOnTop = false;
            HideWhenMinimized = true;
            CloseToTray = true;
            AutoStart = false;

            Save();
        }
    }
}
