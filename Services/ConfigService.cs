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
            if (Sections.TryGetValue(CurrentLanguage, out var langSec) && langSec.TryGetValue(key, out var val))
            {
                return val;
            }
            if (Sections.TryGetValue("Turkish", out var trSec) && trSec.TryGetValue(key, out var trVal))
            {
                return trVal;
            }
            return fallback;
        }

        public static void Save()
        {
            try
            {
                if (!Sections.ContainsKey("Main"))
                    Sections["Main"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                Sections["Main"]["Language"] = LanguageSetting;
                Sections["Main"]["Theme"] = CurrentTheme;
                Sections["Main"]["ShowSystemTray"] = ShowSystemTray ? "1" : "0";
                Sections["Main"]["AlwaysOnTop"] = AlwaysOnTop ? "1" : "0";
                Sections["Main"]["HideWhenMinimized"] = HideWhenMinimized ? "1" : "0";
                Sections["Main"]["CloseToTray"] = CloseToTray ? "1" : "0";
                Sections["Main"]["AutoStart"] = AutoStart ? "1" : "0";
                Sections["Main"]["UserStartupPlanGuid"] = UserStartupPlanGuid;
                Sections["Main"]["UserStartupPlanName"] = UserStartupPlanName;

                var sb = new StringBuilder();
                sb.AppendLine("; ==============================================================================");
                sb.AppendLine("; Power Grid HaYTooL - Yapılandırma ve Dil Ayarları");
                sb.AppendLine("; ==============================================================================");
                sb.AppendLine();

                foreach (var sec in Sections)
                {
                    sb.AppendLine($"[{sec.Key}]");
                    foreach (var kvp in sec.Value)
                    {
                        sb.AppendLine($"{kvp.Key}={kvp.Value}");
                    }
                    sb.AppendLine();
                }

                File.WriteAllText(IniPath, sb.ToString(), Encoding.UTF8);
            }
            catch { }
        }

        private static void SaveDefaultIni()
        {
            LanguageSetting = "Auto";
            CurrentLanguage = DetectSystemLanguage();
            CurrentTheme = "Auto";
            ShowSystemTray = false;

            const string defaultTemplate = @"; ==============================================================================
; Power Grid HaYTooL - Yapılandırma ve Dil Ayarları
; ==============================================================================

[Main]
Language=Auto
Theme=Auto
ShowSystemTray=0
AlwaysOnTop=0
HideWhenMinimized=1
AutoStart=0

[Turkish]
Title=Power Grid HaYTooL
FileMenu=Dosya
EditMenu=Düzen
ToolsMenu=Araçlar
OptionsMenu=Seçenekler
HelpMenu=Yardım
ApplySelected=Seçileni Uygula
NewPlan=Yeni Güç Düzeni Oluştur
DeleteSelected=Listeden Seçileni Sil
ImportPlan=Dosyadan Güç Düzeni Al...
ExportPlan=Dışarıya Aktar...
Exit=Kapat
Rename=Yeniden Adlandır
CreateShortcut=Masaüstüne Kısayol Oluştur
Copy=Kopyala
CopyGuid=GUID Kopyala
CopyName=Plan İsmini Kopyala
PowerOptions=Denetim Masası Güç Seçenekleri
MobilityCenter=Windows Mobility Center
PowerSleepSettings=Güç ve Uyku Ayarları
RestoreDefaults=Güç Düzenlerini Sıfırla
DesktopContextMenu=Masaüstü Sağ Tuş Menüsüne Ekle
ShowSystemTrayOpt=Sistem Tepsisinde Göster (System Tray)
AlwaysOnTopOpt=Pencere Her Zaman Üstte
HideWhenMinimizedOpt=Küçültüldüğünde Pencereyi Gizle
AutoStartOpt=Oturum Başlangıcında Çalıştır
LanguageOpt=Dil (Language)
Website=Web Sitesi
About=Hakkında
ColPlans=Güç Düzenleri
ColDesc=Açıklaması / GUID
StatusReady=Hazır. Toplam Güç Düzeni:
ActiveLabel=[Aktif]
ActiveCannotDelete=Aktif güç düzeni silinemez!
ConfirmDelete=Seçilen güç düzenini silmek istediğinizden emin misiniz?
ConfirmRestore=Tüm güç düzenlerini varsayılana sıfırlamak istediğinizden emin misiniz?
ShortcutCreated=için masaüstüne tek tık kısayolu oluşturuldu!
SuccessExport=Güç planı başarıyla dışarıya aktarıldı.
SuccessImport=Güç planı başarıyla içe aktarıldı.
MenuAdded=Masaüstü sağ tuş menüsüne eklendi!
MenuRemoved=Masaüstü sağ tuş menüsünden kaldırıldı.

[English]
Title=Power Grid HaYTooL
FileMenu=File
EditMenu=Edit
ToolsMenu=Tools
OptionsMenu=Options
HelpMenu=Help
ApplySelected=Apply Selected
NewPlan=Create New Power Plan
DeleteSelected=Delete Selected
ImportPlan=Import Power Plan...
ExportPlan=Export Power Plan...
Exit=Exit
Rename=Rename
CreateShortcut=Create Desktop Shortcut
Copy=Copy
CopyGuid=Copy GUID
CopyName=Copy Name
PowerOptions=Control Panel Power Options
MobilityCenter=Windows Mobility Center
PowerSleepSettings=Power & Sleep Settings
RestoreDefaults=Restore Default Power Plans
DesktopContextMenu=Add to Desktop Context Menu
ShowSystemTrayOpt=Show in System Tray
AlwaysOnTopOpt=Always On Top
HideWhenMinimizedOpt=Hide When Minimized
AutoStartOpt=Start with Windows
LanguageOpt=Language (Dil)
Website=Website
About=About
ColPlans=Power Plans
ColDesc=Description / GUID
StatusReady=Ready. Total Power Plans:
ActiveLabel=[Active]
ActiveCannotDelete=Active power plan cannot be deleted!
ConfirmDelete=Are you sure you want to delete the selected power plan?
ConfirmRestore=Are you sure you want to restore default power plans?
ShortcutCreated=desktop shortcut created successfully!
SuccessExport=Power plan exported successfully.
SuccessImport=Power plan imported successfully.
MenuAdded=Added to desktop context menu!
MenuRemoved=Removed from desktop context menu.
";
            try
            {
                File.WriteAllText(IniPath, defaultTemplate, Encoding.UTF8);
            }
            catch { }
        }
    }
}
