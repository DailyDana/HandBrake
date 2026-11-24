// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThemeModeHelper.cs" company="HandBrake Project (https://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Fork: tema modunun koyu olup olmadığına tek yerden karar verir (simgeler, başlık çubuğu, mesaj kutusu).
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrakeWPF.Themes
{
    using HandBrakeWPF.Model;
    using HandBrakeWPF.Utilities;

    public static class ThemeModeHelper
    {
        public static bool IsDark(AppThemeMode mode)
        {
            switch (mode)
            {
                case AppThemeMode.Dark:
                case AppThemeMode.NordicDark:
                    return true;
                case AppThemeMode.System:
                case AppThemeMode.NordicSystem:
                    return SystemInfo.IsAppsUsingDarkTheme();
                default:
                    return false;
            }
        }

        public static bool IsNordic(AppThemeMode mode)
        {
            return mode == AppThemeMode.NordicLight || mode == AppThemeMode.NordicDark || mode == AppThemeMode.NordicSystem;
        }
    }
}
