namespace PowerGridHaYTooL.Models
{
    public class PowerPlanSettings
    {
        public string Guid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        // Alias for Guid / Name
        public string PlanGuid { get => Guid; set => Guid = value; }
        public string PlanName { get => Name; set => Name = value; }

        // --- 1. Ekran, Uyku, Disk ve Zamanlayıcılar ---
        // Ekran Kapatma Süresi (Dakika, 0 = Hiçbir zaman)
        public int VideoTimeoutAc { get; set; }
        public int VideoTimeoutDc { get; set; }
        public int VideoTimeoutAcMinutes { get => VideoTimeoutAc; set => VideoTimeoutAc = value; }
        public int VideoTimeoutDcMinutes { get => VideoTimeoutDc; set => VideoTimeoutDc = value; }

        // Uyku Süresi (Dakika, 0 = Hiçbir zaman)
        public int SleepTimeoutAc { get; set; }
        public int SleepTimeoutDc { get; set; }
        public int SleepTimeoutAcMinutes { get => SleepTimeoutAc; set => SleepTimeoutAc = value; }
        public int SleepTimeoutDcMinutes { get => SleepTimeoutDc; set => SleepTimeoutDc = value; }

        // Hazırda Bekletme Süresi (Dakika, 0 = Hiçbir zaman)
        public int HibernateTimeoutAc { get; set; }
        public int HibernateTimeoutDc { get; set; }

        // Sabit Disk Kapatma Süresi (Dakika, 0 = Hiçbir zaman)
        public int DiskTimeoutAc { get; set; }
        public int DiskTimeoutDc { get; set; }
        public int DiskTimeoutAcMinutes { get => DiskTimeoutAc; set => DiskTimeoutAc = value; }
        public int DiskTimeoutDcMinutes { get => DiskTimeoutDc; set => DiskTimeoutDc = value; }

        // Uyandırma Zamanlayıcılarına İzin Ver (0 = Devre Dışı, 1 = Etkin, 2 = Yalnızca Önemli)
        public int AllowWakeTimersAc { get; set; } = 1;
        public int AllowWakeTimersDc { get; set; } = 1;

        // --- 2. İşlemci Güç Yönetimi (CPU) ---
        // En Düşük İşlemci Durumu (Yüzde %)
        public int CpuMinAc { get; set; }
        public int CpuMinDc { get; set; }
        public int MinCpuAc { get => CpuMinAc; set => CpuMinAc = value; }
        public int MinCpuDc { get => CpuMinDc; set => CpuMinDc = value; }

        // En Yüksek İşlemci Durumu (Yüzde %)
        public int CpuMaxAc { get; set; }
        public int CpuMaxDc { get; set; }
        public int MaxCpuAc { get => CpuMaxAc; set => CpuMaxAc = value; }
        public int MaxCpuDc { get => CpuMaxDc; set => CpuMaxDc = value; }

        // Sistem Soğutma İlkesi (1 = Etkin/Aktif, 0 = Edilgin/Pasif)
        public int CoolingPolicyAc { get; set; }
        public int CoolingPolicyDc { get; set; }

        // İşlemci Performans Artırma Modu (Turbo Boost):
        // 0=Devre Dışı, 1=Etkin, 2=Agresif, 3=Verimli Etkin, 4=Verimli Agresif, 5=Agresif Garantili
        public int BoostModeAc { get; set; } = 2;
        public int BoostModeDc { get; set; } = 2;

        // CPU Çekirdek Park Etme En Az Çekirdek (%) (0-100, 100=Park Yok/Tümü Aktif)
        public int CoreParkingMinAc { get; set; } = 100;
        public int CoreParkingMinDc { get; set; } = 100;

        // CPU Çekirdek Park Etme En Fazla Çekirdek (%) (0-100)
        public int CoreParkingMaxAc { get; set; } = 100;
        public int CoreParkingMaxDc { get; set; } = 100;

        // İşlemci Enerji Performans Tercihi (EPP / HWP):
        // 0 = Maksimum Performans, 50 = Dengeli, 100 = Maksimum Enerji Tasarrufu
        public int EnergyPerfPreferenceAc { get; set; } = 0;
        public int EnergyPerfPreferenceDc { get; set; } = 50;

        // --- 3. Donanım & Çevre Birimleri ---
        // PCI Express Bağlantı Durumu Güç Yönetimi (ASPM):
        // 0 = Kapalı (Maks Performans), 1 = Orta Düzey Güç Tasarrufu, 2 = Maksimum Güç Tasarrufu
        public int PcieAspmAc { get; set; } = 0;
        public int PcieAspmDc { get; set; } = 1;

        // USB Seçici Askıya Alma Ayarı:
        // 0 = Devre Dışı (Portlar kapanmaz), 1 = Etkin
        public int UsbSelectiveSuspendAc { get; set; } = 0;
        public int UsbSelectiveSuspendDc { get; set; } = 1;

        // Kablosuz Bağdaştırıcı Güç Modu (Wi-Fi):
        // 0 = Maksimum Performans, 1 = Düşük Güç Tasarrufu, 2 = Orta Güç Tasarrufu, 3 = Maksimum Tasarruf
        public int WifiPowerSavingAc { get; set; } = 0;
        public int WifiPowerSavingDc { get; set; } = 2;

        // --- 4. Güç Düğmeleri ve Kapak ---
        // Güç Düğmesi Eylemi: 0=Hiçbir şey yapma, 1=Uyku, 2=Hazırda Beklet, 3=Kapat
        public int PowerButtonActionAc { get; set; } = 3;
        public int PowerButtonActionDc { get; set; } = 3;

        // Uyku Düğmesi Eylemi: 0=Hiçbir şey yapma, 1=Uyku, 2=Hazırda Beklet, 3=Kapat
        public int SleepButtonActionAc { get; set; } = 1;
        public int SleepButtonActionDc { get; set; } = 1;
    }
}
