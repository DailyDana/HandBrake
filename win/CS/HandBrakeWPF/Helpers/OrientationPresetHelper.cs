// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrientationPresetHelper.cs" company="HandBrake Project (https://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Fork: dikey / yatay videolara ayrı preset seçimi.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrakeWPF.Helpers
{
    public static class OrientationPresetHelper
    {
        /// <summary>
        /// Videonun ekranda dikey görünüp görünmediği. libhb, döndürme bilgisi 90/270 olan videolarda
        /// genişlik ve yüksekliği zaten yer değiştirerek verir; burada ayrıca piksel en-boy oranı (PAR)
        /// hesaba katılır. Kare videolar yatay sayılır.
        /// </summary>
        public static bool IsVertical(int width, int height, int parNum, int parDen)
        {
            if (width <= 0 || height <= 0)
            {
                return false;
            }

            double par = (parNum > 0 && parDen > 0) ? (double)parNum / parDen : 1.0;
            double displayWidth = width * par;
            return height > displayWidth;
        }

        public static string Label(bool enabled)
        {
            return enabled ? "Yön preseti: Açık" : "Yön preseti: Kapalı";
        }
    }
}
