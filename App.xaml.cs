using System;
using System.Threading;
using System.Windows;
using PowerGridHaYTooL.Helpers;
using PowerGridHaYTooL.Services;

namespace PowerGridHaYTooL
{
    public partial class App : Application
    {
        private static Mutex? _instanceMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Command Line Argument Handling (e.g. -apply {GUID} or /setactive {GUID})
            // Allows instant, completely silent background plan activation on user login
            if (e.Args.Length >= 2 &&
                (e.Args[0].Equals("-apply", StringComparison.OrdinalIgnoreCase) ||
                 e.Args[0].Equals("/apply", StringComparison.OrdinalIgnoreCase) ||
                 e.Args[0].Equals("-setactive", StringComparison.OrdinalIgnoreCase) ||
                 e.Args[0].Equals("/setactive", StringComparison.OrdinalIgnoreCase)))
            {
                string targetGuid = e.Args[1].Trim();
                try
                {
                    PowerManagerService.SetActivePlan(targetGuid, false);
                }
                catch { }
                Shutdown(0);
                return;
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

