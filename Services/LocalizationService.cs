using System;
using System.Collections.Generic;

namespace PowerGridHaYTooL.Services
{
    public static class LocalizationService
    {
        private static readonly Dictionary<string, string> TurkishStrings = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Title"] = "Power Grid HaYTooL",
            ["FileMenu"] = "Dosya",
            ["EditMenu"] = "Düzen",
            ["ToolsMenu"] = "Araçlar",
            ["OptionsMenu"] = "Seçenekler",
            ["HelpMenu"] = "Yardım",
            ["ApplySelected"] = "Seçileni Uygula",
            ["NewPlan"] = "Yeni Güç Düzeni Oluştur",
            ["DeleteSelected"] = "Listeden Seçileni Sil",
            ["ImportPlan"] = "Dosyadan Güç Düzeni Al...",
            ["ExportPlan"] = "Dışarıya Aktar...",
            ["Exit"] = "Kapat",
            ["Rename"] = "Yeniden Adlandır",
            ["CreateShortcut"] = "Masaüstüne Kısayol Oluştur",
            ["Copy"] = "Kopyala",
            ["CopyGuid"] = "GUID Kopyala",
            ["CopyName"] = "Plan İsmini Kopyala",
            ["PowerOptions"] = "Denetim Masası Güç Seçenekleri",
            ["MobilityCenter"] = "Windows Mobility Center",
            ["PowerSleepSettings"] = "Güç ve Uyku Ayarları",
            ["RestoreDefaults"] = "Güç Düzenlerini Sıfırla",
            ["DesktopContextMenu"] = "Masaüstü Sağ Tuş Menüsüne Ekle",
            ["ShowSystemTrayOpt"] = "Sistem Tepsisinde Göster (System Tray)",
            ["AlwaysOnTopOpt"] = "Pencere Her Zaman Üstte",
            ["HideWhenMinimizedOpt"] = "Küçültüldüğünde Pencereyi Gizle",
            ["AutoStartOpt"] = "Oturum Başlangıcında Çalıştır",
            ["LanguageOpt"] = "Dil (Language)",
            ["Website"] = "Web Sitesi",
            ["About"] = "Hakkında",
            ["ColPlans"] = "Güç Düzenleri",
            ["ColDesc"] = "Açıklaması / GUID",
            ["StatusReady"] = "Hazır. Toplam Güç Düzeni:",
            ["ActiveLabel"] = "[Aktif]",
            ["ActiveCannotDelete"] = "Aktif güç düzeni silinemez!",
            ["ConfirmDelete"] = "Seçilen güç düzenini silmek istediğinizden emin misiniz?",
            ["ConfirmRestore"] = "Tüm güç düzenlerini varsayılana sıfırlamak istediğinizden emin misiniz?",
            ["ShortcutCreated"] = "için masaüstüne tek tık kısayolu oluşturuldu!",
            ["SuccessExport"] = "Güç planı başarıyla dışarıya aktarıldı.",
            ["SuccessImport"] = "Güç planı başarıyla içe aktarıldı.",
            ["MenuAdded"] = "Masaüstü sağ tuş menüsüne eklendi!",
            ["MenuRemoved"] = "Masaüstü sağ tuş menüsünden kaldırıldı.",
            ["BackupAll"] = "Tüm Güç Düzenlerini Yedekle (Toplu)...",
            ["RestoreAll"] = "Yedek Klasöründen Toplu İçe Aktar...",
            ["TemplatesMenu"] = "Şablonlar",
            ["UltimatePerf"] = "⚡ Nihai Performans (Ultimate Performance) Ekle",
            ["AmdBalanced"] = "🔴 AMD Ryzen™ Dengeli Ekle",
            ["AmdHighPerf"] = "🚀 AMD Ryzen™ Yüksek Performans Ekle",
            ["SuccessBackupAll"] = "adet güç düzeni seçilen klasöre başarıyla yedeklendi!",
            ["SuccessRestoreAll"] = "adet güç düzeni klasörden sisteme başarıyla geri yüklendi!",
            ["TemplateAdded"] = "şablonu sisteme başarıyla eklendi!",
            ["PlanAlreadyExists"] = "güç düzeni sistemde zaten mevcut!",
            ["EditSettings"] = "Plan Ayarlarını Düzenle...",
            ["ComparePlans"] = "Güç Düzenlerini Karşılaştır...",
            ["SmartAutoSwitch"] = "🎮 Akıllı GPU ve Oyun Otomatik Güç Anahtarı...",
            ["UnlockHidden"] = "Windows Gizli Güç Ayarlarını Aç (Tümünü Göster)"
        };

