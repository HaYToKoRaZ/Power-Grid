using System;
using System.Linq;
using System.Windows;
using PowerGridHaYTooL.Models;
using PowerGridHaYTooL.Services;

namespace PowerGridHaYTooL
{
    public partial class MainWindow
    {
        private void BtnAddAllCustom_Click(object sender, RoutedEventArgs e)
        {
            string promptText = ConfigService.CurrentLanguage == "English"
                ? "🚀 The following 5 custom power plan templates will be added to your system:\n\n" +
                  "  • ⚡ Power Grid Apex Performance (100% CPU, Active Cooling, Aggressive Boost, 0ms Latency)\n" +
                  "  • ⚖️ Power Grid Smart Balanced (5-100% Adaptive CPU, Active Cooling, Efficient Boost)\n" +
                  "  • 🌱 Power Grid Eco Stealth (5-75% Silent CPU, Passive Cooling, Boost Disabled)\n" +
                  "  • 🔴 AMD Ryzen™ Balanced (Optimized 90% Base Throttle)\n" +
                  "  • 🚀 AMD Ryzen™ High Performance (100% Full Clock)\n\n" +
                  "Do you want to add all these templates in a single click?"
                : "🚀 Aşağıdaki 5 özel şablon profilinin tümü tek tıkla sisteminize eklenecek:\n\n" +
                  "  • ⚡ Power Grid Apex Performance (%100 CPU, Aktif Soğutma, Agresif Boost, Sıfır Gecikme)\n" +
                  "  • ⚖️ Power Grid Smart Balanced (%5-100% Dinamik CPU, Aktif Soğutma, Verimli Boost)\n" +
                  "  • 🌱 Power Grid Eco Stealth (%5-75% Soğuk CPU, Pasif Sessiz Fan, Boost Kapalı)\n" +
                  "  • 🔴 AMD Ryzen™ Dengeli (%90 Taban İşlemci Durumu)\n" +
                  "  • 🚀 AMD Ryzen™ Yüksek Performans (%100 Tam Hız)\n\n" +
                  "Tüm bu profilleri tek seferde eklemek istiyor musunuz?";

            var res = MessageBox.Show(promptText, "Power Grid HaYTooL", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;

            int addedCount = 0;

            // 1. Power Grid Apex Performance
            if (!_plans.Any(p => string.Equals(p.Guid, PowerManagerService.GUID_HAYTOOL_APEX_PERF, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "Power Grid Apex Performance", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "HaYTooL Apex Performance", StringComparison.OrdinalIgnoreCase)))
            {
                if (PowerManagerService.AddHaYTooLApexPerformance("Power Grid Apex Performance", out _)) addedCount++;
            }

            // 2. Power Grid Smart Balanced
            if (!_plans.Any(p => string.Equals(p.Guid, PowerManagerService.GUID_HAYTOOL_SMART_BAL, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "Power Grid Smart Balanced", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "HaYTooL Smart Balanced", StringComparison.OrdinalIgnoreCase)))
            {
                if (PowerManagerService.AddHaYTooLSmartBalanced("Power Grid Smart Balanced", out _)) addedCount++;
            }

            // 3. Power Grid Eco Stealth
            if (!_plans.Any(p => string.Equals(p.Guid, PowerManagerService.GUID_HAYTOOL_ECO_STEALTH, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "Power Grid Eco Stealth", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "HaYTooL Eco Stealth", StringComparison.OrdinalIgnoreCase)))
            {
                if (PowerManagerService.AddHaYTooLEcoStealth("Power Grid Eco Stealth", out _)) addedCount++;
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
                ? "⚡ The following 3 custom Power Grid profiles will be added to your system:\n\n" +
                  "  • ⚡ Power Grid Apex Performance (%100 CPU, Active Cooling, Aggressive Boost, 0ms Latency)\n" +
                  "  • ⚖️ Power Grid Smart Balanced (%5-100% Dynamic CPU, Active Cooling, Efficient Boost)\n" +
                  "  • 🌱 Power Grid Eco Stealth (%5-75% Cold CPU, Passive Whisper Cooling, Boost Disabled)\n\n" +
                  "Do you want to add these 3 custom profiles in a single click?"
                : "⚡ Aşağıdaki 3 özel Power Grid güç profili tek tıkla sisteminize eklenecek:\n\n" +
                  "  • ⚡ Power Grid Apex Performance (%100 CPU, Aktif Soğutma, Agresif Boost, Sıfır Gecikme)\n" +
                  "  • ⚖️ Power Grid Smart Balanced (%5-100% Dinamik CPU, Aktif Soğutma, Verimli Boost)\n" +
                  "  • 🌱 Power Grid Eco Stealth (%5-75% Soğuk CPU, Pasif Sessiz Fan, Boost Kapalı)\n\n" +
                  "Bu 3 özel profili tek seferde eklemek istiyor musunuz?";

            var res = MessageBox.Show(promptText, "Power Grid HaYTooL", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;

            int addedCount = 0;

            if (!_plans.Any(p => string.Equals(p.Guid, PowerManagerService.GUID_HAYTOOL_APEX_PERF, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "Power Grid Apex Performance", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "HaYTooL Apex Performance", StringComparison.OrdinalIgnoreCase)))
            {
                if (PowerManagerService.AddHaYTooLApexPerformance("Power Grid Apex Performance", out _)) addedCount++;
            }

            if (!_plans.Any(p => string.Equals(p.Guid, PowerManagerService.GUID_HAYTOOL_SMART_BAL, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "Power Grid Smart Balanced", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "HaYTooL Smart Balanced", StringComparison.OrdinalIgnoreCase)))
            {
                if (PowerManagerService.AddHaYTooLSmartBalanced("Power Grid Smart Balanced", out _)) addedCount++;
            }

            if (!_plans.Any(p => string.Equals(p.Guid, PowerManagerService.GUID_HAYTOOL_ECO_STEALTH, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "Power Grid Eco Stealth", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(p.Name.Trim(), "HaYTooL Eco Stealth", StringComparison.OrdinalIgnoreCase)))
            {
                if (PowerManagerService.AddHaYTooLEcoStealth("Power Grid Eco Stealth", out _)) addedCount++;
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
                ? "⚡ [Power Grid Apex Performance]\n- Min/Max CPU: 100% (Instant Zero-Lag Frequency Response)\n- Active Fan Cooling (Spins Fans Before Thermal Throttling)\n- Aggressive Turbo Boost & All CPU Cores Awake/Unparked (100%)\n- EPP 0, PCIe ASPM Disabled (0), Zero-Latency USB & Wi-Fi"
                : "⚡ [Power Grid Apex Performance]\n- Min/Max İşlemci: %100 (Anında Sıfır Gecikmeli Frekans Yanıtı)\n- Aktif Fan Soğutma (Isınma Öncesi Erken Fan Devri)\n- Agresif Turbo Boost ve Tüm Çekirdekler Uyanık/Park Yok (%100)\n- EPP 0, PCIe ASPM Kapalı (0), Sıfır Gecikmeli USB ve Wi-Fi";

            PromptAndAddCustomPlan("Power Grid Apex Performance", PowerManagerService.GUID_HAYTOOL_APEX_PERF, details, PowerManagerService.AddHaYTooLApexPerformance);
        }

        private void BtnAddHaYTooLBalanced_Click(object sender, RoutedEventArgs e)
        {
            string details = ConfigService.CurrentLanguage == "English"
                ? "⚖️ [Power Grid Smart Balanced]\n- Min CPU 5%, Max CPU 100% (Dynamic Adaptive Clocking)\n- Active Cooling with Efficient Turbo Boost (Level 3)\n- Microsecond Dynamic Core Scaling (Min 25% Active, Max 100%)\n- EPP 35/50, PCIe ASPM Moderate, Balanced Wi-Fi"
                : "⚖️ [Power Grid Smart Balanced]\n- Min İşlemci %5, Max İşlemci %100 (Dinamik Adaptif Hız)\n- Aktif Soğutma ve Verimli Turbo Boost (Seviye 3)\n- İhtiyaca Göre Mikro-Saniye Dinamik Çekirdek Yönetimi\n- EPP 35/50, PCIe ASPM Dengeli, Verimli Wi-Fi";

            PromptAndAddCustomPlan("Power Grid Smart Balanced", PowerManagerService.GUID_HAYTOOL_SMART_BAL, details, PowerManagerService.AddHaYTooLSmartBalanced);
        }

        private void BtnAddHaYTooLEco_Click(object sender, RoutedEventArgs e)
        {
            string details = ConfigService.CurrentLanguage == "English"
                ? "🌱 [Power Grid Eco Stealth]\n- Min CPU 5%, Max CPU 75% (Eliminates Excessive Heat)\n- Passive Cooling Policy (Silent Whisper Fan Operation)\n- Turbo Boost Disabled (0) for Extended Battery Life & Cold CPU\n- Deep Core Parking, Max ASPM & Wi-Fi Energy Savings"
                : "🌱 [Power Grid Eco Stealth]\n- Min İşlemci %5, Max İşlemci %75 (Aşırı Isınmayı Keser)\n- Pasif Soğutma İlkesi (Tamamen Sessiz Fan Çalışması)\n- Turbo Boost Devre Dışı (Sıcaklık ve Pil Tasarrufu)\n- Derin Çekirdek Parkı, Maksimum ASPM ve Wi-Fi Tasarrufu";

            PromptAndAddCustomPlan("Power Grid Eco Stealth", PowerManagerService.GUID_HAYTOOL_ECO_STEALTH, details, PowerManagerService.AddHaYTooLEcoStealth);
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
    }
}
