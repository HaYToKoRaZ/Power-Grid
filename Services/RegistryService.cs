using System;
using System.Collections.Generic;
using Microsoft.Win32;
using PowerGridHaYTooL.Models;

namespace PowerGridHaYTooL.Services
{
    public static class RegistryService
    {
        private const string ContextMenuKeyPath = @"Software\Classes\DesktopBackground\Shell\PowerGridHaYTooL";

        public static bool AddDesktopContextMenu(List<PowerPlanItem> plans)
        {
            try
            {
                // Önce eski kayıtları temizle (böylece silinen veya adı değişen planlar menüde artık kalmaz)
                Registry.CurrentUser.DeleteSubKeyTree(ContextMenuKeyPath, false);

                using var key = Registry.CurrentUser.CreateSubKey(ContextMenuKeyPath);
                if (key == null) return false;

                key.SetValue("MUIVerb", "Power Grid (HaYTooL)");
                key.SetValue("SubCommands", "");

                string appPath = Environment.ProcessPath ?? "";
                if (string.IsNullOrEmpty(appPath))
                {
                    try { appPath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? ""; } catch { }
                }

                if (!string.IsNullOrEmpty(appPath) && System.IO.File.Exists(appPath))
                {
                    key.SetValue("Icon", $"\"{appPath}\",0");
                }
                else
                {
                    key.SetValue("Icon", "powercpl.dll");
                }

                using var shellSub = key.CreateSubKey("Shell");
                if (shellSub == null) return false;

                // 1. Yazılımımızı Aç Seçeneği ("⚡ Power Grid (HaYTooL) Aç")
                using var openKey = shellSub.CreateSubKey("00_OpenApp");
                openKey.SetValue("MUIVerb", "⚡ Power Grid (HaYTooL) Aç");
                if (!string.IsNullOrEmpty(appPath) && System.IO.File.Exists(appPath))
                {
                    openKey.SetValue("Icon", $"\"{appPath}\",0");
                    using var openCmd = openKey.CreateSubKey("command");
                    openCmd.SetValue("", $"\"{appPath}\"");
                }
                else
                {
                    openKey.SetValue("Icon", "powercpl.dll");
                    using var openCmd = openKey.CreateSubKey("command");
                    openCmd.SetValue("", "control.exe powercfg.cpl");
                }
                openKey.SetValue("SeparatorAfter", "");

                // 2. Güç Planları Listesi
                int i = 1;
                foreach (var p in plans)
                {
                    using var planKey = shellSub.CreateSubKey($"Plan{i:D2}");
                    planKey.SetValue("MUIVerb", $"{p.PlanIcon} {p.Name}" + (p.IsActive ? " (Aktif)" : ""));
                    planKey.SetValue("Icon", "powercpl.dll");

                    using var cmdKey = planKey.CreateSubKey("command");
                    cmdKey.SetValue("", $"powercfg.exe /setactive {p.Guid}");
                    i++;
                }

                // 3. Denetim Masası Güç Seçenekleri
                using var cplKey = shellSub.CreateSubKey("99_PowerCpl");
                cplKey.SetValue("MUIVerb", "⚙ Denetim Masası Güç Seçenekleri");
                cplKey.SetValue("Icon", "powercpl.dll");
                cplKey.SetValue("SeparatorBefore", "");
                using var cplCmd = cplKey.CreateSubKey("command");
                cplCmd.SetValue("", "control.exe powercfg.cpl");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool RemoveDesktopContextMenu()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(ContextMenuKeyPath, false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsDesktopContextMenuAdded()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(ContextMenuKeyPath);
                return key != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsAutoStartEnabled()
        {
            try
            {
                using var rk = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                return rk?.GetValue("PowerGridHaYTooL") != null;
            }
            catch
            {
                return false;
            }
        }

        public static void SetAutoStart(bool enable, string exePath)
        {
            try
            {
                using var rk = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (rk != null)
                {
                    if (enable)
                    {
                        rk.SetValue("PowerGridHaYTooL", $"\"{exePath}\"");
                    }
                    else
                    {
                        rk.DeleteValue("PowerGridHaYTooL", false);
                    }
                }
            }
            catch { }
        }

        private const string UserStartupRunValue = "PowerGrid_StartupPlan";

        public static bool SetUserStartupPlan(string guid, string exePath)
        {
            try
            {
                using var rk = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (rk != null)
                {
                    if (!string.IsNullOrEmpty(guid) && !string.IsNullOrEmpty(exePath))
                    {
                        rk.SetValue(UserStartupRunValue, $"\"{exePath}\" -apply {guid}");
                        return true;
                    }
                    else
                    {
                        rk.DeleteValue(UserStartupRunValue, false);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public static bool RemoveUserStartupPlan()
        {
            return SetUserStartupPlan(string.Empty, string.Empty);
        }

        public static string? GetUserStartupPlanGuidFromRegistry()
        {
            try
            {
                using var rk = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                string? val = rk?.GetValue(UserStartupRunValue)?.ToString();
                if (!string.IsNullOrEmpty(val))
                {
                    int idx = val.IndexOf("-apply ", StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0)
                    {
                        return val.Substring(idx + 7).Trim().Trim('"');
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
