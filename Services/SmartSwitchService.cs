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
        private static SmartSwitchStatus _lastStatus = new();

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
            EnsureValidPlans();
        }

        public static SmartSwitchSettings GetSettings()
        {
            lock (_syncLock)
            {
                return _settings.Clone();
            }
        }

        public static SmartSwitchStatus GetCurrentStatus()
        {
            lock (_syncLock)
            {
                if (string.IsNullOrEmpty(_lastStatus.ActivePlanGuid))
                {
                    var active = PowerManagerService.GetActivePlan();
                    _lastStatus.IsActive = _settings.IsEnabled;
                    _lastStatus.ActivePlanGuid = active?.Guid ?? "";
                    _lastStatus.ActivePlanName = active?.Name ?? "";
                }
                return _lastStatus;
            }
        }

        public static void EnsureValidPlans()
        {
            lock (_syncLock)
            {
                var plans = PowerManagerService.GetPowerPlans();
                if (plans == null || plans.Count == 0) return;

                bool modified = false;

                // 1. Kademe: Düşük / Boşta (Eco / Tasarruf / Saver)
                if (string.IsNullOrWhiteSpace(_settings.LowPlanGuid) || !plans.Any(p => p.Guid.Equals(_settings.LowPlanGuid, StringComparison.OrdinalIgnoreCase)))
                {
                    var low = plans.FirstOrDefault(p => p.Name.Contains("Eco Stealth", StringComparison.OrdinalIgnoreCase))
                           ?? plans.FirstOrDefault(p => p.Name.Contains("Tasarruf", StringComparison.OrdinalIgnoreCase) ||
                                                        p.Name.Contains("Saver", StringComparison.OrdinalIgnoreCase) ||
                                                        p.Name.Contains("Sessiz", StringComparison.OrdinalIgnoreCase) ||
                                                        p.Name.Contains("Quiet", StringComparison.OrdinalIgnoreCase))
                           ?? plans.FirstOrDefault();

                    if (low != null)
                    {
                        _settings.LowPlanGuid = low.Guid;
                        modified = true;
                    }
                }

                // 2. Kademe: Orta / Video / Günlük (Smart Balanced / Dengeli / Balanced)
                if (string.IsNullOrWhiteSpace(_settings.MediumPlanGuid) || !plans.Any(p => p.Guid.Equals(_settings.MediumPlanGuid, StringComparison.OrdinalIgnoreCase)))
                {
                    var med = plans.FirstOrDefault(p => p.Name.Contains("Smart Balanced", StringComparison.OrdinalIgnoreCase))
                           ?? plans.FirstOrDefault(p => p.Name.Contains("Dengeli", StringComparison.OrdinalIgnoreCase) ||
                                                        p.Name.Contains("Balanced", StringComparison.OrdinalIgnoreCase))
                           ?? plans.FirstOrDefault();

                    if (med != null)
                    {
                        _settings.MediumPlanGuid = med.Guid;
                        modified = true;
                    }
                }

                // 3. Kademe: Yüksek / Oyun / Render (Apex Performance / Yüksek / High / Ultimate / Nihai)
                if (string.IsNullOrWhiteSpace(_settings.HighPlanGuid) || !plans.Any(p => p.Guid.Equals(_settings.HighPlanGuid, StringComparison.OrdinalIgnoreCase)))
                {
                    var high = plans.FirstOrDefault(p => p.Name.Contains("Apex Performance", StringComparison.OrdinalIgnoreCase))
                            ?? plans.FirstOrDefault(p => p.Name.Contains("Nihai", StringComparison.OrdinalIgnoreCase) ||
                                                         p.Name.Contains("Ultimate", StringComparison.OrdinalIgnoreCase) ||
                                                         p.Name.Contains("Yüksek", StringComparison.OrdinalIgnoreCase) ||
                                                         p.Name.Contains("High", StringComparison.OrdinalIgnoreCase))
                            ?? plans.LastOrDefault();

                    if (high != null)
                    {
                        _settings.HighPlanGuid = high.Guid;
                        modified = true;
                    }
                }

                if (modified)
                {
                    SaveSettings();
                }
            }
        }

        public static bool ToggleEnabled()
        {
            lock (_syncLock)
            {
                EnsureValidPlans();
                _settings.IsEnabled = !_settings.IsEnabled;

                if (_settings.IsEnabled)
                {
                    ConfigService.ShowSystemTray = true;
                    ConfigService.HideWhenMinimized = true;
                    ConfigService.CloseToTray = true;
                    ConfigService.Save();
                    SystemTrayStateRequested?.Invoke(true);
                    Start();
                }
                else
                {
                    Stop();
                }

                SaveSettings();
                return _settings.IsEnabled;
            }
        }

        public static void UpdateSettings(SmartSwitchSettings newSettings)
        {
            lock (_syncLock)
            {
                _settings = newSettings.Clone();
                EnsureValidPlans();

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
                EnsureValidPlans();

                if (_monitorTimer == null)
                {
                    _currentTier = 0;
                    _targetTier = 0;
                    _targetTierTicks = 0;
                    _monitorTimer = new Timer(OnTimerTick, null, 500, 1500);
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

                _lastStatus.IsActive = false;
                _lastStatus.StatusMessage = ConfigService.CurrentLanguage == "English" ? "Disabled" : "Devre Dışı";
                StatusUpdated?.Invoke(_lastStatus);
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
                // Plan GUID'leri kontrol et
                if (string.IsNullOrWhiteSpace(_settings.LowPlanGuid) ||
                    string.IsNullOrWhiteSpace(_settings.MediumPlanGuid) ||
                    string.IsNullOrWhiteSpace(_settings.HighPlanGuid))
                {
                    EnsureValidPlans();
                }

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

                // Beklenen Hedef Plan GUID'i ve Kademe İsimleri
                string expectedGuid = "";
                string tierNameTr = "";
                string tierNameEn = "";

                switch (evaluatedTier)
                {
                    case 3:
                        expectedGuid = _settings.HighPlanGuid;
                        tierNameTr = $"3. Kademe: Yüksek/Oyun (≥%{_settings.HighThresholdPercent})";
                        tierNameEn = $"Tier 3: High/Gaming (≥{_settings.HighThresholdPercent}%)";
                        break;
                    case 2:
                        expectedGuid = _settings.MediumPlanGuid;
                        tierNameTr = $"2. Kademe: Orta (%{_settings.LowThresholdPercent}-%{_settings.HighThresholdPercent})";
                        tierNameEn = $"Tier 2: Medium ({_settings.LowThresholdPercent}%-{_settings.HighThresholdPercent}%)";
                        break;
                    case 1:
                    default:
                        expectedGuid = _settings.LowPlanGuid;
                        tierNameTr = $"1. Kademe: Düşük/Boşta (<%{_settings.LowThresholdPercent})";
                        tierNameEn = $"Tier 1: Idle/Low (<{_settings.LowThresholdPercent}%)";
                        break;
                }

                // 4. Windows'un Mevcut Gerçek Aktif Güç Planını Al
                var activePlan = PowerManagerService.GetActivePlan();
                string activeGuid = activePlan?.Guid ?? "";

                int realActiveTier = 0;
                if (!string.IsNullOrEmpty(activeGuid))
                {
                    if (string.Equals(activeGuid, _settings.HighPlanGuid, StringComparison.OrdinalIgnoreCase)) realActiveTier = 3;
                    else if (string.Equals(activeGuid, _settings.MediumPlanGuid, StringComparison.OrdinalIgnoreCase)) realActiveTier = 2;
                    else if (string.Equals(activeGuid, _settings.LowPlanGuid, StringComparison.OrdinalIgnoreCase)) realActiveTier = 1;
                }

                _currentTier = realActiveTier != 0 ? realActiveTier : evaluatedTier;

                // 5. Otomatik Geçiş Mantığı (Aktif plan ile hedef plan farklıysa tetikle)
                int requiredTicks = 2;
                bool isPendingSwitch = false;

                if (string.Equals(activeGuid, expectedGuid, StringComparison.OrdinalIgnoreCase))
                {
                    // Sistem zaten olması gereken güç planında
                    _targetTier = evaluatedTier;
                    _targetTierTicks = 0;
                    isPendingSwitch = false;
                }
                else
                {
                    // Sistem farklı bir planda! GPU yükünün gerektirdiği plana geçilmesi gerekiyor.
                    isPendingSwitch = true;

                    if (_targetTier != evaluatedTier)
                    {
                        _targetTier = evaluatedTier;
                        _targetTierTicks = 1;
                    }
                    else
                    {
                        _targetTierTicks++;
                    }

                    // Kademe yükseltirken daha hızlı (TriggerSeconds), düşürürken ani dalgalanmayı önlemek için temkinli (RevertSeconds)
                    requiredTicks = (_targetTier > realActiveTier && realActiveTier != 0)
                        ? Math.Max(1, (int)Math.Round(_settings.TriggerSeconds / 1.5))
                        : Math.Max(2, (int)Math.Round(_settings.RevertSeconds / 1.5));

                    if (_targetTierTicks >= requiredTicks)
                    {
                        if (!string.IsNullOrWhiteSpace(expectedGuid))
                        {
                            _currentTier = _targetTier;
                            _targetTierTicks = 0;
                            isPendingSwitch = false;

                            bool switchSuccess = PowerManagerService.SetActivePlan(expectedGuid, true, false);
                            if (switchSuccess)
                            {
                                string reason = ConfigService.CurrentLanguage == "English"
                                    ? $"GPU Load: {totalGpuUsage:F0}% ({tierNameEn})"
                                    : $"GPU Kullanımı: %{totalGpuUsage:F0} ({tierNameTr})";

                                PlanSwitched?.Invoke(expectedGuid, reason);
                            }
                        }
                    }
                }

                // 6. Durum Bildirimi (UI Telemetrisi için)
                activePlan = PowerManagerService.GetActivePlan();

                string statusMsgTr;
                string statusMsgEn;

                if (isPendingSwitch && _targetTierTicks > 0)
                {
                    statusMsgTr = _targetTier switch
                    {
                        3 => $"🚀 Oyun Algılandı: Yüksek Moda Geçiliyor ({_targetTierTicks}/{requiredTicks})... GPU: %{totalGpuUsage:F0}",
                        2 => $"⚖️ Orta Yük Algılandı: Dengeli Moda Geçiliyor ({_targetTierTicks}/{requiredTicks})... GPU: %{totalGpuUsage:F0}",
                        _ => $"🌱 Boşta Algılandı: Düşük/Eco Moda Dönülüyor ({_targetTierTicks}/{requiredTicks})... GPU: %{totalGpuUsage:F0}"
                    };

                    statusMsgEn = _targetTier switch
                    {
                        3 => $"🚀 Game Detected: Switching to High Mode ({_targetTierTicks}/{requiredTicks})... GPU: {totalGpuUsage:F0}%",
                        2 => $"⚖️ Load Detected: Switching to Balanced Mode ({_targetTierTicks}/{requiredTicks})... GPU: {totalGpuUsage:F0}%",
                        _ => $"🌱 Idle Detected: Reverting to Low/Eco Mode ({_targetTierTicks}/{requiredTicks})... GPU: {totalGpuUsage:F0}%"
                    };
                }
                else
                {
                    statusMsgTr = _currentTier switch
                    {
                        3 => $"🚀 3. Kademe: Yüksek GPU Aktif (%{totalGpuUsage:F0})",
                        2 => $"⚖️ 2. Kademe: Orta GPU Aktif (%{totalGpuUsage:F0})",
                        _ => $"🌱 1. Kademe: Boşta/Tasarruf Modu (%{totalGpuUsage:F0})"
                    };

                    statusMsgEn = _currentTier switch
                    {
                        3 => $"🚀 Tier 3: High GPU Active ({totalGpuUsage:F0}%)",
                        2 => $"⚖️ Tier 2: Medium GPU Active ({totalGpuUsage:F0}%)",
                        _ => $"🌱 Tier 1: Idle/Eco Active ({totalGpuUsage:F0}%)"
                    };
                }

                _lastStatus = new SmartSwitchStatus
                {
                    IsActive = _settings.IsEnabled,
                    CurrentTier = _currentTier,
                    GpuUsagePercent = (float)Math.Round(totalGpuUsage, 1),
                    ForegroundProcessName = procName,
                    ActivePlanGuid = activePlan?.Guid ?? "",
                    ActivePlanName = activePlan?.Name ?? (ConfigService.CurrentLanguage == "English" ? "Unknown" : "Bilinmiyor"),
                    StatusMessage = ConfigService.CurrentLanguage == "English" ? statusMsgEn : statusMsgTr
                };

                StatusUpdated?.Invoke(_lastStatus);
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