        private static readonly Dictionary<string, string> EnglishStrings = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Title"] = "Power Grid HaYTooL",
            ["FileMenu"] = "File",
            ["EditMenu"] = "Edit",
            ["ToolsMenu"] = "Tools",
            ["OptionsMenu"] = "Options",
            ["HelpMenu"] = "Help",
            ["ApplySelected"] = "Apply Selected",
            ["NewPlan"] = "Create New Power Plan",
            ["DeleteSelected"] = "Delete Selected",
            ["ImportPlan"] = "Import Power Plan...",
            ["ExportPlan"] = "Export Power Plan...",
            ["Exit"] = "Exit",
            ["Rename"] = "Rename",
            ["CreateShortcut"] = "Create Desktop Shortcut",
            ["Copy"] = "Copy",
            ["CopyGuid"] = "Copy GUID",
            ["CopyName"] = "Copy Name",
            ["PowerOptions"] = "Control Panel Power Options",
            ["MobilityCenter"] = "Windows Mobility Center",
            ["PowerSleepSettings"] = "Power & Sleep Settings",
            ["RestoreDefaults"] = "Restore Default Power Plans",
            ["DesktopContextMenu"] = "Add to Desktop Context Menu",
            ["ShowSystemTrayOpt"] = "Show in System Tray",
            ["AlwaysOnTopOpt"] = "Always On Top",
            ["HideWhenMinimizedOpt"] = "Hide When Minimized",
            ["AutoStartOpt"] = "Start with Windows",
            ["LanguageOpt"] = "Language (Dil)",
            ["Website"] = "Website",
            ["About"] = "About",
            ["ColPlans"] = "Power Plans",
            ["ColDesc"] = "Description / GUID",
            ["StatusReady"] = "Ready. Total Power Plans:",
            ["ActiveLabel"] = "[Active]",
            ["ActiveCannotDelete"] = "Active power plan cannot be deleted!",
            ["ConfirmDelete"] = "Are you sure you want to delete the selected power plan?",
            ["ConfirmRestore"] = "Are you sure you want to restore default power plans?",
            ["ShortcutCreated"] = "desktop shortcut created successfully!",
            ["SuccessExport"] = "Power plan exported successfully.",
            ["SuccessImport"] = "Power plan imported successfully.",
            ["MenuAdded"] = "Added to desktop context menu!",
            ["MenuRemoved"] = "Removed from desktop context menu.",
            ["BackupAll"] = "Backup All Power Plans (Batch)...",
            ["RestoreAll"] = "Batch Import from Backup Folder...",
            ["TemplatesMenu"] = "Templates",
            ["UltimatePerf"] = "⚡ Add Ultimate Performance",
            ["AmdBalanced"] = "🔴 Add AMD Ryzen™ Balanced",
            ["AmdHighPerf"] = "🚀 Add AMD Ryzen™ High Performance",
            ["SuccessBackupAll"] = "power plans exported successfully to folder!",
            ["SuccessRestoreAll"] = "power plans imported successfully from folder!",
            ["TemplateAdded"] = "template added to the system successfully!",
            ["PlanAlreadyExists"] = "power plan already exists in the system!",
            ["EditSettings"] = "Edit Plan Settings...",
            ["ComparePlans"] = "Compare Power Plans...",
            ["SmartAutoSwitch"] = "🎮 Smart Game & GPU Auto-Switch...",
            ["UnlockHidden"] = "Unlock Windows Hidden Power Settings"
        };

        public static string GetString(string key, string fallback = "")
        {
            var dict = ConfigService.CurrentLanguage == "English" ? EnglishStrings : TurkishStrings;
            if (dict.TryGetValue(key, out var val))
            {
                return val;
            }

            if (TurkishStrings.TryGetValue(key, out var trVal))
            {
                return trVal;
            }

            return fallback;
        }
    }
}
