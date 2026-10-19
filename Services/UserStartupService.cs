using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using PowerGridHaYTooL.Models;

namespace PowerGridHaYTooL.Services
{
    public static class UserStartupService
    {
        private static readonly HashSet<string> IgnoredFolders = new(StringComparer.OrdinalIgnoreCase)
        {
            "Public",
            "Default",
            "Default User",
            "All Users",
            "desktop.ini",
            "WDAGUtilityAccount"
        };

        public static List<UserStartupItem> GetLocalUsers(List<PowerPlanItem> allPlans)
        {
            var userMap = new Dictionary<string, UserStartupItem>(StringComparer.OrdinalIgnoreCase);
            string currentUserName = Environment.UserName;

            // 1. Windows Kayıt Defteri ProfileList taraması
            try
            {
                using var hklmProfiles = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList");
                if (hklmProfiles != null)
                {
                    foreach (string sid in hklmProfiles.GetSubKeyNames())
                    {
                        if (!sid.StartsWith("S-1-5-21-", StringComparison.OrdinalIgnoreCase)) continue;

                        using var sub = hklmProfiles.OpenSubKey(sid);
                        string? path = sub?.GetValue("ProfileImagePath")?.ToString();
                        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                        {
                            string uName = Path.GetFileName(path);
                            if (IgnoredFolders.Contains(uName)) continue;

                            bool isCurrent = string.Equals(uName, currentUserName, StringComparison.OrdinalIgnoreCase);
                            userMap[uName] = new UserStartupItem
                            {
                                UserName = uName,
                                ProfilePath = path,
                                IsCurrentUser = isCurrent
                            };
                        }
                    }
                }
            }
            catch { }

            // 2. C:\Users dizini yedek taraması (Profil listesinde atlanan veya ek kullanıcılar)
            try
            {
                string usersDir = Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)) ?? @"C:\Users";
                if (Directory.Exists(usersDir))
                {
                    foreach (string dir in Directory.GetDirectories(usersDir))
                    {
                        string uName = Path.GetFileName(dir);
                        if (IgnoredFolders.Contains(uName) || userMap.ContainsKey(uName)) continue;

                        bool isCurrent = string.Equals(uName, currentUserName, StringComparison.OrdinalIgnoreCase);
                        userMap[uName] = new UserStartupItem
                        {
                            UserName = uName,
                            ProfilePath = dir,
                            IsCurrentUser = isCurrent
                        };
                    }
                }
            }
            catch { }

            // Her kullanıcı için mevcut başlangıç planını tespit et
            var result = userMap.Values.ToList();
            foreach (var user in result)
            {
                string? detectedGuid = null;

                // Mevcut oturum kullanıcısı ise önce HKCU Run değerine bak
                if (user.IsCurrentUser)
                {
                    detectedGuid = RegistryService.GetUserStartupPlanGuidFromRegistry();
                }

                // Eğer tespit edilmediyse veya başka kullanıcı ise Startup klasöründeki dosyayı oku
                if (string.IsNullOrEmpty(detectedGuid))
                {
                    detectedGuid = ReadGuidFromStartupFile(user.StartupFilePath);
                }

                user.InitialPlanGuid = detectedGuid;
                user.SelectedPlanGuid = detectedGuid;

                // Erişim denetimi
                try
                {
                    string startupDir = Path.GetDirectoryName(user.StartupFilePath)!;
                    if (Directory.Exists(user.ProfilePath))
                    {
                        user.HasAccess = true;
                    }
                }
                catch
                {
                    user.HasAccess = false;
                }
            }

            // Sıralama: Önce o anki oturum kullanıcısı, ardından diğer kullanıcılar alfabetik
            return result
                .OrderByDescending(u => u.IsCurrentUser)
                .ThenBy(u => u.UserName)
                .ToList();
        }

        public static string? ReadGuidFromStartupFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string content = File.ReadAllText(filePath);
                    int idx = content.IndexOf("-apply ", StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0)
                    {
                        string rest = content.Substring(idx + 7).Trim();
                        int endIdx = rest.IndexOfAny(new[] { ' ', '\r', '\n', '"' });
                        return endIdx > 0 ? rest.Substring(0, endIdx).Trim() : rest.Trim();
                    }
                }
            }
            catch { }
            return null;
        }

        public static bool SaveUserStartupPlan(UserStartupItem user, string? targetGuid, List<PowerPlanItem> allPlans)
        {
            string exePath = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "PowerGridHaYTooL.exe");

            try
            {
                if (string.IsNullOrWhiteSpace(targetGuid))
                {
                    // Planı Kaldır (Yok / Devre Dışı)
                    if (File.Exists(user.StartupFilePath))
                    {
                        File.Delete(user.StartupFilePath);
                    }

                    if (user.IsCurrentUser)
                    {
                        RegistryService.RemoveUserStartupPlan();
                        ConfigService.UserStartupPlanGuid = "";
                        ConfigService.UserStartupPlanName = "";
                        ConfigService.Save();
                    }

                    user.InitialPlanGuid = null;
                    user.SelectedPlanGuid = null;
                    return true;
                }
                else
                {
                    // Belirli bir Plan Ata
                    string startupDir = Path.GetDirectoryName(user.StartupFilePath)!;
                    if (!Directory.Exists(startupDir))
                    {
                        Directory.CreateDirectory(startupDir);
                    }

                    string cmdContent =
                        $"@echo off\r\n" +
                        $":: Power Grid HaYTooL - User Startup Plan Configuration\r\n" +
                        $"start \"\" /b \"{exePath}\" -apply {targetGuid}\r\n";

                    File.WriteAllText(user.StartupFilePath, cmdContent, Encoding.ASCII);

                    if (user.IsCurrentUser)
                    {
                        RegistryService.SetUserStartupPlan(targetGuid, exePath);
                        ConfigService.UserStartupPlanGuid = targetGuid;
                        var matchedPlan = allPlans.FirstOrDefault(p => string.Equals(p.Guid, targetGuid, StringComparison.OrdinalIgnoreCase));
                        ConfigService.UserStartupPlanName = matchedPlan?.Name ?? "";
                        ConfigService.Save();
                    }

                    user.InitialPlanGuid = targetGuid;
                    user.SelectedPlanGuid = targetGuid;
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
