using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace PowerGridHaYTooL.Services
{
    public static class ThemeService
    {
        public static string CurrentTheme { get; private set; } = "Auto";

        public static bool DetectWindowsIsLight()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if (key != null)
                {
                    object? val = key.GetValue("AppsUseLightTheme");
                    if (val is int intVal)
                    {
                        return intVal == 1;
                    }
                }
            }
            catch { }
            return true; // Varsayılan açık
        }

        public static bool IsCurrentlyDark()
        {
            if (string.Equals(CurrentTheme, "Auto", StringComparison.OrdinalIgnoreCase))
            {
                return !DetectWindowsIsLight();
            }

            if (string.Equals(CurrentTheme, "Light", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(CurrentTheme, "BotanicalGarden", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(CurrentTheme, "SilverBlue", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(CurrentTheme, "ArcticFrost", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public static void ApplyTheme(string themeName, Window? window = null)
        {
            CurrentTheme = themeName;
            string mode = themeName;

            if (string.Equals(themeName, "Auto", StringComparison.OrdinalIgnoreCase))
            {
                mode = DetectWindowsIsLight() ? "BotanicalGarden" : "SunsetBoulevard";
            }
            else if (string.Equals(themeName, "Dark", StringComparison.OrdinalIgnoreCase))
            {
                mode = "SunsetBoulevard";
            }
            else if (string.Equals(themeName, "Light", StringComparison.OrdinalIgnoreCase))
            {
                mode = "BotanicalGarden";
            }
            else if (string.Equals(themeName, "Black", StringComparison.OrdinalIgnoreCase))
            {
                mode = "Obsidian";
            }

            ResourceDictionary targetDict = Application.Current?.Resources ?? new ResourceDictionary();

            switch (mode)
            {
                case "BotanicalGarden":
                    // 🌿 Varsayılan Açık Tema: Botanik Bahçesi (Botanical Garden)
                    SetPalette(targetDict,
                        Color.FromRgb(242, 244, 237), Color.FromRgb(255, 255, 255),
                        Color.FromRgb(230, 234, 223), Color.FromRgb(255, 255, 255), Color.FromRgb(220, 226, 212),
                        Color.FromRgb(255, 255, 255), Color.FromRgb(237, 241, 231), Color.FromRgb(223, 230, 215),
                        Color.FromRgb(234, 240, 227), Color.FromRgb(204, 214, 194), Color.FromRgb(204, 214, 194),
                        Color.FromRgb(230, 234, 223), Color.FromRgb(255, 255, 255), Color.FromRgb(185, 199, 172), Color.FromRgb(239, 243, 235),
                        Color.FromRgb(255, 255, 255), Color.FromRgb(31, 46, 35),
                        Color.FromRgb(31, 46, 35), Color.FromRgb(74, 124, 89),
                        Color.FromRgb(45, 106, 79));
                    break;

                case "Nordic":
                    // 🌌 Nordic Gece Mavisi (Slate Navy)
                    SetPalette(targetDict,
                        Color.FromRgb(22, 27, 38), Color.FromRgb(30, 37, 54),
                        Color.FromRgb(26, 32, 46), Color.FromRgb(30, 37, 54), Color.FromRgb(45, 57, 82),
                        Color.FromRgb(20, 24, 34), Color.FromRgb(34, 43, 61), Color.FromRgb(42, 54, 78),
                        Color.FromRgb(26, 32, 46), Color.FromRgb(45, 55, 77), Color.FromRgb(45, 55, 77),
                        Color.FromRgb(24, 30, 42), Color.FromRgb(34, 43, 61), Color.FromRgb(51, 65, 85), Color.FromRgb(46, 58, 82),
                        Color.FromRgb(24, 30, 43), Color.FromRgb(248, 250, 252),
                        Color.FromRgb(248, 250, 252), Color.FromRgb(148, 163, 184),
                        Color.FromRgb(0, 210, 255));
                    break;

                case "Obsidian":
                    // ⬛ Obsidyen Siyah (Mevcut Koyu Tema)
                    SetPalette(targetDict,
                        Color.FromRgb(18, 20, 26), Color.FromRgb(24, 28, 38),
                        Color.FromRgb(22, 25, 34), Color.FromRgb(26, 30, 42), Color.FromRgb(40, 48, 66),
                        Color.FromRgb(16, 18, 24), Color.FromRgb(30, 36, 50), Color.FromRgb(38, 48, 68),
                        Color.FromRgb(24, 28, 38), Color.FromRgb(42, 48, 65), Color.FromRgb(42, 48, 65),
                        Color.FromRgb(22, 25, 34), Color.FromRgb(28, 33, 46), Color.FromRgb(45, 54, 75), Color.FromRgb(42, 52, 74),
                        Color.FromRgb(26, 30, 42), Color.FromRgb(248, 250, 252),
                        Color.FromRgb(248, 250, 252), Color.FromRgb(148, 163, 184),
                        Color.FromRgb(0, 242, 254));
                    break;

                case "SilverBlue":
                    // ☀️ Gümüş Mavi (Klasik Açık Tema)
                    SetPalette(targetDict,
                        Color.FromRgb(243, 244, 246), Color.FromRgb(255, 255, 255),
                        Color.FromRgb(255, 255, 255), Color.FromRgb(255, 255, 255), Color.FromRgb(241, 245, 249),
                        Color.FromRgb(255, 255, 255), Color.FromRgb(241, 245, 249), Color.FromRgb(226, 232, 240),
                        Color.FromRgb(248, 250, 252), Color.FromRgb(226, 232, 240), Color.FromRgb(226, 232, 240),
                        Color.FromRgb(248, 250, 252), Color.FromRgb(255, 255, 255), Color.FromRgb(203, 213, 225), Color.FromRgb(241, 245, 249),
                        Color.FromRgb(255, 255, 255), Color.FromRgb(17, 24, 39),
                        Color.FromRgb(17, 24, 39), Color.FromRgb(100, 116, 139),
                        Color.FromRgb(2, 132, 199));
                    break;

                case "OceanDepths":
                    // 🌊 Theme Factory: Ocean Depths
                    SetPalette(targetDict,
                        Color.FromRgb(26, 35, 50), Color.FromRgb(34, 45, 64),
                        Color.FromRgb(23, 31, 44), Color.FromRgb(34, 45, 64), Color.FromRgb(45, 59, 84),
                        Color.FromRgb(20, 28, 40), Color.FromRgb(38, 51, 72), Color.FromRgb(46, 62, 88),
                        Color.FromRgb(28, 37, 53), Color.FromRgb(46, 60, 82), Color.FromRgb(46, 60, 82),
                        Color.FromRgb(23, 31, 44), Color.FromRgb(34, 45, 64), Color.FromRgb(58, 75, 102), Color.FromRgb(44, 58, 82),
                        Color.FromRgb(26, 35, 50), Color.FromRgb(241, 250, 238),
                        Color.FromRgb(241, 250, 238), Color.FromRgb(168, 218, 220),
                        Color.FromRgb(45, 139, 139));
                    break;

                case "MidnightGalaxy":
                    // ✨ Theme Factory: Midnight Galaxy
                    SetPalette(targetDict,
                        Color.FromRgb(29, 20, 43), Color.FromRgb(40, 29, 60),
                        Color.FromRgb(23, 16, 34), Color.FromRgb(40, 29, 60), Color.FromRgb(56, 42, 82),
                        Color.FromRgb(22, 15, 33), Color.FromRgb(50, 36, 76), Color.FromRgb(66, 47, 100),
                        Color.FromRgb(32, 22, 48), Color.FromRgb(62, 43, 92), Color.FromRgb(62, 43, 92),
                        Color.FromRgb(23, 16, 34), Color.FromRgb(43, 30, 62), Color.FromRgb(74, 53, 106), Color.FromRgb(57, 40, 82),
                        Color.FromRgb(29, 20, 43), Color.FromRgb(230, 230, 250),
                        Color.FromRgb(244, 240, 251), Color.FromRgb(184, 169, 212),
                        Color.FromRgb(164, 144, 194));
                    break;

                case "TechInnovation":
                    // ⚡ Theme Factory: Tech Innovation
                    SetPalette(targetDict,
                        Color.FromRgb(18, 22, 30), Color.FromRgb(26, 32, 44),
                        Color.FromRgb(15, 18, 24), Color.FromRgb(26, 32, 44), Color.FromRgb(38, 48, 66),
                        Color.FromRgb(14, 17, 23), Color.FromRgb(34, 43, 59), Color.FromRgb(45, 58, 80),
                        Color.FromRgb(21, 27, 37), Color.FromRgb(42, 53, 71), Color.FromRgb(42, 53, 71),
                        Color.FromRgb(15, 18, 24), Color.FromRgb(26, 32, 44), Color.FromRgb(0, 102, 255), Color.FromRgb(38, 50, 70),
                        Color.FromRgb(18, 22, 30), Color.FromRgb(255, 255, 255),
                        Color.FromRgb(255, 255, 255), Color.FromRgb(136, 153, 176),
                        Color.FromRgb(0, 255, 255));
                    break;

                case "ForestCanopy":
                    // 🌲 Theme Factory: Forest Canopy
                    SetPalette(targetDict,
                        Color.FromRgb(26, 39, 27), Color.FromRgb(35, 53, 36),
                        Color.FromRgb(21, 32, 22), Color.FromRgb(35, 53, 36), Color.FromRgb(48, 72, 50),
                        Color.FromRgb(19, 30, 20), Color.FromRgb(42, 64, 44), Color.FromRgb(53, 82, 56),
                        Color.FromRgb(29, 44, 30), Color.FromRgb(53, 79, 55), Color.FromRgb(53, 79, 55),
                        Color.FromRgb(21, 32, 22), Color.FromRgb(35, 53, 36), Color.FromRgb(61, 91, 63), Color.FromRgb(46, 70, 48),
                        Color.FromRgb(26, 39, 27), Color.FromRgb(250, 249, 246),
                        Color.FromRgb(250, 249, 246), Color.FromRgb(164, 172, 134),
                        Color.FromRgb(82, 183, 136));
                    break;

                case "ArcticFrost":
                    // ❄️ Theme Factory: Arctic Frost (Light)
                    SetPalette(targetDict,
                        Color.FromRgb(237, 242, 249), Color.FromRgb(255, 255, 255),
                        Color.FromRgb(226, 234, 244), Color.FromRgb(255, 255, 255), Color.FromRgb(216, 228, 242),
                        Color.FromRgb(255, 255, 255), Color.FromRgb(230, 239, 248), Color.FromRgb(212, 227, 243),
                        Color.FromRgb(230, 238, 247), Color.FromRgb(203, 220, 239), Color.FromRgb(203, 220, 239),
                        Color.FromRgb(226, 234, 244), Color.FromRgb(255, 255, 255), Color.FromRgb(181, 206, 234), Color.FromRgb(234, 241, 249),
                        Color.FromRgb(255, 255, 255), Color.FromRgb(27, 42, 61),
                        Color.FromRgb(27, 42, 61), Color.FromRgb(74, 111, 165),
                        Color.FromRgb(43, 108, 176));
                    break;

                case "DesertRose":
                    // 🌹 Theme Factory: Desert Rose
                    SetPalette(targetDict,
                        Color.FromRgb(42, 27, 34), Color.FromRgb(54, 35, 44),
                        Color.FromRgb(33, 21, 27), Color.FromRgb(54, 35, 44), Color.FromRgb(74, 48, 60),
                        Color.FromRgb(28, 18, 23), Color.FromRgb(63, 41, 52), Color.FromRgb(78, 51, 64),
                        Color.FromRgb(46, 29, 37), Color.FromRgb(78, 51, 64), Color.FromRgb(78, 51, 64),
                        Color.FromRgb(33, 21, 27), Color.FromRgb(54, 35, 44), Color.FromRgb(90, 60, 74), Color.FromRgb(67, 44, 55),
                        Color.FromRgb(42, 27, 34), Color.FromRgb(255, 240, 243),
                        Color.FromRgb(255, 240, 243), Color.FromRgb(212, 165, 165),
                        Color.FromRgb(226, 132, 144));
                    break;

                case "GoldenHour":
                    // 🍁 Theme Factory: Golden Hour
                    SetPalette(targetDict,
                        Color.FromRgb(36, 30, 26), Color.FromRgb(48, 40, 35),
                        Color.FromRgb(28, 23, 20), Color.FromRgb(48, 40, 35), Color.FromRgb(66, 55, 48),
                        Color.FromRgb(24, 20, 17), Color.FromRgb(57, 47, 41), Color.FromRgb(73, 60, 53),
                        Color.FromRgb(39, 32, 28), Color.FromRgb(71, 58, 51), Color.FromRgb(71, 58, 51),
                        Color.FromRgb(28, 23, 20), Color.FromRgb(48, 40, 35), Color.FromRgb(87, 71, 63), Color.FromRgb(59, 49, 43),
                        Color.FromRgb(36, 30, 26), Color.FromRgb(255, 248, 240),
                        Color.FromRgb(255, 248, 240), Color.FromRgb(212, 184, 150),
                        Color.FromRgb(244, 169, 0));
                    break;

                case "ModernMinimalist":
                    // 🏢 Theme Factory: Modern Minimalist
                    SetPalette(targetDict,
                        Color.FromRgb(31, 34, 40), Color.FromRgb(41, 45, 53),
                        Color.FromRgb(24, 26, 31), Color.FromRgb(41, 45, 53), Color.FromRgb(56, 61, 72),
                        Color.FromRgb(21, 23, 26), Color.FromRgb(49, 54, 64), Color.FromRgb(62, 68, 80),
                        Color.FromRgb(33, 36, 43), Color.FromRgb(62, 68, 80), Color.FromRgb(62, 68, 80),
                        Color.FromRgb(24, 26, 31), Color.FromRgb(41, 45, 53), Color.FromRgb(74, 82, 96), Color.FromRgb(53, 59, 69),
                        Color.FromRgb(31, 34, 40), Color.FromRgb(255, 255, 255),
                        Color.FromRgb(255, 255, 255), Color.FromRgb(160, 174, 192),
                        Color.FromRgb(148, 163, 184));
                    break;

                case "SunsetBoulevard":
                default:
                    // 🌅 Varsayılan Koyu Tema: Gün Batımı (Sunset Boulevard)
                    SetPalette(targetDict,
                        Color.FromRgb(30, 36, 43), Color.FromRgb(38, 46, 55),
                        Color.FromRgb(24, 29, 35), Color.FromRgb(38, 46, 55), Color.FromRgb(53, 63, 75),
                        Color.FromRgb(21, 25, 30), Color.FromRgb(47, 57, 68), Color.FromRgb(59, 72, 86),
                        Color.FromRgb(32, 39, 46), Color.FromRgb(60, 72, 86), Color.FromRgb(60, 72, 86),
                        Color.FromRgb(24, 29, 35), Color.FromRgb(38, 46, 55), Color.FromRgb(74, 88, 104), Color.FromRgb(51, 62, 74),
                        Color.FromRgb(30, 36, 43), Color.FromRgb(255, 245, 235),
                        Color.FromRgb(255, 245, 235), Color.FromRgb(244, 162, 97),
                        Color.FromRgb(231, 111, 81));
                    break;
            }

            if (window != null)
            {
                window.Background = (Brush)targetDict["BrushWindowBg"];
                window.Foreground = (Brush)targetDict["BrushTextPrimary"];
                foreach (System.Collections.DictionaryEntry entry in targetDict)
                {
                    window.Resources[entry.Key] = entry.Value;
                }
            }
        }

        private static void SetPalette(
            ResourceDictionary dict,
            Color windowBg,
            Color cardBg,
            Color menuBg,
            Color menuPopupBg,
            Color menuItemHover,
            Color listBg,
            Color listItemHover,
            Color listItemSelected,
            Color headerBg,
            Color headerBorder,
            Color border,
            Color statusBg,
            Color btnBg,
            Color btnBorder,
            Color btnHover,
            Color inputBg,
            Color inputText,
            Color textPrimary,
            Color textSecondary,
            Color accent)
        {
            dict["BrushWindowBg"] = new SolidColorBrush(windowBg);
            dict["BrushCardBg"] = new SolidColorBrush(cardBg);
            dict["BrushMenuBg"] = new SolidColorBrush(menuBg);
            dict["BrushMenuPopupBg"] = new SolidColorBrush(menuPopupBg);
            dict["BrushMenuItemHover"] = new SolidColorBrush(menuItemHover);
            dict["BrushListBg"] = new SolidColorBrush(listBg);
            dict["BrushListItemHover"] = new SolidColorBrush(listItemHover);
            dict["BrushListItemSelected"] = new SolidColorBrush(listItemSelected);
            dict["BrushHeaderBg"] = new SolidColorBrush(headerBg);
            dict["BrushHeaderBorder"] = new SolidColorBrush(headerBorder);
            dict["BrushBorder"] = new SolidColorBrush(border);
            dict["BrushStatusBg"] = new SolidColorBrush(statusBg);
            dict["BrushBtnBg"] = new SolidColorBrush(btnBg);
            dict["BrushBtnBorder"] = new SolidColorBrush(btnBorder);
            dict["BrushBtnHover"] = new SolidColorBrush(btnHover);
            dict["BrushInputBg"] = new SolidColorBrush(inputBg);
            dict["BrushInputText"] = new SolidColorBrush(inputText);
            dict["BrushTextPrimary"] = new SolidColorBrush(textPrimary);
            dict["BrushTextSecondary"] = new SolidColorBrush(textSecondary);
            dict["BrushAccent"] = new SolidColorBrush(accent);
            dict["BrushAccentGlow"] = new SolidColorBrush(Color.FromArgb(40, accent.R, accent.G, accent.B));
        }
    }
}
