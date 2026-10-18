using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using PowerGridHaYTooL.Helpers;
using PowerGridHaYTooL.Models;
using PowerGridHaYTooL.Services;

namespace PowerGridHaYTooL.UI
{
    public partial class RestoreBackupWindow : Window
    {
        private List<AutoBackupItem> _backups = new();

        public RestoreBackupWindow()
        {
            InitializeComponent();
            ThemeService.ApplyTheme(ConfigService.CurrentTheme, this);
            ApplyLanguage();
            LoadBackups();
        }

        private void ApplyLanguage()
        {
            bool isEng = ConfigService.CurrentLanguage == "English";
            if (isEng)
            {
                Title = "Restore Automatic ZIP Backups";
                TxtHeaderTitle.Text = "Restore Automatic ZIP Backups";
                TxtHeaderDesc.Text = "Select any automatic power plan backup created by the application to restore it in one click.";
                BtnBrowseZip.Content = "📁 Browse Other .ZIP File...";
                BtnRestoreSelected.Content = "🔄 Restore Selected";
                BtnClose.Content = "Close";
                TxtBackupDetails.Text = "Select a backup archive to preview the power plans contained inside.";
            }
        }

        private void LoadBackups()
        {
            _backups = AutoBackupService.GetAutoBackups();
            LstBackups.ItemsSource = null;
            LstBackups.ItemsSource = _backups;

            if (_backups.Count > 0)
            {
                LstBackups.SelectedIndex = 0;
            }
            else
            {
                bool isEng = ConfigService.CurrentLanguage == "English";
                TxtBackupDetails.Text = isEng 
                    ? "No automatic backups found yet. You can use 'Browse Other .ZIP File...' to import an external backup."
                    : "Henüz kayıtlı otomatik yedek bulunamadı. 'Başka Bir .ZIP Dosyası Seç...' butonunu kullanarak harici bir yedeği de geri yükleyebilirsiniz.";
            }
        }

        private void LstBackups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstBackups.SelectedItem is AutoBackupItem item)
            {
                TxtBackupDetails.Text = string.IsNullOrWhiteSpace(item.Details)
                    ? (ConfigService.CurrentLanguage == "English" ? "No detail summary found in this backup." : "Bu yedek arşivinde detay raporu bulunamadı.")
                    : item.Details;
            }
        }

        private void LstBackups_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BtnRestoreSelected_Click(sender, e);
        }

        private void BtnRestoreSelected_Click(object sender, RoutedEventArgs e)
        {
            if (LstBackups.SelectedItem is not AutoBackupItem item)
            {
                string msg = ConfigService.CurrentLanguage == "English" 
                    ? "Please select a backup archive from the list." 
                    : "Lütfen listeden bir yedek arşivi seçin.";
                MessageBox.Show(msg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            PerformRestore(item.FilePath, item.FileName);
        }

        private void BtnBrowseZip_Click(object sender, RoutedEventArgs e)
        {
            bool isEng = ConfigService.CurrentLanguage == "English";
            var ofd = new OpenFileDialog
            {
                Title = isEng ? "Select Power Plans ZIP Backup" : "Güç Düzenleri ZIP Yedeği Seçin",
                Filter = isEng ? "ZIP Backup Archives (*.zip)|*.zip" : "ZIP Yedek Arşivleri (*.zip)|*.zip"
            };

            if (ofd.ShowDialog() == true)
            {
                PerformRestore(ofd.FileName, Path.GetFileName(ofd.FileName));
            }
        }

        private void PerformRestore(string zipPath, string displayName)
        {
            bool isEng = ConfigService.CurrentLanguage == "English";
            string confirmMsg = isEng
                ? $"Are you sure you want to restore the power plans from:\n'{displayName}'?"
                : $"'{displayName}' yedeğindeki güç düzenlerini sisteme geri yüklemek istediğinizden emin misiniz?";

            var confirm = MessageBox.Show(confirmMsg, "Power Grid HaYTooL", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            var result = AutoBackupService.RestoreFromZip(zipPath);

            if (result.SuccessCount > 0)
            {
                string msg = isEng 
                    ? $"Successfully restored {result.SuccessCount} power plan(s) from backup!"
                    : $"{result.SuccessCount} adet güç düzeni yedeğinden başarıyla geri yüklendi!";
                
                MessageBox.Show(msg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                string msg = isEng
                    ? $"Failed to restore power plans from the backup.\n{result.Message}"
                    : $"Yedekten geri yükleme yapılamadı.\n{result.Message}";
                MessageBox.Show(msg, "Power Grid HaYTooL", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string dir = AutoBackupService.GetBackupDirectory();
                ProcessHelper.OpenUri(dir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Klasör açılamadı: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
