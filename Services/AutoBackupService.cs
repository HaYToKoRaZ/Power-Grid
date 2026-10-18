using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using PowerGridHaYTooL.Helpers;
using PowerGridHaYTooL.Models;

namespace PowerGridHaYTooL.Services
{
    public static class AutoBackupService
    {
        private static readonly object _lock = new object();
        private static DateTime _lastBackupTime = DateTime.MinValue;

        public static string GetBackupDirectory()
        {
            // Uygulamanın kendi çalıştığı dizinin altında 'backup' klasörü
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string localBackup = Path.Combine(baseDir, "backup");
            if (!Directory.Exists(localBackup))
            {
                Directory.CreateDirectory(localBackup);
            }
            return localBackup;
        }

        public static void TriggerAutoBackupAsync(List<PowerPlanItem>? plans = null)
        {
            Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        // Minimum 3 saniye aralık kontrolü (aynı anda mükerrer tetiklemeyi önler)
                        if ((DateTime.Now - _lastBackupTime).TotalSeconds < 3)
                        {
                            return;
                        }

                        var activePlans = plans ?? PowerManagerService.GetPowerPlans();
                        if (activePlans == null || activePlans.Count == 0) return;

                        string backupDir = GetBackupDirectory();
                        if (!Directory.Exists(backupDir))
                        {
                            Directory.CreateDirectory(backupDir);
                        }

                        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                        string zipFileName = $"PowerPlans_{timestamp}.zip";
                        string zipPath = Path.Combine(backupDir, zipFileName);

                        string tempFolder = Path.Combine(Path.GetTempPath(), "PowerGrid_AutoBackup_" + Guid.NewGuid().ToString("N"));
                        Directory.CreateDirectory(tempFolder);

                        try
                        {
                            char[] invalid = Path.GetInvalidFileNameChars();
                            var sbInfo = new StringBuilder();
                            sbInfo.AppendLine("Power Grid HaYTooL - Otomatik Güç Düzenleri Yedeği");
                            sbInfo.AppendLine($"Tarih / Saat : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                            sbInfo.AppendLine($"Toplam Plan  : {activePlans.Count}");
                            sbInfo.AppendLine(new string('-', 55));

                            foreach (var plan in activePlans)
                            {
                                string cleanName = string.Concat(plan.Name.Select(c => invalid.Contains(c) ? '_' : c)).Trim();
                                if (string.IsNullOrWhiteSpace(cleanName)) cleanName = plan.Guid;

                                string powPath = Path.Combine(tempFolder, $"{cleanName}.pow");
                                bool exported = false;

                                try
                                {
                                    PowerManagerService.ExportPlan(plan.Guid, powPath);
                                    if (File.Exists(powPath) && new FileInfo(powPath).Length > 0)
                                    {
                                        exported = true;
                                    }
                                    else if (File.Exists(powPath))
                                    {
                                        File.Delete(powPath);
                                    }
                                }
                                catch { }

                                // powercfg /export yönetici yetkisi gerektirdiğinden (0x522), yetki yoksa her planı kusursuz .reg olarak yedekle
                                if (!exported)
                                {
                                    string regPath = Path.Combine(tempFolder, $"{cleanName}.reg");
                                    ProcessHelper.Execute("reg", $"export \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Power\\User\\PowerSchemes\\{plan.Guid}\" \"{regPath}\" /y");
                                    exported = File.Exists(regPath) && new FileInfo(regPath).Length > 0;
                                }

                                string statusMark = exported ? "[✓]" : "[!]";
                                string activeMark = plan.IsActive ? " (Aktif)" : "";
                                sbInfo.AppendLine($"{statusMark} {plan.Name}{activeMark} (GUID: {plan.Guid})");
                            }

                            // Tüm sistem güç düzenlerinin genel registry imajını da ekle
                            try
                            {
                                string allReg = Path.Combine(tempFolder, "All_PowerSchemes_Full.reg");
                                ProcessHelper.Execute("reg", $"export \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Power\\User\\PowerSchemes\" \"{allReg}\" /y");
                            }
                            catch { }

                            File.WriteAllText(Path.Combine(tempFolder, "Backup_Info.txt"), sbInfo.ToString(), Encoding.UTF8);

                            // ZIP olarak maksimum sıkıştırma ile arşivle
                            ZipFile.CreateFromDirectory(tempFolder, zipPath, CompressionLevel.SmallestSize, false);

                            _lastBackupTime = DateTime.Now;

                            // Maksimum 20 yedek kuralını uygula (en eskileri sil)
                            EnforceMaxBackups(backupDir, 20);
                        }
                        finally
                        {
                            if (Directory.Exists(tempFolder))
                            {
                                try { Directory.Delete(tempFolder, true); } catch { }
                            }
                        }
                    }
                    catch { }
                }
            });
        }

        private static void EnforceMaxBackups(string backupDir, int maxCount)
        {
            try
            {
                var files = Directory.GetFiles(backupDir, "PowerPlans_*.zip")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTime)
                    .ToList();

                if (files.Count > maxCount)
                {
                    foreach (var file in files.Skip(maxCount))
                    {
                        try { file.Delete(); } catch { }
                    }
                }
            }
            catch { }
        }

        public static List<AutoBackupItem> GetAutoBackups()
        {
            var list = new List<AutoBackupItem>();
            try
            {
                string backupDir = GetBackupDirectory();
                if (!Directory.Exists(backupDir)) return list;

                var files = Directory.GetFiles(backupDir, "PowerPlans_*.zip")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTime)
                    .ToList();

                foreach (var file in files)
                {
                    var item = new AutoBackupItem
                    {
                        FilePath = file.FullName,
                        FileName = file.Name,
                        CreatedTime = file.LastWriteTime,
                        SizeBytes = file.Length,
                        PlanCountText = "Otomatik Yedek",
                        Details = ""
                    };

                    try
                    {
                        using var archive = ZipFile.OpenRead(file.FullName);
                        var infoEntry = archive.GetEntry("Backup_Info.txt");
                        if (infoEntry != null)
                        {
                            using var reader = new StreamReader(infoEntry.Open(), Encoding.UTF8);
                            string content = reader.ReadToEnd();
                            item.Details = content;

                            var match = System.Text.RegularExpressions.Regex.Match(content, @"Toplam Plan\s*:\s*(\d+)");
                            if (match.Success)
                            {
                                item.PlanCountText = $"{match.Groups[1].Value} Plan";
                            }
                            else
                            {
                                int powCount = archive.Entries.Count(e => e.FullName.EndsWith(".pow", StringComparison.OrdinalIgnoreCase));
                                item.PlanCountText = $"{powCount} Plan";
                            }
                        }
                        else
                        {
                            int powCount = archive.Entries.Count(e => e.FullName.EndsWith(".pow", StringComparison.OrdinalIgnoreCase));
                            item.PlanCountText = $"{powCount} Plan";
                        }
                    }
                    catch { }

                    list.Add(item);
                }
            }
            catch { }
            return list;
        }

        public static (int SuccessCount, int FailCount, string Message) RestoreFromZip(string zipPath)
        {
            if (!File.Exists(zipPath))
            {
                return (0, 0, "Belirtilen ZIP yedek dosyası bulunamadı.");
            }

            int success = 0;
            int fail = 0;
            string tempFolder = Path.Combine(Path.GetTempPath(), "PowerGrid_Restore_" + Guid.NewGuid().ToString("N"));

            try
            {
                Directory.CreateDirectory(tempFolder);
                ZipFile.ExtractToDirectory(zipPath, tempFolder);

                // 1. Önce .pow dosyalarını içeri aktarmayı dene
                var powFiles = Directory.GetFiles(tempFolder, "*.pow");
                if (powFiles.Length > 0)
                {
                    foreach (var pow in powFiles)
                    {
                        if (PowerManagerService.ImportPlan(pow))
                        {
                            success++;
                        }
                        else
                        {
                            fail++;
                        }
                    }
                }

                // 2. Eğer .pow aktarılamadıysa veya yoksa .reg dosyalarından içe aktar
                if (success == 0)
                {
                    var regFiles = Directory.GetFiles(tempFolder, "*.reg");
                    foreach (var reg in regFiles)
                    {
                        if (Path.GetFileName(reg).Equals("All_PowerSchemes_Full.reg", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var (exitCode, _) = ProcessHelper.Execute("reg", $"import \"{reg}\"");
                        if (exitCode == 0)
                            success++;
                        else
                            fail++;
                    }

                    if (success == 0 && File.Exists(Path.Combine(tempFolder, "All_PowerSchemes_Full.reg")))
                    {
                        var (exitCode, _) = ProcessHelper.Execute("reg", $"import \"{Path.Combine(tempFolder, "All_PowerSchemes_Full.reg")}\"");
                        if (exitCode == 0)
                            success++;
                    }
                }

                return (success, fail, $"{success} adet güç düzeni başarıyla geri yüklendi.");
            }
            catch (Exception ex)
            {
                return (success, fail, $"Geri yükleme sırasında hata oluştu: {ex.Message}");
            }
            finally
            {
                if (Directory.Exists(tempFolder))
                {
                    try { Directory.Delete(tempFolder, true); } catch { }
                }
            }
        }
    }
}
