using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using PowerGridHaYTooL.Services;

namespace PowerGridHaYTooL.UI
{
    public partial class ToastNotificationWindow : Window
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_TOPMOST = 0x00000008;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private DispatcherTimer? _closeTimer;
        private string? _clickUrl;
        private double _displayDurationSeconds;

        public ToastNotificationWindow(string title, string planName, string reason, string icon = "⚡", string? clickUrl = null, double displayDurationSeconds = 3.5)
        {
            InitializeComponent();
            ThemeService.ApplyTheme(ConfigService.CurrentTheme, this);

            _clickUrl = clickUrl;
            _displayDurationSeconds = displayDurationSeconds > 0 ? displayDurationSeconds : 3.5;

            TxtTitle.Text = title;
            TxtPlanName.Text = planName;
            TxtReason.Text = reason;
            TxtIcon.Text = icon;

            if (!string.IsNullOrEmpty(_clickUrl))
            {
                RootBorder.Cursor = System.Windows.Input.Cursors.Hand;
                RootBorder.ToolTip = ConfigService.CurrentLanguage == "English"
                    ? "Click to open release download page"
                    : "İndirme sayfasını açmak için tıklayın";
            }

            if (BtnClose != null)
            {
                BtnClose.ToolTip = ConfigService.CurrentLanguage == "English" ? "Close" : "Kapat";
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Win32 Non-Activating Window (Oyun oynarken veya çalışırken odağı kesinlikle çalmaz)
            var helper = new WindowInteropHelper(this);
            int exStyle = GetWindowLong(helper.Handle, GWL_EXSTYLE);
            SetWindowLong(helper.Handle, GWL_EXSTYLE, exStyle | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Görev çubuğunun hemen üstünde, saatin yanındaki köşeye konumlandır
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 12;
            Top = workArea.Bottom - Height - 12;

            // Giriş animasyonunu başlat
            if (Resources["FadeInStoryboard"] is Storyboard fadeIn)
            {
                fadeIn.Begin(this);
            }

            // Belirlenen süre sonra yumuşakça kaybol ve kapat
            _closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(_displayDurationSeconds) };
            _closeTimer.Tick += (s, args) =>
            {
                _closeTimer.Stop();
                FadeOutAndClose();
            };
            _closeTimer.Start();
        }

        private void RootBorder_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(_clickUrl))
            {
                PowerGridHaYTooL.Helpers.ProcessHelper.OpenUri(_clickUrl);
                _closeTimer?.Stop();
                FadeOutAndClose();
            }
        }

        private void FadeOutAndClose()
        {
            if (Resources["FadeOutStoryboard"] is Storyboard fadeOut)
            {
                fadeOut.Completed += (s, e) => Close();
                fadeOut.Begin(this);
            }
            else
            {
                Close();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            _closeTimer?.Stop();
            FadeOutAndClose();
        }

        public static void ShowToast(string title, string planName, string reason, string icon = "⚡", string? clickUrl = null, double displayDurationSeconds = 3.5)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var toast = new ToastNotificationWindow(title, planName, reason, icon, clickUrl, displayDurationSeconds);
                    toast.Show();
                }
                catch { }
            });
        }
    }
}
