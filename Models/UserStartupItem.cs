using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PowerGridHaYTooL.Models
{
    public class UserStartupItem : INotifyPropertyChanged
    {
        private string? _selectedPlanGuid;

        public string UserName { get; set; } = string.Empty;
        public string ProfilePath { get; set; } = string.Empty;
        public bool IsCurrentUser { get; set; }
        public bool HasAccess { get; set; } = true;

        public string DisplayName => IsCurrentUser ? $"{UserName} (Şu Anki Oturum)" : UserName;
        public string UserIcon => IsCurrentUser ? "⭐ 👤" : "👤";

        public string? InitialPlanGuid { get; set; }

        public string? SelectedPlanGuid
        {
            get => _selectedPlanGuid;
            set
            {
                if (_selectedPlanGuid != value)
                {
                    _selectedPlanGuid = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsModified));
                }
            }
        }

        public string StartupFilePath => System.IO.Path.Combine(
            ProfilePath,
            @"AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\PowerGrid_StartupPlan.cmd");

        public bool IsModified => !string.Equals(InitialPlanGuid ?? "", SelectedPlanGuid ?? "", StringComparison.OrdinalIgnoreCase);

        public System.Windows.Visibility ModifiedBadgeVisibility => IsModified ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            if (propName == nameof(SelectedPlanGuid))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModifiedBadgeVisibility)));
            }
        }
    }
}
