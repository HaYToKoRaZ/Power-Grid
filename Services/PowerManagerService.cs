using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using PowerGridHaYTooL.Helpers;
using PowerGridHaYTooL.Models;

namespace PowerGridHaYTooL.Services
{
    public static class PowerManagerService
    {
        public static List<PowerPlanItem> GetPowerPlans()
        {
            var list = new List<PowerPlanItem>();
            var (_, output) = ProcessHelper.Execute("powercfg", "/list");

            if (string.IsNullOrWhiteSpace(output)) return list;

            var regex = new Regex(@"GUID:\s*([a-fA-F0-9\-]+)\s*\((.*?)\)(\s*\*?)");
            var matches = regex.Matches(output);

            foreach (Match m in matches)
            {
                string guid = m.Groups[1].Value.Trim();
                string name = m.Groups[2].Value.Trim();
                bool isActive = m.Groups[3].Value.Contains("*");

                list.Add(new PowerPlanItem
                {
                    Guid = guid,
                    Name = name,
                    IsActive = isActive
                });
            }

            return list;
        }

        public static PowerPlanItem? GetActivePlan()
        {
            return GetPowerPlans().FirstOrDefault(p => p.IsActive);
        }

        public static bool SetActivePlan(string guid, bool playSound = true, bool showToast = true)
        {
            var (exitCode, _) = ProcessHelper.Execute("powercfg", $"/setactive {guid}");
            if (exitCode == 0)
            {
                if (playSound)
                {
                    SoundHelper.PlayPlanSwitchSound();
                }

                if (showToast)
                {
                    try
                    {
                        var plan = GetPowerPlans().FirstOrDefault(p => p.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase));
                        string planName = plan?.Name ?? guid;
                        bool isTr = ConfigService.CurrentLanguage != "English";
                        string title = isTr ? "⚡ Güç Planı Değiştirildi" : "⚡ Power Plan Activated";
                        string reason = isTr ? "Aktif Güç Düzeni" : "Active Power Scheme";

                        string icon = "⚡";
                        if (planName.Contains("Yüksek", StringComparison.OrdinalIgnoreCase) ||
                            planName.Contains("Nihai", StringComparison.OrdinalIgnoreCase) ||
                            planName.Contains("High", StringComparison.OrdinalIgnoreCase) ||
                            planName.Contains("Ultimate", StringComparison.OrdinalIgnoreCase))
                        {
                            icon = "🚀";
                        }
                        else if (planName.Contains("Tasarruf", StringComparison.OrdinalIgnoreCase) ||
                                 planName.Contains("Eco", StringComparison.OrdinalIgnoreCase) ||
                                 planName.Contains("Saver", StringComparison.OrdinalIgnoreCase))
                        {
                            icon = "🌱";
                        }

                        UI.ToastNotificationWindow.ShowToast(title, planName, reason, icon);
                    }
                    catch { }
                }

                return true;
            }
            return false;
        }

        public static bool DeletePlan(string guid)
        {
            var (exitCode, _) = ProcessHelper.Execute("powercfg", $"/delete {guid}");
            return exitCode == 0;
        }

        public static bool DuplicatePlan(string guid, string newName = "Yeni Güç Düzeni")
        {
            var (exitCode, output) = ProcessHelper.Execute("powercfg", $"/duplicatescheme {guid}");
            if (exitCode != 0) return false;

            var match = Regex.Match(output, @"GUID:\s*([a-fA-F0-9\-]+)");
            if (match.Success)
            {
                string newGuid = match.Groups[1].Value;
                RenamePlan(newGuid, newName);
                return true;
            }
            return false;
        }

        public static bool RenamePlan(string guid, string newName)
        {
            var (exitCode, _) = ProcessHelper.Execute("powercfg", $"/changename {guid} \"{newName}\"");
            return exitCode == 0;
        }

        public static bool RestoreDefaults()
        {
            var (exitCode, _) = ProcessHelper.Execute("powercfg", "/restoredefaultschemes");
            return exitCode == 0;
        }

        public static bool ExportPlan(string guid, string filePath)
        {
            var (exitCode, _) = ProcessHelper.Execute("powercfg", $"/export \"{filePath}\" {guid}");
            return exitCode == 0;
        }

        public static bool ImportPlan(string filePath)
        {
            var (exitCode, _) = ProcessHelper.Execute("powercfg", $"/import \"{filePath}\"");
            return exitCode == 0;
        }

        public static (int SuccessCount, int FailCount) BackupAllPlans(string targetFolder, List<PowerPlanItem> plans)
        {
            int success = 0;
            int fail = 0;
            char[] invalid = System.IO.Path.GetInvalidFileNameChars();

            var sbInfo = new System.Text.StringBuilder();
            sbInfo.AppendLine("Power Grid HaYTooL - Güç Düzenleri Toplu Yedek Raporu");
            sbInfo.AppendLine($"Tarih: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sbInfo.AppendLine($"Toplam Plan Sayısı: {plans.Count}");
            sbInfo.AppendLine(new string('-', 55));

            foreach (var plan in plans)
            {
                string cleanName = string.Concat(plan.Name.Select(c => invalid.Contains(c) ? '_' : c)).Trim();
                if (string.IsNullOrWhiteSpace(cleanName)) cleanName = plan.Guid;
                string fileName = $"{cleanName}.pow";
                string filePath = System.IO.Path.Combine(targetFolder, fileName);

                if (ExportPlan(plan.Guid, filePath))
                {
                    success++;
                    sbInfo.AppendLine($"[✓] {plan.Name} (GUID: {plan.Guid}) -> {fileName}");
                }
                else
                {
                    fail++;
                    sbInfo.AppendLine($"[X] {plan.Name} (GUID: {plan.Guid}) -> Dışa aktarılamadı");
                }
            }

            try
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(targetFolder, "PowerPlans_Backup_Info.txt"), sbInfo.ToString(), System.Text.Encoding.UTF8);
            }
            catch { }

