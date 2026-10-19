using System;
using System.IO;
using System.Threading;
using System.Windows;
using PowerGridHaYTooL.Helpers;
using PowerGridHaYTooL.Services;

namespace PowerGridHaYTooL
{
    public partial class App : Application
    {
        private static Mutex? _instanceMutex;
        private static bool _keepRunningInTray = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // CLI Argüman Yönetimi (Command Line Interface)
            if (e.Args.Length > 0)
            {
                if (HandleCommandLineArgs(e.Args))
                {
                    if (!_keepRunningInTray)
                    {
                        Shutdown(0);
                        return;
                    }

                    // Arka planda tepside çalışma isteği (-smart on ile kapalıyken tetiklendiğinde)
                    bool createdNewTray;
                    _instanceMutex = new Mutex(true, "PowerGridHaYTooL_SingleInstance_Mutex_HaYTooL", out createdNewTray);
                    if (!createdNewTray)
                    {
                        Shutdown(0);
                        return;
                    }

                    var trayWindow = new MainWindow(startMinimizedToTray: true);
                    trayWindow.ShowActivated = false;
                    trayWindow.WindowState = WindowState.Minimized;
                    trayWindow.Show();
                    trayWindow.Hide();
                    return;
                }
            }

            // Single Instance Mutex Protection (Önceki pencereyi öne getir)
            bool createdNew;
            _instanceMutex = new Mutex(true, "PowerGridHaYTooL_SingleInstance_Mutex_HaYTooL", out createdNew);
            if (!createdNew)
            {
                ProcessHelper.SignalExistingInstanceToForeground();
                Shutdown(0);
                return;
            }

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private bool HandleCommandLineArgs(string[] args)
        {
            string cmd = args[0].ToLowerInvariant();

            // 1. Akıllı Güç Anahtarı (Smart Switch CLI)
            if (cmd == "-smart" || cmd == "/smart" || cmd == "--smart" ||
                cmd == "-smart-switch" || cmd == "/smart-switch" || cmd == "--smart-switch")
            {
                ProcessHelper.AttachParentConsole();
                string subArg = args.Length > 1 ? args[1].ToLowerInvariant() : "toggle";
                bool isRunning = ProcessHelper.IsAppAlreadyRunning();

                if (subArg == "status" || subArg == "durum" || subArg == "info")
                {
                    var settings = SmartSwitchService.GetSettings();
                    var active = PowerManagerService.GetActivePlan();

                    Console.WriteLine();
                    Console.WriteLine("=======================================================");
                    Console.WriteLine("   ⚡ Power Grid HaYTooL - Akıllı GPU Güç Anahtarı");
                    Console.WriteLine("=======================================================");
                    Console.WriteLine($" [*] Uygulama Durumu : {(isRunning ? "ÇALIŞIYOR (Arka Plan / Sistem Tepsisi)" : "KAPALI (Çalışmıyor)")}");
                    Console.WriteLine($" [*] Akıllı Anahtar  : {(settings.IsEnabled ? "AÇIK (Etkin)" : "KAPALI (Devre Dışı)")}");
                    Console.WriteLine($" [*] Aktif Güç Planı : {active?.Name ?? "Bilinmiyor"}");
                    if (!isRunning && settings.IsEnabled)
                    {
                        Console.WriteLine(" [!] UYARI           : Akıllı anahtar açık fakat uygulama kapalı olduğu");
                        Console.WriteLine("                       için arka planda GPU yükü takip edilemez!");
                        Console.WriteLine(" [*] Başlatmak için  : PowerGridHaYTooL.exe -smart on");
                    }
                    Console.WriteLine("=======================================================");
                    Console.WriteLine();
                }
                else
                {
                    bool enable;
                    int actionCode; // 1: On, 0: Off, 2: Toggle

                    if (subArg == "on" || subArg == "enable" || subArg == "1" || subArg == "true" || subArg == "ac")
                    {
                        enable = true;
                        actionCode = 1;
                    }
                    else if (subArg == "off" || subArg == "disable" || subArg == "0" || subArg == "false" || subArg == "kapat")
                    {
                        enable = false;
                        actionCode = 0;
                    }
                    else // toggle / degistir
                    {
                        var cur = SmartSwitchService.GetSettings();
                        enable = !cur.IsEnabled;
                        actionCode = 2;
                    }

                    SmartSwitchService.SetEnabled(enable);

                    Console.WriteLine();
                    Console.WriteLine("=======================================================");
                    Console.WriteLine("   ⚡ Power Grid HaYTooL - Akıllı GPU Güç Anahtarı");
                    Console.WriteLine("=======================================================");
                    Console.WriteLine($" [✓] Durum: {(enable ? "[AÇIK / AKTİF]" : "[KAPALI / DEVRE DIŞI]")}");

                    if (isRunning)
                    {
                        ProcessHelper.SignalSmartSwitchState(actionCode);
                        Console.WriteLine(" [*] Arka planda çalışan uygulamaya iletildi.");
                    }
                    else
                    {
                        if (enable)
                        {
                            Console.WriteLine(" [*] Bilgilendirme: Uygulama kapalıydı; GPU izleme ve otomatik");
                            Console.WriteLine("     geçişin çalışabilmesi için arka planda (Sistem Tepsisi)");
                            Console.WriteLine("     sessizce başlatıldı.");
                            _keepRunningInTray = true;
                        }
                        else
                        {
                            Console.WriteLine(" [*] Uygulama şu anda kapalı; ayar yapılandırma dosyasına (INI) kaydedildi.");
                        }
                    }

                    Console.WriteLine("=======================================================");
                    Console.WriteLine();
                }

                ProcessHelper.DetachParentConsole();
                return true;
            }

            // 2. Güç Planı Uygulama (-apply /setactive {GUID})
            if ((cmd == "-apply" || cmd == "/apply" || cmd == "-setactive" || cmd == "/setactive") && args.Length >= 2)
            {
                string targetGuid = args[1].Trim();
                try
                {
                    PowerManagerService.SetActivePlan(targetGuid, false);
                }
                catch { }
                return true;
            }

            // 3. Plan Listesi (-list /list)
            if (cmd == "-list" || cmd == "/list" || cmd == "--list")
            {
                ProcessHelper.AttachParentConsole();
                var plans = PowerManagerService.GetPowerPlans();
                Console.WriteLine();
                Console.WriteLine("=======================================================");
                Console.WriteLine("   ⚡ Power Grid HaYTooL - Sistem Güç Düzenleri");
                Console.WriteLine("=======================================================");
                foreach (var p in plans)
                {
                    string marker = p.IsActive ? " [AKTİF] " : "         ";
                    Console.WriteLine($"{marker}{p.PlanIcon} {p.Name,-35} | {p.Guid}");
                }
                Console.WriteLine("=======================================================");
                Console.WriteLine();
                ProcessHelper.DetachParentConsole();
                return true;
            }

            // 4. Uygulamayı Kapat (-exit / -close / -quit / -kapat)
            if (cmd == "-exit" || cmd == "/exit" || cmd == "-close" || cmd == "/close" ||
                cmd == "-quit" || cmd == "/quit" || cmd == "-kapat" || cmd == "/kapat")
            {
                ProcessHelper.AttachParentConsole();
                bool isRunning = ProcessHelper.IsAppAlreadyRunning();
                Console.WriteLine();
                Console.WriteLine("=======================================================");
                Console.WriteLine("   ⚡ Power Grid HaYTooL");
                Console.WriteLine("=======================================================");
                if (isRunning)
                {
                    ProcessHelper.SignalCloseInstance();
                    Console.WriteLine(" [✓] Arka planda çalışan Power Grid HaYTooL kapatıldı.");
                }
                else
                {
                    Console.WriteLine(" [*] Power Grid HaYTooL zaten çalışmıyor.");
                }
                Console.WriteLine("=======================================================");
                Console.WriteLine();
                ProcessHelper.DetachParentConsole();
                return true;
            }

            // 5. Yardım (-help /? -h)
            if (cmd == "-help" || cmd == "/help" || cmd == "--help" || cmd == "/?" || cmd == "-h")
            {
                ProcessHelper.AttachParentConsole();
                Console.WriteLine();
                Console.WriteLine("=======================================================");
                Console.WriteLine("   ⚡ Power Grid HaYTooL - Komut Satırı (CLI) Kılavuzu");
                Console.WriteLine("=======================================================");
                Console.WriteLine(" Kullanım: PowerGridHaYTooL.exe [komut] [parametre]");
                Console.WriteLine();
                Console.WriteLine(" Komutlar:");
                Console.WriteLine("   -smart on          : Akıllı GPU Güç Anahtarını AÇAR (Uygulama kapalıysa arka planda başlatır).");
                Console.WriteLine("   -smart off         : Akıllı GPU Güç Anahtarını KAPATIR.");
                Console.WriteLine("   -smart toggle      : Açık ise kapatır, kapalı ise açar.");
                Console.WriteLine("   -smart status      : Uygulama ve akıllı güç anahtarı durumunu gösterir.");
                Console.WriteLine("   -apply {GUID}      : Belirtilen GUID'e sahip güç planına anında geçer.");
                Console.WriteLine("   -list              : Sistemdeki tüm güç düzenlerini listeler.");
                Console.WriteLine("   -exit / -close     : Arka planda çalışan uygulamayı tamamen kapatır.");
                Console.WriteLine("   -help /?           : Bu yardım kılavuzunu görüntüler.");
                Console.WriteLine("=======================================================");
                Console.WriteLine();
                ProcessHelper.DetachParentConsole();
                return true;
            }

            return false;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_instanceMutex != null)
            {
                try
                {
                    _instanceMutex.ReleaseMutex();
                }
                catch { }
                _instanceMutex.Dispose();
                _instanceMutex = null;
            }
            base.OnExit(e);
        }
    }
}

