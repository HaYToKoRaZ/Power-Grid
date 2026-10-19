using System;
using System.Windows;
using PowerGridHaYTooL.Services;

namespace PowerGridHaYTooL
{
    public partial class MainWindow
    {
        private void UpdateLanguageMenuChecks()
        {
            MnuLangAuto.IsChecked = string.Equals(ConfigService.LanguageSetting, "Auto", StringComparison.OrdinalIgnoreCase);
            MnuLangTurkish.IsChecked = string.Equals(ConfigService.LanguageSetting, "Turkish", StringComparison.OrdinalIgnoreCase);
            MnuLangEnglish.IsChecked = string.Equals(ConfigService.LanguageSetting, "English", StringComparison.OrdinalIgnoreCase);
        }

        private void UpdateThemeMenuChecks()
        {
            string cur = ConfigService.CurrentTheme ?? "Auto";
            MnuThemeAuto.IsChecked = string.Equals(cur, "Auto", StringComparison.OrdinalIgnoreCase);
            MnuThemeDark.IsChecked = string.Equals(cur, "Dark", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(cur, "SunsetBoulevard", StringComparison.OrdinalIgnoreCase);
            MnuThemeLight.IsChecked = string.Equals(cur, "Light", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(cur, "BotanicalGarden", StringComparison.OrdinalIgnoreCase);

            MnuThemeNordic.IsChecked = string.Equals(cur, "Nordic", StringComparison.OrdinalIgnoreCase);
            MnuThemeObsidian.IsChecked = string.Equals(cur, "Obsidian", StringComparison.OrdinalIgnoreCase) ||
                                         string.Equals(cur, "Black", StringComparison.OrdinalIgnoreCase);
            MnuThemeSilverBlue.IsChecked = string.Equals(cur, "SilverBlue", StringComparison.OrdinalIgnoreCase);

            MnuThemeOceanDepths.IsChecked = string.Equals(cur, "OceanDepths", StringComparison.OrdinalIgnoreCase);
            MnuThemeMidnightGalaxy.IsChecked = string.Equals(cur, "MidnightGalaxy", StringComparison.OrdinalIgnoreCase);
            MnuThemeTechInnovation.IsChecked = string.Equals(cur, "TechInnovation", StringComparison.OrdinalIgnoreCase);
            MnuThemeForestCanopy.IsChecked = string.Equals(cur, "ForestCanopy", StringComparison.OrdinalIgnoreCase);
            MnuThemeArcticFrost.IsChecked = string.Equals(cur, "ArcticFrost", StringComparison.OrdinalIgnoreCase);
            MnuThemeDesertRose.IsChecked = string.Equals(cur, "DesertRose", StringComparison.OrdinalIgnoreCase);
            MnuThemeGoldenHour.IsChecked = string.Equals(cur, "GoldenHour", StringComparison.OrdinalIgnoreCase);
            MnuThemeModernMinimalist.IsChecked = string.Equals(cur, "ModernMinimalist", StringComparison.OrdinalIgnoreCase);
        }

        private void ThemeAuto_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.CurrentTheme = "Auto";
            ConfigService.Save();
            ThemeService.ApplyTheme("Auto", this);
            UpdateThemeMenuChecks();
        }

        private void ThemeDark_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.CurrentTheme = "Dark";
            ConfigService.Save();
            ThemeService.ApplyTheme("Dark", this);
            UpdateThemeMenuChecks();
        }

        private void ThemeObsidian_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.CurrentTheme = "Obsidian";
            ConfigService.Save();
            ThemeService.ApplyTheme("Obsidian", this);
            UpdateThemeMenuChecks();
        }

        private void ThemeLight_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.CurrentTheme = "Light";
            ConfigService.Save();
            ThemeService.ApplyTheme("Light", this);
            UpdateThemeMenuChecks();
        }

        private void ThemeCustom_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is string themeName)
            {
                ConfigService.CurrentTheme = themeName;
                ConfigService.Save();
                ThemeService.ApplyTheme(themeName, this);
                UpdateThemeMenuChecks();
            }
        }

        private void LangAuto_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.LanguageSetting = "Auto";
            ConfigService.CurrentLanguage = ConfigService.DetectSystemLanguage();
            ConfigService.Save();
            ApplySettingsAndLanguage();
            RefreshData();
        }

        private void LangTurkish_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.LanguageSetting = "Turkish";
            ConfigService.CurrentLanguage = "Turkish";
            ConfigService.Save();
            ApplySettingsAndLanguage();
            RefreshData();
        }

        private void LangEnglish_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.LanguageSetting = "English";
            ConfigService.CurrentLanguage = "English";
            ConfigService.Save();
            ApplySettingsAndLanguage();
            RefreshData();
        }
    }
}