            return (success, fail);
        }

        public static (int SuccessCount, int FailCount) RestoreAllPlansFromFolder(string folderPath)
        {
            int success = 0;
            int fail = 0;

            if (!System.IO.Directory.Exists(folderPath)) return (0, 0);

            var powFiles = System.IO.Directory.GetFiles(folderPath, "*.pow");
            foreach (var file in powFiles)
            {
                if (ImportPlan(file))
                {
                    success++;
                }
                else
                {
                    fail++;
                }
            }

            return (success, fail);
        }

        // Sabit, kalıcı şablon GUID'leri (Güç düzenleri sıfırlansa veya yeniden eklense dahi her zaman aynı kalır)
        public const string GUID_ULTIMATE_PERF = "e9a42b02-d5df-448d-aa00-03f14749eb00";
        public const string GUID_AMD_BALANCED  = "9897998c-92de-4669-853f-b7cd3ecb2790";
        public const string GUID_AMD_HIGH_PERF = "9897998c-92de-4669-853f-b7cd3ecb2791";

        // Bize Özel Optimize Edilmiş HaYTooL Güç Profilleri GUID'leri
        public const string GUID_HAYTOOL_APEX_PERF   = "7a1f0001-c001-4a11-b001-000000000001";
        public const string GUID_HAYTOOL_SMART_BAL   = "7a1f0002-c002-4a11-b002-000000000002";
        public const string GUID_HAYTOOL_ECO_STEALTH = "7a1f0003-c003-4a11-b003-000000000003";

        public static bool AddHaYTooLApexPerformance(string name, out string createdGuid)
        {
            createdGuid = GUID_HAYTOOL_APEX_PERF;
            string baseGuid = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c"; // High Performance
            var (exitCode, output) = ProcessHelper.Execute("powercfg", $"/duplicatescheme {baseGuid} {GUID_HAYTOOL_APEX_PERF}");
            if (exitCode != 0)
            {
                (exitCode, output) = ProcessHelper.Execute("powercfg", $"/duplicatescheme {baseGuid}");
            }
            if (exitCode != 0) return false;

            var match = Regex.Match(output, @"GUID:\s*([a-fA-F0-9\-]+)");
            if (match.Success) createdGuid = match.Groups[1].Value;

            RenamePlan(createdGuid, name);

            // Apex Performans: %100 CPU min/max, Aktif Soğutma, Agresif Boost, 0 EPP, Park Yok, 0 ASPM/USB/WiFi Gecikmesi
            var s = GetPlanSettings(createdGuid, name);
            s.Guid = createdGuid;
            s.Name = name;
            s.CpuMinAc = 100;
            s.CpuMinDc = 100;
            s.CpuMaxAc = 100;
            s.CpuMaxDc = 100;
            s.CoolingPolicyAc = 1; // Aktif fan
            s.CoolingPolicyDc = 1;
            s.BoostModeAc = 2; // Agresif Turbo Boost
            s.BoostModeDc = 2;
            s.CoreParkingMinAc = 100; // Tüm çekirdekler daima uyanık
            s.CoreParkingMinDc = 100;
            s.CoreParkingMaxAc = 100;
            s.CoreParkingMaxDc = 100;
            s.EnergyPerfPreferenceAc = 0; // Maksimum Donanım Performansı (EPP 0)
            s.EnergyPerfPreferenceDc = 0;
            s.PcieAspmAc = 0; // Kapalı (0 gecikme, tam veri yolu hızı)
            s.PcieAspmDc = 0;
            s.UsbSelectiveSuspendAc = 0; // Devre dışı (Portlar uyumaz)
            s.UsbSelectiveSuspendDc = 0;
            s.WifiPowerSavingAc = 0; // Maksimum Ağ Performansı & Düşük Ping
            s.WifiPowerSavingDc = 0;
            s.DiskTimeoutAc = 0; // Asla kapanmaz
            s.DiskTimeoutDc = 0;
            s.VideoTimeoutAc = 30;
            s.VideoTimeoutDc = 15;
            s.SleepTimeoutAc = 0; // Asla uyumaz
            s.SleepTimeoutDc = 60;
            s.AllowWakeTimersAc = 1;
            s.AllowWakeTimersDc = 1;

            SavePlanSettings(s);
            return true;
        }

        public static bool AddHaYTooLSmartBalanced(string name, out string createdGuid)
        {
            createdGuid = GUID_HAYTOOL_SMART_BAL;
            string baseGuid = "381b4222-f694-41f0-9685-ff5bb260df2e"; // Balanced
            var (exitCode, output) = ProcessHelper.Execute("powercfg", $"/duplicatescheme {baseGuid} {GUID_HAYTOOL_SMART_BAL}");
            if (exitCode != 0)
            {
                (exitCode, output) = ProcessHelper.Execute("powercfg", $"/duplicatescheme {baseGuid}");
            }
            if (exitCode != 0) return false;

            var match = Regex.Match(output, @"GUID:\s*([a-fA-F0-9\-]+)");
            if (match.Success) createdGuid = match.Groups[1].Value;

            RenamePlan(createdGuid, name);

            // Akıllı Dengeli: %5 min, %100 max CPU, Aktif Soğutma, Verimli Boost (3), Dengeli EPP, Dengeli ASPM/WiFi
            var s = GetPlanSettings(createdGuid, name);
            s.Guid = createdGuid;
            s.Name = name;
            s.CpuMinAc = 5;
            s.CpuMinDc = 5;
            s.CpuMaxAc = 100;
            s.CpuMaxDc = 100;
            s.CoolingPolicyAc = 1; // Aktif fan soğutma
            s.CoolingPolicyDc = 1;
            s.BoostModeAc = 3; // Verimli Etkin (Efficient Enabled)
            s.BoostModeDc = 3;
            s.CoreParkingMinAc = 25; // İhtiyaca göre mikro-saniyede dinamik açılır
            s.CoreParkingMinDc = 10;
            s.CoreParkingMaxAc = 100;
            s.CoreParkingMaxDc = 100;
            s.EnergyPerfPreferenceAc = 35; // Tepkisel dengeli EPP
            s.EnergyPerfPreferenceDc = 50;
            s.PcieAspmAc = 1; // Orta düzey güç tasarrufu
            s.PcieAspmDc = 1;
            s.UsbSelectiveSuspendAc = 1;
            s.UsbSelectiveSuspendDc = 1;
            s.WifiPowerSavingAc = 0; // AC'de maksimum hız
            s.WifiPowerSavingDc = 1; // Pilde hafif tasarruf
            s.DiskTimeoutAc = 20;
            s.DiskTimeoutDc = 10;
            s.VideoTimeoutAc = 15;
            s.VideoTimeoutDc = 10;
            s.SleepTimeoutAc = 30;
            s.SleepTimeoutDc = 20;
            s.AllowWakeTimersAc = 1;
            s.AllowWakeTimersDc = 1;

            SavePlanSettings(s);
            return true;
        }

        public static bool AddHaYTooLEcoStealth(string name, out string createdGuid)
        {
            createdGuid = GUID_HAYTOOL_ECO_STEALTH;
            string baseGuid = "a1841308-3541-4fab-bc81-f71556f20b4a"; // Power Saver
            var (exitCode, output) = ProcessHelper.Execute("powercfg", $"/duplicatescheme {baseGuid} {GUID_HAYTOOL_ECO_STEALTH}");
            if (exitCode != 0)
            {
                (exitCode, output) = ProcessHelper.Execute("powercfg", $"/duplicatescheme {baseGuid}");
            }
            if (exitCode != 0) return false;

            var match = Regex.Match(output, @"GUID:\s*([a-fA-F0-9\-]+)");
            if (match.Success) createdGuid = match.Groups[1].Value;

            RenamePlan(createdGuid, name);

            // Eko Sessiz / Stealth: %5 min, %75 max CPU (soğuk çalışma), Pasif Soğutma (sessiz fan), Boost Kapalı (0), EPP 100, Maks Tasarruf
            var s = GetPlanSettings(createdGuid, name);
            s.Guid = createdGuid;
            s.Name = name;
            s.CpuMinAc = 5;
            s.CpuMinDc = 5;
            s.CpuMaxAc = 75; // Tatlı verimlilik noktası (ısınmayı önler)
            s.CpuMaxDc = 65;
            s.CoolingPolicyAc = 0; // Pasif / Edilgin Soğutma (Fanlar susar, sessiz çalışır)
            s.CoolingPolicyDc = 0;
            s.BoostModeAc = 0; // Turbo Boost Devre Dışı (Sıcaklık ve pil tüketimini keser)
            s.BoostModeDc = 0;
            s.CoreParkingMinAc = 10;
            s.CoreParkingMinDc = 5;
            s.CoreParkingMaxAc = 75;
            s.CoreParkingMaxDc = 50;
            s.EnergyPerfPreferenceAc = 85;
            s.EnergyPerfPreferenceDc = 100; // Maksimum Enerji Tasarrufu (EPP 100)
            s.PcieAspmAc = 2; // Maksimum Güç Tasarrufu
            s.PcieAspmDc = 2;
            s.UsbSelectiveSuspendAc = 1;
            s.UsbSelectiveSuspendDc = 1;
            s.WifiPowerSavingAc = 2;
            s.WifiPowerSavingDc = 3; // Maksimum Wi-Fi Güç Tasarrufu
            s.DiskTimeoutAc = 5;
            s.DiskTimeoutDc = 3;
            s.VideoTimeoutAc = 5;
            s.VideoTimeoutDc = 3;
            s.SleepTimeoutAc = 15;
            s.SleepTimeoutDc = 10;
            s.AllowWakeTimersAc = 0; // Çantada/gece istenmeyen uyanmaları önler
            s.AllowWakeTimersDc = 0;

            SavePlanSettings(s);
            return true;
        }

        public static bool AddUltimatePerformance(string name = "Nihai Performans")
        {
            var (exitCode, output) = ProcessHelper.Execute("powercfg", $"/duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61 {GUID_ULTIMATE_PERF}");
            if (exitCode != 0)
            {
                (exitCode, output) = ProcessHelper.Execute("powercfg", "/duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61");
            }
            if (exitCode != 0) return false;

            var match = Regex.Match(output, @"GUID:\s*([a-fA-F0-9\-]+)");
            string targetGuid = match.Success ? match.Groups[1].Value : GUID_ULTIMATE_PERF;
            RenamePlan(targetGuid, name);
            return true;
        }

        public static bool AddAmdRyzenBalanced(string name = "AMD Ryzen™ Dengeli")
        {
            var (exitCode, output) = ProcessHelper.Execute("powercfg", $"/duplicatescheme 381b4222-f694-41f0-9685-ff5bb260df2e {GUID_AMD_BALANCED}");
            if (exitCode != 0)
            {
                (exitCode, output) = ProcessHelper.Execute("powercfg", "/duplicatescheme 381b4222-f694-41f0-9685-ff5bb260df2e");
            }
            if (exitCode != 0) return false;

            var match = Regex.Match(output, @"GUID:\s*([a-fA-F0-9\-]+)");
            string targetGuid = match.Success ? match.Groups[1].Value : GUID_AMD_BALANCED;
            RenamePlan(targetGuid, name);

            // AMD Resmi Yonga Seti (Chipset Driver) Spesifikasyonları:
            // 1. Min CPU: %90 (Zen mimarisi saat geçiş gecikmesini sıfırlar)
            // 2. Max CPU: %100 (Tam saat hızı tavanı)
            // 3. Soğutma: Aktif (Fanlar erken devreye girer, Precision Boost tavanını korur)
            // 4. Turbo Boost: Agresif (Seviye 2)
            // 5. Çekirdek Parkı: %100 (AMD CCX/CCD geçiş gecikmesini önlemek için park yok)
            // 6. EPP: 25 / 50 (Otonom donanım kontrollü CPPC2 geçişi)
            // 7. PCIe ASPM: 1 (Orta düzey dengeli)
            // 8. USB Askıya Alma: 0 (AMD USB kopmalarını önler)
            // 9. Wi-Fi: 0 (Maksimum performans)
            var s = GetPlanSettings(targetGuid, name);
            s.Guid = targetGuid;
            s.Name = name;
            s.CpuMinAc = 90;
            s.CpuMinDc = 90;
            s.CpuMaxAc = 100;
            s.CpuMaxDc = 100;
            s.CoolingPolicyAc = 1;
            s.CoolingPolicyDc = 1;
            s.BoostModeAc = 2;
            s.BoostModeDc = 2;
            s.CoreParkingMinAc = 100;
            s.CoreParkingMinDc = 90;
            s.CoreParkingMaxAc = 100;
            s.CoreParkingMaxDc = 100;
            s.EnergyPerfPreferenceAc = 25;
            s.EnergyPerfPreferenceDc = 50;
            s.PcieAspmAc = 1;
            s.PcieAspmDc = 1;
            s.UsbSelectiveSuspendAc = 0;
            s.UsbSelectiveSuspendDc = 1;
            s.WifiPowerSavingAc = 0;
            s.WifiPowerSavingDc = 1;

            SavePlanSettings(s);
            return true;
        }

        public static bool AddAmdRyzenHighPerformance(string name = "AMD Ryzen™ Yüksek Performans")
        {
            var (exitCode, output) = ProcessHelper.Execute("powercfg", $"/duplicatescheme 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c {GUID_AMD_HIGH_PERF}");
            if (exitCode != 0)
            {
                (exitCode, output) = ProcessHelper.Execute("powercfg", "/duplicatescheme 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
            }
            if (exitCode != 0) return false;

            var match = Regex.Match(output, @"GUID:\s*([a-fA-F0-9\-]+)");
            string targetGuid = match.Success ? match.Groups[1].Value : GUID_AMD_HIGH_PERF;
            RenamePlan(targetGuid, name);

            // AMD Ryzen Yüksek Performans: %100 Min/Max CPU, Aktif Soğutma, Agresif Boost, Park Yok, 0 EPP, 0 Gecikme
            var s = GetPlanSettings(targetGuid, name);
            s.Guid = targetGuid;
            s.Name = name;
            s.CpuMinAc = 100;
            s.CpuMinDc = 100;
            s.CpuMaxAc = 100;
            s.CpuMaxDc = 100;
            s.CoolingPolicyAc = 1;
            s.CoolingPolicyDc = 1;
            s.BoostModeAc = 2;
            s.BoostModeDc = 2;
            s.CoreParkingMinAc = 100;
            s.CoreParkingMinDc = 100;
            s.CoreParkingMaxAc = 100;
            s.CoreParkingMaxDc = 100;
            s.EnergyPerfPreferenceAc = 0;
            s.EnergyPerfPreferenceDc = 0;
            s.PcieAspmAc = 0;
            s.PcieAspmDc = 0;
            s.UsbSelectiveSuspendAc = 0;
            s.UsbSelectiveSuspendDc = 0;
            s.WifiPowerSavingAc = 0;
            s.WifiPowerSavingDc = 0;
            s.DiskTimeoutAc = 0;
            s.DiskTimeoutDc = 0;

            SavePlanSettings(s);
            return true;
        }

        public static PowerPlanSettings GetPlanSettings(string guid, string name)
        {
            var settings = new PowerPlanSettings
            {
                Guid = guid,
                Name = name
            };

            var (_, output) = ProcessHelper.Execute("powercfg", $"/query {guid}");
            if (string.IsNullOrWhiteSpace(output)) return settings;

            var regex = new Regex(@"Power Setting GUID:\s*([a-fA-F0-9\-]+)[\s\S]*?Current AC Power Setting Index:\s*0x([0-9a-fA-F]+)[\s\S]*?Current DC Power Setting Index:\s*0x([0-9a-fA-F]+)", RegexOptions.IgnoreCase);
            var matches = regex.Matches(output);

            foreach (Match m in matches)
            {
                string settingGuid = m.Groups[1].Value.ToLowerInvariant();
                int acVal = Convert.ToInt32(m.Groups[2].Value, 16);
                int dcVal = Convert.ToInt32(m.Groups[3].Value, 16);

                switch (settingGuid)
                {
                    // VIDEOIDLE: Ekranı kapatmak için beklenecek süre (saniye -> dakika)
                    case "3c0bc021-c8a8-4e07-a973-6b14cbcb2b7e":
                        settings.VideoTimeoutAc = acVal / 60;
                        settings.VideoTimeoutDc = dcVal / 60;
                        break;

                    // STANDBYIDLE: Uykuya geçmek için beklenecek süre (saniye -> dakika)
                    case "29f6c1db-86da-48c5-9fdb-f2b67b1f44da":
                        settings.SleepTimeoutAc = acVal / 60;
                        settings.SleepTimeoutDc = dcVal / 60;
                        break;

                    // DISKIDLE: Sabit diski kapatmak için beklenecek süre (saniye -> dakika)
                    case "6738e2c4-e8a5-4a42-b16a-e040e769756e":
                    case "6738e2c4-e8a5-4424-a1ff-8097d77b2d08":
                        settings.DiskTimeoutAc = acVal / 60;
                        settings.DiskTimeoutDc = dcVal / 60;
                        break;

                    // PROCTHROTTLEMIN: En düşük işlemci durumu (%)
                    case "893dee8e-2bef-41e0-89c6-b55d0929964c":
                        settings.CpuMinAc = acVal;
                        settings.CpuMinDc = dcVal;
                        break;

                    // PROCTHROTTLEMAX: En yüksek işlemci durumu (%)
                    case "bc5038f7-23e0-4960-96da-33abaf5935ec":
                        settings.CpuMaxAc = acVal;
                        settings.CpuMaxDc = dcVal;
                        break;

                    // SYSCOOLPOL: Sistem soğutma ilkesi (1 = Etkin, 0 = Edilgin)
                    case "94d3a615-a899-4ac5-ae2b-e4d8f634367f":
                    case "94d3a615-a899-4ac5-ae2b-e4d8f6343b57":
                        settings.CoolingPolicyAc = acVal;
                        settings.CoolingPolicyDc = dcVal;
                        break;

                    // HIBERNATEIDLE: Hazırda bekletmeye geçiş süresi (saniye -> dakika)
                    case "9d7815a6-7ee4-497e-8888-515a05f02364":
                        settings.HibernateTimeoutAc = acVal / 60;
                        settings.HibernateTimeoutDc = dcVal / 60;
                        break;

                    // RTCWAKE: Uyandırma zamanlayıcılarına izin ver
                    case "bd3b718a-0680-4d9d-8ab2-e1d2b4ac806d":
                        settings.AllowWakeTimersAc = acVal;
                        settings.AllowWakeTimersDc = dcVal;
                        break;

                    // PERFBOOSTMODE: Turbo Boost Modu (0=Kapalı, 1=Etkin, 2=Agresif...)
                    case "be337238-0d82-4146-a960-4f3749d470c7":
                        settings.BoostModeAc = acVal;
                        settings.BoostModeDc = dcVal;
                        break;

                    // CPMINCORES: Çekirdek Park Etme Min Çekirdek (%)
                    case "0cc5b647-c1df-4637-891a-dec35c318583":
                    case "0cc5b647-c1df-4637-891a-dec35c318584":
                        settings.CoreParkingMinAc = acVal;
                        settings.CoreParkingMinDc = dcVal;
                        break;

                    // CPMAXCORES: Çekirdek Park Etme Max Çekirdek (%)
                    case "ea062031-0e34-4ff1-9b6d-eb1059334028":
                    case "ea062031-0e34-4ff1-9b6d-eb1059334029":
                    case "ea062031-0e34-4ff1-9b6d-eb10593ac08f":
                        settings.CoreParkingMaxAc = acVal;
                        settings.CoreParkingMaxDc = dcVal;
                        break;

                    // PERFEPP: İşlemci Enerji Performans Tercihi (0=Max Performans, 100=Tasarruf)
                    case "36687f9e-e3a5-4dbf-b1dc-15eb381c6863":
                    case "36687f9e-e376-4512-a778-dd514d66bb38":
                        settings.EnergyPerfPreferenceAc = acVal;
                        settings.EnergyPerfPreferenceDc = dcVal;
                        break;

                    // ASPM: PCI Express Bağlantı Durumu Güç Yönetimi
                    case "ee12f906-d277-404b-b6da-e5fa1a576df5":
                    case "ee12f906-d277-47ba-b601-1e445653d4b6":
                        settings.PcieAspmAc = acVal;
                        settings.PcieAspmDc = dcVal;
                        break;

                    // USBSELECT: USB Seçici Askıya Alma
                    case "48e6b7a6-50f5-4782-a5d4-53bb8f07e226":
                        settings.UsbSelectiveSuspendAc = acVal;
                        settings.UsbSelectiveSuspendDc = dcVal;
                        break;

                    // WIFIPOWER: Kablosuz Ağ Güç Tasarrufu
                    case "12bbebe6-58d6-4636-95bb-3217ef867c1a":
                    case "12bbebe6-58d6-4636-95bb-ebf6ef865cd6":
                        settings.WifiPowerSavingAc = acVal;
                        settings.WifiPowerSavingDc = dcVal;
                        break;

                    // PBUTTONACTION: Güç Düğmesi Eylemi
                    case "7648efa3-dd9c-4e3e-b566-50f929386280":
                    case "7648efa3-dd9c-4e3e-b56e-012a8441767a":
                        settings.PowerButtonActionAc = acVal;
                        settings.PowerButtonActionDc = dcVal;
                        break;

                    // SBUTTONACTION: Uyku Düğmesi Eylemi
                    case "96996bc0-ad50-47ec-923b-6f41874dd9eb":
                    case "96996bc8-4b0a-47d0-9e60-9d0b0d03d14f":
                        settings.SleepButtonActionAc = acVal;
                        settings.SleepButtonActionDc = dcVal;
                        break;
                }
            }

            // powercfg /query çıktısında yer almayan veya gizli durumdaki ayarları Registry'den doğrudan tamamla
            FillMissingFromRegistry(guid, settings);

            return settings;
        }

        private static void FillMissingFromRegistry(string planGuid, PowerPlanSettings s)
        {
            try
            {
                // DISK
                if (s.DiskTimeoutAc == 0 && TryReadReg(planGuid, "0012ee47-9041-4b5d-9b77-535fba8b1442", "6738e2c4-e8a5-4a42-b16a-e040e769756e", out int diskAc, out int diskDc))
                {
                    s.DiskTimeoutAc = diskAc / 60;
                    s.DiskTimeoutDc = diskDc / 60;
                }

                // SYSCOOLPOL
                if (TryReadReg(planGuid, "54533251-82be-4824-96c1-47b60b740d00", "94d3a615-a899-4ac5-ae2b-e4d8f634367f", out int coolAc, out int coolDc))
                {
                    s.CoolingPolicyAc = coolAc;
                    s.CoolingPolicyDc = coolDc;
                }

                // CPMAXCORES
                if (TryReadReg(planGuid, "54533251-82be-4824-96c1-47b60b740d00", "ea062031-0e34-4ff1-9b6d-eb1059334028", out int cpMaxAc, out int cpMaxDc))
                {
                    s.CoreParkingMaxAc = cpMaxAc;
                    s.CoreParkingMaxDc = cpMaxDc;
                }

                // PERFEPP
                if (TryReadReg(planGuid, "54533251-82be-4824-96c1-47b60b740d00", "36687f9e-e3a5-4dbf-b1dc-15eb381c6863", out int eppAc, out int eppDc))
                {
                    s.EnergyPerfPreferenceAc = eppAc;
                    s.EnergyPerfPreferenceDc = eppDc;
                }

                // ASPM
                if (TryReadReg(planGuid, "501a4d13-42af-4429-9fd1-a8218c268e20", "ee12f906-d277-404b-b6da-e5fa1a576df5", out int aspmAc, out int aspmDc))
                {
                    s.PcieAspmAc = aspmAc;
                    s.PcieAspmDc = aspmDc;
                }

                // WIFIPOWER
                if (TryReadReg(planGuid, "19cbb8fa-5279-450e-9fac-8a3d5fedd0c1", "12bbebe6-58d6-4636-95bb-3217ef867c1a", out int wifiAc, out int wifiDc))
                {
                    s.WifiPowerSavingAc = wifiAc;
                    s.WifiPowerSavingDc = wifiDc;
                }

                // PBUTTONACTION
                if (TryReadReg(planGuid, "4f971e89-eebd-4455-a8de-9e59040e7347", "7648efa3-dd9c-4e3e-b566-50f929386280", out int pbtnAc, out int pbtnDc))
                {
                    s.PowerButtonActionAc = pbtnAc;
                    s.PowerButtonActionDc = pbtnDc;
                }

                // SBUTTONACTION
                if (TryReadReg(planGuid, "4f971e89-eebd-4455-a8de-9e59040e7347", "96996bc0-ad50-47ec-923b-6f41874dd9eb", out int sbtnAc, out int sbtnDc))
                {
                    s.SleepButtonActionAc = sbtnAc;
                    s.SleepButtonActionDc = sbtnDc;
                }
            }
            catch { }
        }

        private static bool TryReadReg(string planGuid, string subGuid, string settingGuid, out int ac, out int dc)
        {
            ac = 0;
            dc = 0;
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{planGuid}\{subGuid}\{settingGuid}");
                if (key != null)
                {
                    var acObj = key.GetValue("ACSettingIndex");
                    var dcObj = key.GetValue("DCSettingIndex");
                    if (acObj is int a)
                    {
                        ac = a;
                        dc = dcObj is int d ? d : a;
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public static bool SavePlanSettings(PowerPlanSettings s)
        {
            try
            {
                // Alt Gruplar (Subgroups - Resmi Windows GUID'leri)
                const string SUB_VIDEO = "7516b95f-f776-4464-8c53-06167f40cc99";
                const string SUB_SLEEP = "238c9fa8-0aad-41ed-83f4-97be242c8f20";
                const string SUB_DISK = "0012ee47-9041-4b5d-9b77-535fba8b1442";
                const string SUB_PROCESSOR = "54533251-82be-4824-96c1-47b60b740d00";
                const string SUB_PCIEXPRESS = "501a4d13-42af-4429-9fd1-a8218c268e20";
                const string SUB_USB = "2a737441-1930-4402-8d77-b2bebba308a3";
                const string SUB_BUTTONS = "4f971e89-eebd-4455-a8de-9e59040e7347";
                const string SUB_WIFI = "19cbb8fa-5279-450e-9fac-8a3d5fedd0c1";

                // Ayar GUID'leri (Doğrulanmış Resmi Windows GUID'leri)
                const string VIDEOIDLE = "3c0bc021-c8a8-4e07-a973-6b14cbcb2b7e";
                const string STANDBYIDLE = "29f6c1db-86da-48c5-9fdb-f2b67b1f44da";
                const string HIBERNATEIDLE = "9d7815a6-7ee4-497e-8888-515a05f02364";
                const string RTCWAKE = "bd3b718a-0680-4d9d-8ab2-e1d2b4ac806d";
                const string DISKIDLE = "6738e2c4-e8a5-4a42-b16a-e040e769756e";
                const string PROCTHROTTLEMIN = "893dee8e-2bef-41e0-89c6-b55d0929964c";
                const string PROCTHROTTLEMAX = "bc5038f7-23e0-4960-96da-33abaf5935ec";
                const string SYSCOOLPOL = "94d3a615-a899-4ac5-ae2b-e4d8f634367f";
                const string PERFBOOSTMODE = "be337238-0d82-4146-a960-4f3749d470c7";
                const string CPMINCORES = "0cc5b647-c1df-4637-891a-dec35c318583";
                const string CPMAXCORES = "ea062031-0e34-4ff1-9b6d-eb1059334028";
                const string PERFEPP = "36687f9e-e3a5-4dbf-b1dc-15eb381c6863";
                const string ASPM = "ee12f906-d277-404b-b6da-e5fa1a576df5";
                const string USBSELECT = "48e6b7a6-50f5-4782-a5d4-53bb8f07e226";
                const string WIFIPOWER = "12bbebe6-58d6-4636-95bb-3217ef867c1a";
                const string PBUTTONACTION = "7648efa3-dd9c-4e3e-b566-50f929386280";
                const string SBUTTONACTION = "96996bc0-ad50-47ec-923b-6f41874dd9eb";

                // Video Timeout (saniye)
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_VIDEO} {VIDEOIDLE} {s.VideoTimeoutAc * 60}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_VIDEO} {VIDEOIDLE} {s.VideoTimeoutDc * 60}");

                // Sleep Timeout (saniye)
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_SLEEP} {STANDBYIDLE} {s.SleepTimeoutAc * 60}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_SLEEP} {STANDBYIDLE} {s.SleepTimeoutDc * 60}");

                // Hibernate Timeout (saniye)
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_SLEEP} {HIBERNATEIDLE} {s.HibernateTimeoutAc * 60}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_SLEEP} {HIBERNATEIDLE} {s.HibernateTimeoutDc * 60}");

                // Allow Wake Timers
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_SLEEP} {RTCWAKE} {s.AllowWakeTimersAc}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_SLEEP} {RTCWAKE} {s.AllowWakeTimersDc}");

                // Disk Timeout (saniye)
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_DISK} {DISKIDLE} {s.DiskTimeoutAc * 60}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_DISK} {DISKIDLE} {s.DiskTimeoutDc * 60}");

                // CPU Min (%)
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_PROCESSOR} {PROCTHROTTLEMIN} {s.CpuMinAc}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_PROCESSOR} {PROCTHROTTLEMIN} {s.CpuMinDc}");

                // CPU Max (%)
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_PROCESSOR} {PROCTHROTTLEMAX} {s.CpuMaxAc}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_PROCESSOR} {PROCTHROTTLEMAX} {s.CpuMaxDc}");

                // Cooling Policy (1 = Etkin, 0 = Edilgin)
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_PROCESSOR} {SYSCOOLPOL} {s.CoolingPolicyAc}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_PROCESSOR} {SYSCOOLPOL} {s.CoolingPolicyDc}");

                // Turbo Boost Mode (0..5)
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_PROCESSOR} {PERFBOOSTMODE} {s.BoostModeAc}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_PROCESSOR} {PERFBOOSTMODE} {s.BoostModeDc}");

                // Core Parking Min (%)
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_PROCESSOR} {CPMINCORES} {s.CoreParkingMinAc}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_PROCESSOR} {CPMINCORES} {s.CoreParkingMinDc}");

                // Core Parking Max (%)
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_PROCESSOR} {CPMAXCORES} {s.CoreParkingMaxAc}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_PROCESSOR} {CPMAXCORES} {s.CoreParkingMaxDc}");

                // Energy Performance Preference (0..100)
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_PROCESSOR} {PERFEPP} {s.EnergyPerfPreferenceAc}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_PROCESSOR} {PERFEPP} {s.EnergyPerfPreferenceDc}");

                // PCI Express ASPM (0..2)
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_PCIEXPRESS} {ASPM} {s.PcieAspmAc}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_PCIEXPRESS} {ASPM} {s.PcieAspmDc}");

                // USB Selective Suspend (0..1)
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_USB} {USBSELECT} {s.UsbSelectiveSuspendAc}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_USB} {USBSELECT} {s.UsbSelectiveSuspendDc}");

                // WiFi Power Saving (0..3)
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_WIFI} {WIFIPOWER} {s.WifiPowerSavingAc}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_WIFI} {WIFIPOWER} {s.WifiPowerSavingDc}");

                // Buttons
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_BUTTONS} {PBUTTONACTION} {s.PowerButtonActionAc}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_BUTTONS} {PBUTTONACTION} {s.PowerButtonActionDc}");
                ProcessHelper.Execute("powercfg", $"/setacvalueindex {s.Guid} {SUB_BUTTONS} {SBUTTONACTION} {s.SleepButtonActionAc}");
                ProcessHelper.Execute("powercfg", $"/setdcvalueindex {s.Guid} {SUB_BUTTONS} {SBUTTONACTION} {s.SleepButtonActionDc}");

                // Aktif olan plan ise ayarları anında uygula
                ProcessHelper.Execute("powercfg", $"/setactive {s.Guid}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Mevcut kullanıcının Yönetici (Administrator) yetkisine sahip olup olmadığını denetler.
        /// </summary>
        public static bool IsAdministrator()
        {
            try
            {
                using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Windows Denetim Masasında gelişmiş güç ayarlarının açık (unlocked / Attributes == 2) olup olmadığını kontrol eder.
        /// </summary>
        public static bool AreHiddenSettingsUnlocked()
        {
            try
            {
                // Turbo Boost ve Min İşlemci ayarlarının Attributes değerini kontrol et
                using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\be337238-0d82-4146-a960-4f3749d470c7");
                if (key != null)
                {
                    var val = key.GetValue("Attributes");
                    if (val is int intVal)
                    {
                        // Windows Kayıt Defterinde:
                        // 2 = Açık / Görünür (Unhidden in Control Panel)
                        // 1 veya 0 = Gizli / Kilitli (Hidden)
                        return intVal == 2;
                    }
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Windows Denetim Masasında varsayılan olarak gizli tutulan gelişmiş güç ayarlarının kilidini açar (Attributes=2) veya tekrar gizler/kapatır (Attributes=1).
        /// </summary>
        public static bool UnlockAllHiddenSettings(bool makeVisible = true)
        {
            int attrVal = makeVisible ? 2 : 1;
            string flag = makeVisible ? "-ATTRIB_HIDE" : "+ATTRIB_HIDE";

            var pairs = new[]
            {
                // SUB_PROCESSOR (54533251-82be-4824-96c1-47b60b740d00)
                ("54533251-82be-4824-96c1-47b60b740d00", "893dee8e-2bef-41e0-89c6-b55d0929964c"), // Min İşlemci Durumu (PROCTHROTTLEMIN)
                ("54533251-82be-4824-96c1-47b60b740d00", "bc5038f7-23e0-4960-96da-33abaf5935ec"), // Max İşlemci Durumu (PROCTHROTTLEMAX)
                ("54533251-82be-4824-96c1-47b60b740d00", "94d3a615-a899-4ac5-ae2b-e4d8f634367f"), // Sistem Soğutma İlkesi (SYSCOOLPOL)
                ("54533251-82be-4824-96c1-47b60b740d00", "be337238-0d82-4146-a960-4f3749d470c7"), // Boost Mode (PERFBOOSTMODE)
                ("54533251-82be-4824-96c1-47b60b740d00", "0cc5b647-c1df-4637-891a-dec35c318583"), // Core Parking Min (CPMINCORES)
                ("54533251-82be-4824-96c1-47b60b740d00", "ea062031-0e34-4ff1-9b6d-eb1059334028"), // Core Parking Max (CPMAXCORES)
                ("54533251-82be-4824-96c1-47b60b740d00", "36687f9e-e3a5-4dbf-b1dc-15eb381c6863"), // EPP (PERFEPP)
                // SUB_PCIEXPRESS (501a4d13-42af-4429-9fd1-a8218c268e20)
                ("501a4d13-42af-4429-9fd1-a8218c268e20", "ee12f906-d277-404b-b6da-e5fa1a576df5"), // ASPM Link State Power Management
                // SUB_USB (2a737441-1930-4402-8d77-b2bebba308a3)
                ("2a737441-1930-4402-8d77-b2bebba308a3", "48e6b7a6-50f5-4782-a5d4-53bb8f07e226"), // USB Selective Suspend
                // SUB_WIFI (19cbb8fa-5279-450e-9fac-8a3d5fedd0c1)
                ("19cbb8fa-5279-450e-9fac-8a3d5fedd0c1", "12bbebe6-58d6-4636-95bb-3217ef867c1a"), // Wireless Power Saving Mode
                // SUB_SLEEP (238c9fa8-0aad-41ed-83f4-97be242c8f20)
                ("238c9fa8-0aad-41ed-83f4-97be242c8f20", "bd3b718a-0680-4d9d-8ab2-e1d2b4ac806d"), // Wake Timers
                ("238c9fa8-0aad-41ed-83f4-97be242c8f20", "9d7815a6-7ee4-497e-8888-515a05f02364"), // Hibernate Idle
                // SUB_DISK (0012ee47-9041-4b5d-9b77-535fba8b1442)
                ("0012ee47-9041-4b5d-9b77-535fba8b1442", "6738e2c4-e8a5-4a42-b16a-e040e769756e"), // Turn off hard disk after
                // SUB_BUTTONS (4f971e89-eebd-4455-a8de-9e59040e7347)
                ("4f971e89-eebd-4455-a8de-9e59040e7347", "7648efa3-dd9c-4e3e-b566-50f929386280"), // Power button action
                ("4f971e89-eebd-4455-a8de-9e59040e7347", "96996bc0-ad50-47ec-923b-6f41874dd9eb"), // Sleep button action
            };

            if (IsAdministrator())
            {
                // Doğrudan Registry ve powercfg üzerinden uygula
                foreach (var (sub, setting) in pairs)
                {
                    try
                    {
                        using var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\{sub}\{setting}", true);
                        key?.SetValue("Attributes", attrVal, RegistryValueKind.DWord);
                    }
                    catch { }
                    ProcessHelper.Execute("powercfg", $"-attributes {sub} {setting} {flag}");
                }

                try
                {
                    using var procKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00", true);
                    procKey?.SetValue("Attributes", attrVal, RegistryValueKind.DWord);
                }
                catch { }

                return true;
            }

            // Yönetici izni yoksa, tek bir UAC istemiyle çalışacak geçici betik oluştur ve 'runas' ile çalıştır
            string tempScript = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"haytool_unlock_{Guid.NewGuid():N}.cmd");
            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("@echo off");
                sb.AppendLine("chcp 65001 >nul");
                
                // SUB_PROCESSOR ana kategorisini aç/kapat
                sb.AppendLine($@"reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00"" /v Attributes /t REG_DWORD /d {attrVal} /f >nul 2>&1");

                foreach (var (sub, setting) in pairs)
                {
                    sb.AppendLine($@"reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\{sub}\{setting}"" /v Attributes /t REG_DWORD /d {attrVal} /f >nul 2>&1");
                    sb.AppendLine($@"powercfg -attributes {sub} {setting} {flag} >nul 2>&1");
                }

                System.IO.File.WriteAllText(tempScript, sb.ToString(), System.Text.Encoding.UTF8);

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{tempScript}\"",
                    Verb = "runas",
                    UseShellExecute = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };

                using var proc = System.Diagnostics.Process.Start(psi);
                proc?.WaitForExit();
            }
            catch { }
            finally
            {
                try
                {
                    if (System.IO.File.Exists(tempScript))
                    {
                        System.IO.File.Delete(tempScript);
                    }
                }
                catch { }
            }

            return AreHiddenSettingsUnlocked() == makeVisible;
        }
    }
}
