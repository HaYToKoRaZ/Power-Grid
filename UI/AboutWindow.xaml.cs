using System;
using System.Reflection;
using System.Windows;
using PowerGridHaYTooL.Helpers;
using PowerGridHaYTooL.Services;

namespace PowerGridHaYTooL.UI
{
    public partial class AboutWindow : Window
    {
        public const string WebSiteUrl = "https://haytokoraz.github.io/Power-Grid/";
        public const string GithubUrl = "https://github.com/HaYToKoRaZ/Power-Grid/tree/main";
        public const string PortalUrl = "https://haytokoraz.github.io/";
        public const string ContactEmail = "korazhayto@gmail.com";

        public AboutWindow()
        {
            InitializeComponent();
            ThemeService.ApplyTheme(ConfigService.CurrentTheme, this);
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            bool isEnglish = ConfigService.CurrentLanguage == "English";

            Title = isEnglish ? "Power Grid HaYTooL - About" : "Power Grid HaYTooL - Hakkında";

            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            string verStr = ver != null ? $"v{ver.Major}.{ver.Minor}" : "v8.1";
            TxtVersionBadge.Text = verStr;

            if (isEnglish)
            {
                TxtSubtitle.Text = "HaYTooL Portal Official Desktop Power Manager";
                TxtDesc.Text = "A sleek, lightweight desktop utility for switching Windows power schemes in one click, desktop context menu integration, and smart GPU/process power optimization.";
                LblWebSite.Text = "Official Website";
                LblGithub.Text = "GitHub Repository (Open Source)";
                LblPortal.Text = "HaYTooL Portal";
                LblMail.Text = "Contact & Support";
                BtnOpenWebsite.Content = "Open ↗";
                BtnOpenGithub.Content = "Open ↗";
                BtnOpenPortal.Content = "Open ↗";
                BtnCopyEmail.Content = "📋 Copy";
                TxtFeedbackBtn.Text = "Send Feedback (Email)";
                BtnClose.Content = "Close";
            }
            else
            {
                TxtSubtitle.Text = "HaYTooL Portalı Resmi Masaüstü Güç Yöneticisi";
                TxtDesc.Text = "Windows güç planlarını tek tıkla değiştiren, masaüstü sağ tık entegrasyonu sunan ve akıllı GPU/süreç optimizasyonu sağlayan hafif masaüstü güç yöneticisi.";
                LblWebSite.Text = "Resmi Web Sitesi";
                LblGithub.Text = "GitHub Deposu (Açık Kaynak)";
                LblPortal.Text = "HaYTooL Portalı";
                LblMail.Text = "İletişim & Destek";
                BtnOpenWebsite.Content = "Aç ↗";
                BtnOpenGithub.Content = "Aç ↗";
                BtnOpenPortal.Content = "Aç ↗";
                BtnCopyEmail.Content = "📋 Kopyala";
                TxtFeedbackBtn.Text = "Geri Bildirim Gönder (E-Posta)";
                BtnClose.Content = "Kapat";
            }
        }

        private void BtnOpenWebsite_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.OpenUri(WebSiteUrl);
        }

        private void BtnOpenGithub_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.OpenUri(GithubUrl);
        }

        private void BtnOpenPortal_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.OpenUri(PortalUrl);
        }

        private void BtnCopyEmail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(ContactEmail);
                bool isEng = ConfigService.CurrentLanguage == "English";
                BtnCopyEmail.Content = isEng ? "✓ Copied!" : "✓ Kopyalandı!";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnSendFeedback_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ver = Assembly.GetExecutingAssembly().GetName().Version;
                string verStr = ver != null ? $"{ver.Major}.{ver.Minor}" : "8.1";
                string subject = Uri.EscapeDataString($"Power Grid HaYTooL v{verStr} Geri Bildirim");
                string body = Uri.EscapeDataString($"Merhaba,\n\nPower Grid HaYTooL v{verStr} uygulaması hakkındaki geri bildirimim:\n\n- Sistem: {Environment.OSVersion}\n\n");
                string mailto = $"mailto:{ContactEmail}?subject={subject}&body={body}";

                ProcessHelper.OpenUri(mailto);
            }
            catch
            {
                Clipboard.SetText(ContactEmail);
                bool isEng = ConfigService.CurrentLanguage == "English";
                MessageBox.Show(
                    isEng 
                        ? $"Failed to open mail client. Email address '{ContactEmail}' has been copied to your clipboard." 
                        : $"E-posta istemcisi açılamadı. '{ContactEmail}' adresi panoya kopyalandı.",
                    "Power Grid HaYTooL",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
