using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace PowerGridHaYTooL.Services
{
    /// <summary>
    /// Windows Display & Monitör Yenileme Hızı (Hz) Servisi.
    /// Win32 EnumDisplaySettings ve ChangeDisplaySettings API'leri ile çalışır.
    /// Standart kullanıcı izniyle (UAC gerektirmeden) güvenle yenileme hızını okur ve değiştirir.
    /// </summary>
    public static class DisplayService
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int DM_DISPLAYFREQUENCY = 0x400000;
        private const int CDS_UPDATEREGISTRY = 0x01;
        private const int DISP_CHANGE_SUCCESSFUL = 0;

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern bool EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        /// <summary>
        /// Mevcut birincil ekranın anlık yenileme hızını (Hz) döndürür.
        /// </summary>
        public static int GetCurrentRefreshRate()
        {
            try
            {
                DEVMODE current = new DEVMODE();
                current.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref current))
                {
                    return current.dmDisplayFrequency;
                }
            }
            catch { }
            return 60;
        }

        /// <summary>
        /// Mevcut ekran çözünürlüğünü (Genişlik x Yükseklik) döndürür.
        /// </summary>
        public static (int Width, int Height) GetCurrentResolution()
        {
            try
            {
                DEVMODE current = new DEVMODE();
                current.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref current))
                {
                    return (current.dmPelsWidth, current.dmPelsHeight);
                }
            }
            catch { }
            return (1920, 1080);
        }

        /// <summary>
        /// Mevcut ekran çözünürlüğünde monitörün ve ekran kartının desteklediği tüm Hz değerlerini sıralı liste olarak döndürür.
        /// </summary>
        public static List<int> GetSupportedRefreshRates()
        {
            var result = new SortedSet<int>();
            try
            {
                DEVMODE current = new DEVMODE();
                current.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref current))
                {
                    int width = current.dmPelsWidth;
                    int height = current.dmPelsHeight;

                    DEVMODE dm = new DEVMODE();
                    dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                    int modeIndex = 0;

                    while (EnumDisplaySettings(null, modeIndex, ref dm))
                    {
                        if (dm.dmPelsWidth == width && dm.dmPelsHeight == height && dm.dmDisplayFrequency > 0)
                        {
                            result.Add(dm.dmDisplayFrequency);
                        }
                        modeIndex++;
                    }
                }
            }
            catch { }

            if (result.Count == 0)
            {
                result.Add(60);
            }

            return result.ToList();
        }

        /// <summary>
        /// Monitörün yenileme hızını (Hz) değiştirir.
        /// </summary>
        /// <param name="targetHz">Geçilmek istenen Hz değeri (Örn: 60, 120, 144, 165)</param>
        /// <returns>Başarılıysa true, aksi halde false.</returns>
        public static bool SetRefreshRate(int targetHz)
        {
            if (targetHz <= 0) return false;

            try
            {
                DEVMODE current = new DEVMODE();
                current.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref current))
                {
                    return false;
                }

                if (current.dmDisplayFrequency == targetHz)
                {
                    return true; // Zaten hedeflenen Hz değerinde
                }

                // Desteklenen Hz değerleri arasında mı kontrol et
                var supported = GetSupportedRefreshRates();
                if (!supported.Contains(targetHz))
                {
                    return false;
                }

                DEVMODE newMode = current;
                newMode.dmDisplayFrequency = targetHz;
                newMode.dmFields = DM_DISPLAYFREQUENCY;

                // Önce kalıcı registry güncellemesi ile dene, olmazsa dinamik geçiş dene
                int res = ChangeDisplaySettings(ref newMode, CDS_UPDATEREGISTRY);
                if (res != DISP_CHANGE_SUCCESSFUL)
                {
                    res = ChangeDisplaySettings(ref newMode, 0);
                }

                return res == DISP_CHANGE_SUCCESSFUL;
            }
            catch
            {
                return false;
            }
        }
    }
}
