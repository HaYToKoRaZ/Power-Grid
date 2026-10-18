using System;
using System.Runtime.InteropServices;

namespace PowerGridHaYTooL.Helpers
{
    public static class SystemInfoHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_POWER_STATUS
        {
            public byte ACLineStatus;
            public byte BatteryFlag;
            public byte BatteryLifePercent;
            public byte SystemStatusFlag;
            public int BatteryLifeTime;
            public int BatteryFullLifeTime;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS lpSystemPowerStatus);

        /// <summary>
        /// Sistemde fiziksel bir batarya/pil olup olmadığını Win32 API ile anında (0ms) tespit eder.
        /// Masaüstü PC'lerde false döner.
        /// </summary>
        public static bool HasBattery()
        {
            try
            {
                if (GetSystemPowerStatus(out var status))
                {
                    // BatteryFlag == 128 (0x80): No system battery (Masaüstü bilgisayar)
                    // BatteryFlag == 255 (0xFF): Unknown status / no battery
                    if (status.BatteryFlag == 128 || status.BatteryFlag == 255)
                    {
                        return false;
                    }
                    return true;
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Sistemin masaüstü (Desktop) bilgisayar olup olmadığını döner.
        /// </summary>
        public static bool IsDesktopPc() => !HasBattery();
    }
}
