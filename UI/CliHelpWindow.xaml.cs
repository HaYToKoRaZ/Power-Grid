using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PowerGridHaYTooL.Models;
using PowerGridHaYTooL.Services;

namespace PowerGridHaYTooL.UI
{
    public partial class CliHelpWindow : Window
    {
        private List<PowerPlanItem> _plans = new();

        public CliHelpWindow()
        {
            InitializeComponent();
            ThemeService.ApplyTheme(ConfigService.CurrentTheme, this);
            ApplyLocalization();
            LoadPlansIntoComboBox();
        }

        private void ApplyLocalization()
        {
            bool isEn = ConfigService.CurrentLanguage == "English";
            if (isEn)
            {
                Title = "Command-Line Interface (CLI) Guide & Quick Copy";
                TxtHeaderTitle.Text = "Power Grid HaYTooL - CLI Command Guide";
                TxtHeaderSubtitle.Text = "One-click copy terminal commands for game launchers, Stream Deck macros, shortcuts, or scripts.";
                TxtQuickGenTitle.Text = "⚡ Quick Plan Switch Command Generator";
                BtnCopyQuickApply.Content = "📋 Copy Plan Command";
                TxtFooterHint.Text = "💡 Tip: You can prefix commands with '-' or '/' (e.g., -smart on or /smart on).";
                BtnClose.Content = "Close";
            }
        }

        private void LoadPlansIntoComboBox()
        {
            try
            {
                _plans = PowerManagerService.GetPowerPlans();
                CmbPlans.Items.Clear();

                foreach (var plan in _plans)
                {
                    string activeMarker = plan.IsActive ? " [ACTIVE]" : "";
                    CmbPlans.Items.Add($"{plan.PlanIcon} {plan.Name}{activeMarker} ({plan.Guid})");
                }

                int activeIndex = _plans.FindIndex(p => p.IsActive);
                CmbPlans.SelectedIndex = activeIndex >= 0 ? activeIndex : 0;
            }
            catch { }
        }

        private void CmbPlans_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Selection updated
        }

        private async void BtnCopyQuickApply_Click(object sender, RoutedEventArgs e)
        {
            if (CmbPlans.SelectedIndex < 0 || CmbPlans.SelectedIndex >= _plans.Count) return;

            var selectedPlan = _plans[CmbPlans.SelectedIndex];
            string cmd = $"PowerGridHaYTooL.exe -apply {selectedPlan.Guid}";

            try
            {
                Clipboard.SetText(cmd);
                string original = BtnCopyQuickApply.Content?.ToString() ?? "📋 Copy";
                bool isEn = ConfigService.CurrentLanguage == "English";
                BtnCopyQuickApply.Content = isEn ? "✓ Copied to Clipboard!" : "✓ Panoya Kopyalandı!";
                await Task.Delay(1400);
                BtnCopyQuickApply.Content = original;
            }
            catch { }
        }

        private async void BtnCopyCommand_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string cmdText && !string.IsNullOrEmpty(cmdText))
            {
                try
                {
                    Clipboard.SetText(cmdText);
                    string original = btn.Content?.ToString() ?? "📋 Kopyala";
                    bool isEn = ConfigService.CurrentLanguage == "English";
                    btn.Content = isEn ? "✓ Copied!" : "✓ Kopyalandı!";
                    await Task.Delay(1400);
                    btn.Content = original;
                }
                catch { }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
