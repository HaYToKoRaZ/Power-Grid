namespace PowerGridHaYTooL.Models
{
    public class PowerPlanItem
    {
        public string Guid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsUserStartup { get; set; }

        public string PlanIcon
        {
            get
            {
                string g = (Guid ?? string.Empty).ToLowerInvariant();
                if (g == "7a1f0001-c001-4a11-b001-000000000001") return "⚡";
                if (g == "7a1f0002-c002-4a11-b002-000000000002") return "⚖️";
                if (g == "7a1f0003-c003-4a11-b003-000000000003") return "🌱";
                if (g == "e9a42b02-d5df-448d-aa00-03f14749eb00" || g == "e9a42b02-d5df-448d-aa00-03f14749eb61") return "⚡";
                if (g == "9897998c-92de-4669-853f-b7cd3ecb2790") return "🔴";
                if (g == "9897998c-92de-4669-853f-b7cd3ecb2791") return "🚀";
                if (g == "381b4222-f694-41f0-9685-ff5bb260df2e") return "⚖️";
                if (g == "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c") return "🚀";
                if (g == "a1841308-3541-4fab-bc81-f71556f20b4a") return "🍃";

                string n = (Name ?? string.Empty).ToLowerInvariant();
                if (n.Contains("apex")) return "⚡";
                if (n.Contains("stealth") || n.Contains("eco")) return "🌱";
                if (n.Contains("nihai") || n.Contains("ultimate")) return "⚡";
                if (n.Contains("amd") && (n.Contains("dengeli") || n.Contains("balanced"))) return "🔴";
                if (n.Contains("amd") && (n.Contains("performans") || n.Contains("performance") || n.Contains("high"))) return "🚀";
                if (n.Contains("tasarruf") || n.Contains("saver") || n.Contains("energy")) return "🍃";
                if (n.Contains("oyun") || n.Contains("game") || n.Contains("gaming")) return "🎮";
                if (n.Contains("yüksek") || n.Contains("high") || n.Contains("maksimum") || n.Contains("max")) return "🚀";
                if (n.Contains("dengeli") || n.Contains("balanced") || n.Contains("standart") || n.Contains("default")) return "⚖️";
                if (n.Contains("sessiz") || n.Contains("quiet") || n.Contains("silent")) return "🌙";
                if (n.Contains("turbo") || n.Contains("boost")) return "⚡";

                return "⚡";
            }
        }

        public string DisplayName => Name;

        public string TagText
        {
            get
            {
                bool isEng = string.Equals(Services.ConfigService.CurrentLanguage, "English", System.StringComparison.OrdinalIgnoreCase);
                if (IsUserStartup)
                    return isEng ? "📌 Startup Plan" : "📌 Açılış Planı";

                string g = (Guid ?? string.Empty).ToLowerInvariant();
                if (g.StartsWith("7a1f"))
                    return "HaYTooL";
                if (g.StartsWith("9897"))
                    return "AMD Ryzen™";
                if (g == "e9a42b02-d5df-448d-aa00-03f14749eb00" || g == "e9a42b02-d5df-448d-aa00-03f14749eb61")
                    return isEng ? "Ultimate" : "Nihai";
                if (g == "381b4222-f694-41f0-9685-ff5bb260df2e" || g == "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c" || g == "a1841308-3541-4fab-bc81-f71556f20b4a")
                    return isEng ? "Windows Default" : "Windows Varsayılanı";

                return isEng ? "Custom Plan" : "Özel Plan";
            }
        }

        public string StatusBadgeText
        {
            get
            {
                if (!IsActive) return string.Empty;
                bool isEng = string.Equals(Services.ConfigService.CurrentLanguage, "English", System.StringComparison.OrdinalIgnoreCase);
                return isEng ? "ACTIVE" : "AKTİF";
            }
        }

        public string FontBoldWeight => IsActive ? "Bold" : "SemiBold";

        public string Description => $"GUID: {Guid}";
    }
}
