using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using PowerGridHaYTooL.Models;

namespace PowerGridHaYTooL.Services
{
    public static class SmartSwitchService
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private static readonly object _syncLock = new object();
        private static Timer? _monitorTimer;
        private static SmartSwitchSettings _settings = new();

        private static uint _cachedPid = 0;
        private static List<PerformanceCounter> _cachedGpuCounters = new();

        // 3-Kademeli durum takibi
        private static int _currentTier = 0; // 1: Düşük, 2: Orta, 3: Yüksek
        private static int _targetTier = 0;
        private static int _targetTierTicks = 0;

        public static event Action<SmartSwitchStatus>? StatusUpdated;
        public static event Action<string, string>? PlanSwitched; // newPlanGuid, reason
        public static event Action<bool>? SystemTrayStateRequested; // true to activate tray

        private static string IniPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PowerGridHaYTooL.ini");

        static SmartSwitchService()
        {
            LoadSettings();
        }

        public static SmartSwitchSettings GetSettings()
        {
            lock (_syncLock)
            {
                return _settings.Clone();
            }
        }

        public static void UpdateSettings(SmartSwitchSettings newSettings)
        {
            lock (_syncLock)
            {
                _settings = newSettings.Clone();

                // Kullanıcı bu özelliği aktif ettiğinde otomatik olarak System Tray modu da aktif edilsin
                if (_settings.IsEnabled)
                {
                    ConfigService.ShowSystemTray = true;
                    ConfigService.HideWhenMinimized = true;
                    ConfigService.CloseToTray = true;
                    ConfigService.Save();
                    SystemTrayStateRequested?.Invoke(true);
                }

                SaveSettings();

                if (_settings.IsEnabled)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }

        public static void Start()
        {
            lock (_syncLock)
            {
                if (_monitorTimer == null)
                {
                    _currentTier = 0;
                    _targetTier = 0;
                    _targetTierTicks = 0;
                    _monitorTimer = new Timer(OnTimerTick, null, 1000, 1500);
                }
            }
        }

        public static void Stop()
        {
            lock (_syncLock)
            {
                _monitorTimer?.Dispose();
                _monitorTimer = null;
                CleanupCounters();
                _currentTier = 0;
                _targetTier = 0;
                _targetTierTicks = 0;
            }
        }

        private static void CleanupCounters()
        {
            foreach (var counter in _cachedGpuCounters)
            {
                try
                {
                    counter.Dispose();
                }
                catch { }
            }
            _cachedGpuCounters.Clear();
            _cachedPid = 0;
        }

        private static void OnTimerTick(object? state)
        {
            if (!_settings.IsEnabled)
            {
                Stop();
                return;
            }

            try
            {
                float totalGpuUsage = 0f;
                string procName = "Desktop";
                uint currentPid = 0;

                // 1. Aktif Ön Plan Penceresi ve PID tespiti
                IntPtr fgWindow = GetForegroundWindow();
                if (fgWindow != IntPtr.Zero)
                {
                    GetWindowThreadProcessId(fgWindow, out currentPid);
                    if (currentPid > 0)
                    {
                        try
                        {
                            var proc = Process.GetProcessById((int)currentPid);
                            procName = proc.ProcessName;
                        }
                        catch { }
                    }
                }

                // 2. Ön Plan Süreci için GPU Sayaçlarını Önbelleğe Al ve Oku
                if (currentPid > 0 && currentPid != _cachedPid)
                {
                    CleanupCounters();
                    _cachedPid = currentPid;

                    try
                    {
                        var category = new PerformanceCounterCategory("GPU Engine");
                        string matchPrefix = $"pid_{currentPid}_";
                        var instances = category.GetInstanceNames()
                            .Where(i => i.StartsWith(matchPrefix, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        foreach (var inst in instances)
                        {
                            try
                            {
                                var counter = new PerformanceCounter("GPU Engine", "Utilization Percentage", inst);
                                counter.NextValue(); // İlk okuma başlangıç değerini sıfırlar
                                _cachedGpuCounters.Add(counter);
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                // Ön plan sürecinin GPU motorlarının kullanımını topla
                if (_cachedGpuCounters.Count > 0)
                {
                    try
                    {
                        foreach (var counter in _cachedGpuCounters)
                        {
                            float val = counter.NextValue();
                            if (val > 0) totalGpuUsage += val;
                        }

                        if (totalGpuUsage < 0) totalGpuUsage = 0;
                        if (totalGpuUsage > 100) totalGpuUsage = 100;
                    }
                    catch
                    {
                        CleanupCounters();
                    }
                }

                // 3. Üç Kademeli GPU Değerlendirmesi
                // Kademe 1: 0 - LowThresholdPercent (örn. %0 - %10)
                // Kademe 2: LowThresholdPercent - HighThresholdPercent (örn. %10 - %30)
                // Kademe 3: HighThresholdPercent - %100 (örn. %30 - %100)
                int evaluatedTier;
                if (totalGpuUsage >= _settings.HighThresholdPercent)
                {
                    evaluatedTier = 3;
                }
                else if (totalGpuUsage >= _settings.LowThresholdPercent)
                {
                    evaluatedTier = 2;
                }
                else
                {
                    evaluatedTier = 1;
                }

                // İlk çalıştırma anında mevcut aktif planı tespit et
                if (_currentTier == 0)
                {
                    var active = PowerManagerService.GetActivePlan();
                    if (active != null)
                    {
                        if (active.Guid == _settings.HighPlanGuid) _currentTier = 3;
                        else if (active.Guid == _settings.MediumPlanGuid) _currentTier = 2;
                        else if (active.Guid == _settings.LowPlanGuid) _currentTier = 1;
                        else _currentTier = evaluatedTier;
                    }
                    else
                    {
                        _currentTier = evaluatedTier;
                    }
                }

                if (evaluatedTier == _currentTier)
                {
                    _targetTier = _currentTier;
                    _targetTierTicks = 0;
                }
                else
                {
                    if (evaluatedTier != _targetTier)
                    {
                        _targetTier = evaluatedTier;
                        _targetTierTicks = 1;
                    }
                    else
                    {
                        _targetTierTicks++;
                    }

                    // Kademe yükseltirken daha hızlı (TriggerSeconds), düşürürken ani dalgalanmayı önlemek için temkinli (RevertSeconds)
                    int requiredTicks = (_targetTier > _currentTier)
                        ? Math.Max(1, (int)Math.Round(_settings.TriggerSeconds / 1.5))
                        : Math.Max(2, (int)Math.Round(_settings.RevertSeconds / 1.5));

                    if (_targetTierTicks >= requiredTicks)
                    {
                        string targetGuid = "";
                        string tierNameTr = "";
                        string tierNameEn = "";

                        switch (_targetTier)
                        {
                            case 3:
                                targetGuid = _settings.HighPlanGuid;
                                tierNameTr = $"3. Kademe: Yüksek/Oyun (≥%{_settings.HighThresholdPercent})";
                                tierNameEn = $"Tier 3: High/Gaming (≥{_settings.HighThresholdPercent}%)";
                                break;
                            case 2:
                                targetGuid = _settings.MediumPlanGuid;
                                tierNameTr = $"2. Kademe: Orta (%{_settings.LowThresholdPercent}-%{_settings.HighThresholdPercent})";
                                tierNameEn = $"Tier 2: Medium ({_settings.LowThresholdPercent}%-{_settings.HighThresholdPercent}%)";
                                break;
                            case 1:
                            default:
                                targetGuid = _settings.LowPlanGuid;
                                tierNameTr = $"1. Kademe: Düşük/Boşta (<%{_settings.LowThresholdPercent})";
                                tierNameEn = $"Tier 1: Idle/Low (<{_settings.LowThresholdPercent}%)";
                                break;
                        }

                        if (!string.IsNullOrWhiteSpace(targetGuid))
                        {
                            _currentTier = _targetTier;
                            _targetTierTicks = 0;

                            PowerManagerService.SetActivePlan(targetGuid);

                            string reason = ConfigService.CurrentLanguage == "English"
                                ? $"GPU Load: {totalGpuUsage:F0}% ({tierNameEn})"
                                : $"GPU Kullanımı: %{totalGpuUsage:F0} ({tierNameTr})";

                            PlanSwitched?.Invoke(targetGuid, reason);
                        }
                    }
                }

                // 4. Durum Bildirimi (UI Telemetrisi için)
                var activePlan = PowerManagerService.GetActivePlan();
                string statusMsgTr = _currentTier switch
                {
                    3 => $"🚀 3. Kademe: Yüksek GPU Aktif (%{totalGpuUsage:F0})",
                    2 => $"⚖️ 2. Kademe: Orta GPU Aktif (%{totalGpuUsage:F0})",
                    _ => $"🌱 1. Kademe: Boşta/Düşük Mod (%{totalGpuUsage:F0})"
                };

                string statusMsgEn = _currentTier switch
                {
                    3 => $"🚀 Tier 3: High GPU Active ({totalGpuUsage:F0}%)",
                    2 => $"⚖️ Tier 2: Medium GPU Active ({totalGpuUsage:F0}%)",
                    _ => $"🌱 Tier 1: Idle/Low Active ({totalGpuUsage:F0}%)"
                };

                var status = new SmartSwitchStatus
                {
                    IsActive = _settings.IsEnabled,
                    CurrentTier = _currentTier,
                    GpuUsagePercent = (float)Math.Round(totalGpuUsage, 1),
                    ForegroundProcessName = procName,
                    ActivePlanGuid = activePlan?.Guid ?? "",
                    ActivePlanName = activePlan?.Name ?? (ConfigService.CurrentLanguage == "English" ? "Unknown" : "Bilinmiyor"),
                    StatusMessage = ConfigService.CurrentLanguage == "English" ? statusMsgEn : statusMsgTr
                };

                StatusUpdated?.Invoke(status);
            }
            catch { }
        }

        public static void LoadSettings()
        {
            try
            {
                if (!File.Exists(IniPath)) return;

                var lines = File.ReadAllLines(IniPath, Encoding.UTF8);
                bool inSmartSection = false;

                foreach (var line in lines)
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    {
                        inSmartSection = trimmed.Equals("[SmartSwitch]", StringComparison.OrdinalIgnoreCase);
                        continue;
                    }

                    if (inSmartSection && trimmed.Contains("="))
                    {
                        int eqIdx = trimmed.IndexOf('=');
                        string key = trimmed.Substring(0, eqIdx).Trim();
                        string val = trimmed.Substring(eqIdx + 1).Trim();

                        switch (key.ToLowerInvariant())
                        {
                            case "enabled":
                                _settings.IsEnabled = val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase);
                                break;
                            case "lowplanguid":
                            case "normalplanguid":
                                _settings.LowPlanGuid = val;
                                break;
                            case "mediumplanguid":
                                _settings.MediumPlanGuid = val;
                                break;
                            case "highplanguid":
                            case "gameplanguid":
                                _settings.HighPlanGuid = val;
                                break;
                            case "lowthreshold":
                                if (int.TryParse(val, out int lt)) _settings.LowThresholdPercent = Math.Clamp(lt, 2, 40);
                                break;
                            case "highthreshold":
                            case "gputhreshold":
                                if (int.TryParse(val, out int ht)) _settings.HighThresholdPercent = Math.Clamp(ht, 15, 95);
                                break;
                            case "triggerseconds":
                                if (int.TryParse(val, out int ts)) _settings.TriggerSeconds = Math.Clamp(ts, 1, 30);
                                break;
                            case "revertseconds":
                                if (int.TryParse(val, out int rs)) _settings.RevertSeconds = Math.Clamp(rs, 2, 60);
                                break;
                            case "shownotification":
                                _settings.ShowNotification = val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase);
                                break;
                        }
                    }
                }
            }
            catch { }
        }

        public static void SaveSettings()
        {
            try
            {
                if (!File.Exists(IniPath)) return;

                var lines = File.ReadAllLines(IniPath, Encoding.UTF8).ToList();
                int smartIdx = lines.FindIndex(l => l.Trim().Equals("[SmartSwitch]", StringComparison.OrdinalIgnoreCase));

                var smartLines = new List<string>
                {
                    "[SmartSwitch]",
                    $"Enabled={(_settings.IsEnabled ? "1" : "0")}",
                    $"LowPlanGuid={_settings.LowPlanGuid}",
                    $"MediumPlanGuid={_settings.MediumPlanGuid}",
                    $"HighPlanGuid={_settings.HighPlanGuid}",
                    $"LowThreshold={_settings.LowThresholdPercent}",
                    $"HighThreshold={_settings.HighThresholdPercent}",
                    $"TriggerSeconds={_settings.TriggerSeconds}",
                    $"RevertSeconds={_settings.RevertSeconds}",
                    $"ShowNotification={(_settings.ShowNotification ? "1" : "0")}"
                };

                if (smartIdx >= 0)
                {
                    int endIdx = lines.FindIndex(smartIdx + 1, l => l.Trim().StartsWith("["));
                    if (endIdx < 0) endIdx = lines.Count;

                    lines.RemoveRange(smartIdx, endIdx - smartIdx);
                    lines.InsertRange(smartIdx, smartLines);
                }
                else
                {
                    lines.Add("");
                    lines.AddRange(smartLines);
                }

                File.WriteAllLines(IniPath, lines, Encoding.UTF8);
            }
            catch { }
        }
    }
}
