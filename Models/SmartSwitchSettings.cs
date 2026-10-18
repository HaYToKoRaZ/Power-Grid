using System;

namespace PowerGridHaYTooL.Models
{
    public class SmartSwitchSettings
    {
        public bool IsEnabled { get; set; } = false;

        // 3 Kademeli GPU Güç Planı GUID'leri
        public string LowPlanGuid { get; set; } = "";     // Kademe 1: Boşta / Düşük Kullanım (0 - LowThreshold)
        public string MediumPlanGuid { get; set; } = "";  // Kademe 2: Orta Kullanım (LowThreshold - HighThreshold)
        public string HighPlanGuid { get; set; } = "";    // Kademe 3: Yüksek / Oyun Kullanımı (HighThreshold - 100)

        // Eşik Değerleri (%)
        public int LowThresholdPercent { get; set; } = 10;   // Varsayılan %10
        public int HighThresholdPercent { get; set; } = 30;  // Varsayılan %30

        public int TriggerSeconds { get; set; } = 3;  // Yüksek kademeye geçiş gecikmesi
        public int RevertSeconds { get; set; } = 6;   // Düşük kademeye dönüş gecikmesi
        public bool ShowNotification { get; set; } = true;

        // Geriye dönük uyumluluk (Eski 2 kademeli ayarlar için)
        public string GamePlanGuid
        {
            get => HighPlanGuid;
            set { if (string.IsNullOrEmpty(HighPlanGuid)) HighPlanGuid = value; }
        }

        public string NormalPlanGuid
        {
            get => LowPlanGuid;
            set { if (string.IsNullOrEmpty(LowPlanGuid)) LowPlanGuid = value; }
        }

        public int GpuThresholdPercent
        {
            get => HighThresholdPercent;
            set { if (value > 0) HighThresholdPercent = value; }
        }

        public SmartSwitchSettings Clone()
        {
            return new SmartSwitchSettings
            {
                IsEnabled = this.IsEnabled,
                LowPlanGuid = this.LowPlanGuid,
                MediumPlanGuid = this.MediumPlanGuid,
                HighPlanGuid = this.HighPlanGuid,
                LowThresholdPercent = this.LowThresholdPercent,
                HighThresholdPercent = this.HighThresholdPercent,
                TriggerSeconds = this.TriggerSeconds,
                RevertSeconds = this.RevertSeconds,
                ShowNotification = this.ShowNotification
            };
        }
    }

    public class SmartSwitchStatus
    {
        public bool IsActive { get; set; }
        public int CurrentTier { get; set; } = 1; // 1: Düşük (%0-10), 2: Orta (%10-30), 3: Yüksek (%30-100)
        public float GpuUsagePercent { get; set; }
        public string ForegroundProcessName { get; set; } = "";
        public string ActivePlanName { get; set; } = "";
        public string ActivePlanGuid { get; set; } = "";
        public string StatusMessage { get; set; } = "";
    }
}
