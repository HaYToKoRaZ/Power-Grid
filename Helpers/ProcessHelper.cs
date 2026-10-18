using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PowerGridHaYTooL.Helpers
{
    public static class ProcessHelper
    {
        static ProcessHelper()
        {
            // .NET Core / .NET 10 içinde varsayılan olarak kapalı olan Windows Kod Sayfaları sağlayıcısını kaydet
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }
            catch { }
        }

        public static (int ExitCode, string Output) Execute(string fileName, string arguments)
        {
            try
            {
                // powercfg çıktısını ham byte olarak okuyup en doğru Windows Kod Sayfası ile çözümlüyoruz.
                // Kod sayfası sağlayıcısı kayıtlı olduğunda CP857 / CP850 / CP437 Türkçe harfleri ('ü', 'ç', 'ş', 'ğ', 'ı', 'ö') kusursuz çözer.
                var psi = new ProcessStartInfo(fileName, arguments)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = Process.Start(psi);
                if (proc == null) return (-1, string.Empty);

                using var ms = new MemoryStream();
                proc.StandardOutput.BaseStream.CopyTo(ms);
                proc.WaitForExit();

                byte[] bytes = ms.ToArray();
                string output = DecodeBytes(bytes);

                return (proc.ExitCode, output);
            }
            catch (Exception ex)
            {
                return (-1, ex.Message);
            }
        }

        private static string DecodeBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return string.Empty;

            // Öncelikli olarak CP857 (Türkçe OEM), ardından CP850 / CP437 dene
            int[] codepages = { 857, 850, 437, 1254 };
            foreach (int cp in codepages)
            {
                try
                {
                    var enc = Encoding.GetEncoding(cp);
                    string decoded = enc.GetString(bytes);
                    // Eğer 'Yüksek' veya 'Güç' başarıyla çözümlendiyse doğru tablodur
                    if (decoded.Contains("Yüksek") || decoded.Contains("Güç") || decoded.Contains("Dengeli"))
                    {
                        return decoded;
                    }
                }
                catch { }
            }

            // Fallback olarak Encoding.Default veya UTF-8
            return Encoding.Default.GetString(bytes);
        }

        public static void OpenUri(string uri, string args = "")
        {
            try
            {
                Process.Start(new ProcessStartInfo(uri, args) { UseShellExecute = true });
            }
            catch { }
        }

        public static bool CreateDesktopShortcut(string name, string guid, bool isEnglish = false)
        {
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                char[] invalidChars = Path.GetInvalidFileNameChars();
                string cleanName = string.Concat(name.Select(c => invalidChars.Contains(c) ? ' ' : c)).Trim();
                string fileName = $"{cleanName} - Power Grid HaYTooL.cmd";
                string batPath = Path.Combine(desktop, fileName);

                var sb = new StringBuilder();
                sb.AppendLine("@echo off");
                sb.AppendLine($"powercfg.exe /setactive {guid}");
                sb.AppendLine("start \"\" powershell.exe -WindowStyle Hidden -NoProfile -Command \"[System.Media.SystemSounds]::Asterisk.Play(); Start-Sleep -Milliseconds 400\"");

                File.WriteAllText(batPath, sb.ToString(), new UTF8Encoding(false));
                return true;
            }
            catch
            {
                return false;
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern uint RegisterWindowMessage(string lpString);

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        public const string RestoreMessageName = "PowerGridHaYTooL_RestoreWindow_Msg";
        public const int HWND_BROADCAST = 0xffff;
        public const int SW_RESTORE = 9;

        public static void SignalExistingInstanceToForeground()
        {
            try
            {
                uint msg = RegisterWindowMessage(RestoreMessageName);
                PostMessage((IntPtr)HWND_BROADCAST, msg, IntPtr.Zero, IntPtr.Zero);
            }
            catch { }

            try
            {
                var current = Process.GetCurrentProcess();
                var other = Process.GetProcessesByName(current.ProcessName)
                                   .FirstOrDefault(p => p.Id != current.Id);
                if (other != null && other.MainWindowHandle != IntPtr.Zero)
                {
                    ShowWindowAsync(other.MainWindowHandle, SW_RESTORE);
                    SetForegroundWindow(other.MainWindowHandle);
                }
            }
            catch { }
        }
    }
}
