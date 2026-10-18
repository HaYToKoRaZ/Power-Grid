using System;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using PowerGridHaYTooL.Helpers;
using PowerGridHaYTooL.UI;

namespace PowerGridHaYTooL.Services
{
    public static class UpdateCheckService
    {
        private const string GitHubApiUrl = "https://api.github.com/repos/HaYToKoRaZ/Power-Grid/releases/latest";
        private static bool _alreadyCheckedThisSession = false;

        public static void CheckForUpdatesAsync(bool isManual = false)
        {
            if (!isManual && _alreadyCheckedThisSession)
            {
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(6);
                    client.DefaultRequestHeaders.Add("User-Agent", "PowerGridHaYTooL");
                    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

                    var response = await client.GetAsync(GitHubApiUrl).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        if (isManual)
                        {
                            ShowUpToDateOrError(isError: true);
                        }
                        return;
                    }

                    string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    string tagName = root.TryGetProperty("tag_name", out var tagElem) ? tagElem.GetString() ?? "" : "";
                    string htmlUrl = root.TryGetProperty("html_url", out var urlElem) ? urlElem.GetString() ?? "" : "";
                    string releaseTitle = root.TryGetProperty("name", out var nameElem) ? nameElem.GetString() ?? "" : "";

                    if (string.IsNullOrWhiteSpace(htmlUrl))
                    {
                        htmlUrl = "https://github.com/HaYToKoRaZ/Power-Grid/releases/latest";
                    }

                    var currentVer = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0, 0);

                    if (IsNewerVersion(tagName, currentVer))
                    {
                        _alreadyCheckedThisSession = true;

                        bool isTr = ConfigService.CurrentLanguage != "English";
                        string toastTitle = isTr ? "🎉 Yeni Sürüm Hazır!" : "🎉 New Version Available!";
                        string toastPlan = string.IsNullOrWhiteSpace(releaseTitle) 
                            ? $"Power Grid HaYTooL {tagName}" 
                            : releaseTitle;
                        string toastReason = isTr ? "İndirmek için buraya tıklayın 🚀" : "Click here to download 🚀";

                        try
                        {
                            System.Media.SystemSounds.Asterisk.Play();
                        }
                        catch { }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                ToastNotificationWindow.ShowToast(
                                    title: toastTitle,
                                    planName: toastPlan,
                                    reason: toastReason,
                                    icon: "🚀",
                                    clickUrl: htmlUrl,
                                    displayDurationSeconds: 8.0
                                );
                            }
                            catch { }
                        });
                    }
                    else if (isManual)
                    {
                        ShowUpToDateOrError(isError: false);
                    }
                }
                catch
                {
                    if (isManual)
                    {
                        ShowUpToDateOrError(isError: true);
                    }
                }
            });
        }

        public static bool IsNewerVersion(string remoteTag, Version currentVer)
        {
            if (string.IsNullOrWhiteSpace(remoteTag)) return false;

            string clean = remoteTag.Trim().TrimStart('v', 'V').Trim();
            var parts = clean.Split('.');
            if (parts.Length >= 2 && int.TryParse(parts[0], out int rMajor) && int.TryParse(parts[1], out int rMinor))
            {
                if (rMajor > currentVer.Major) return true;
                if (rMajor < currentVer.Major) return false;

                if (rMinor > currentVer.Minor) return true;
                if (rMinor < currentVer.Minor) return false;

                if (parts.Length >= 3 && int.TryParse(parts[2], out int rBuild))
                {
                    int cBuild = Math.Max(0, currentVer.Build);
                    if (rBuild > cBuild) return true;
                    if (rBuild < cBuild) return false;
                }

                return false;
            }

            return false;
        }

        private static void ShowUpToDateOrError(bool isError)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                bool isTr = ConfigService.CurrentLanguage != "English";
                if (isError)
                {
                    MessageBox.Show(
                        isTr ? "Güncelleme kontrolü sırasında sunucuya ulaşılamadı." : "Unable to reach update server.",
                        "Power Grid HaYTooL",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    var ver = Assembly.GetExecutingAssembly().GetName().Version;
                    string verStr = ver != null ? $"{ver.Major}.{ver.Minor}" : "1.0";
                    MessageBox.Show(
                        isTr ? $"En güncel sürümü kullanıyorsunuz (v{verStr})." : $"You are using the latest version (v{verStr}).",
                        "Power Grid HaYTooL",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            });
        }
    }
}
