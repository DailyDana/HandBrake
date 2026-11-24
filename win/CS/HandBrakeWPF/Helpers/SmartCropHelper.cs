// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmartCropHelper.cs" company="HandBrake Project (https://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Fork: libhb otomatik kırpmasının kaçırdığı siyah kenarları önizleme karelerinden bulur.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrakeWPF.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// libhb (scan.c) iki durumda kenarı kırpmaz:
    ///  1. Kenar çerçevenin 1/4'ünden büyükse (ör. 16:9 çerçevede dikey video) o kare hiç sayılmaz.
    ///  2. Satır/sütundaki TÜM piksellerin ortalamaya ±16 yakın olması istenir; kenardaki küçük bir logo ya da
    ///     yazı kırpmayı orada durdurur.
    /// Burada bir satır/sütun, piksellerinin %95'i koyuysa kenar sayılır ve kenar %45'e kadar çıkabilir.
    /// Çok koyu kareler (karartma, gece sahnesi) sayılmaz; sonuç karelerin en az %75'inde geçerli olan değerdir.
    /// </summary>
    public static class SmartCropHelper
    {
        private const int DarkLuma = 40;            // RGB önizlemede siyah ~0-20, gürültü payı
        private const double BarFraction = 0.95;    // satır/sütunun %95'i koyuysa kenar
        private const double MaxSide = 0.45;        // bir kenar en fazla çerçevenin %45'i
        private const double MinVisible = 0.10;     // geriye en az %10 görüntü kalmalı
        private const double DarkFrame = 0.85;      // karenin %85'i koyuysa (karartma) kareyi sayma

        /// <summary>
        /// Tek bir RGB32 (B,G,R,X) önizleme karesinde kenarları bulur. Kare sayılmayacaksa null döner.
        /// Dönen dizi: üst, alt, sol, sağ (önizleme pikseli).
        /// </summary>
        public static int[] DetectBars(byte[] data, int strideBytes, int width, int height)
        {
            if (data == null || width < 16 || height < 16 || strideBytes < width * 4 || data.Length < strideBytes * height)
            {
                return null;
            }

            bool[] dark = new bool[width * height];
            int darkCount = 0;
            for (int y = 0; y < height; y++)
            {
                int row = y * strideBytes;
                for (int x = 0; x < width; x++)
                {
                    int p = row + (x * 4);
                    int luma = ((19 * data[p]) + (183 * data[p + 1]) + (54 * data[p + 2])) >> 8; // BT.709
                    bool d = luma < DarkLuma;
                    dark[(y * width) + x] = d;
                    if (d)
                    {
                        darkCount++;
                    }
                }
            }

            if (darkCount > DarkFrame * width * height)
            {
                return null; // karartma / çok koyu sahne
            }

            int maxV = (int)(height * MaxSide), maxH = (int)(width * MaxSide);

            int top = 0;
            while (top < maxV && RowIsBar(dark, width, top, 0, width))
            {
                top++;
            }

            int bottom = 0;
            while (bottom < maxV && RowIsBar(dark, width, height - 1 - bottom, 0, width))
            {
                bottom++;
            }

            int left = 0;
            while (left < maxH && ColumnIsBar(dark, width, left, top, height - bottom))
            {
                left++;
            }

            int right = 0;
            while (right < maxH && ColumnIsBar(dark, width, width - 1 - right, top, height - bottom))
            {
                right++;
            }

            if (height - top - bottom < height * MinVisible || width - left - right < width * MinVisible)
            {
                return null;
            }

            return new[] { top, bottom, left, right };
        }

        /// <summary>
        /// Karelerin sonuçlarını birleştirip kaynak çözünürlüğüne çevirir. En az 3 geçerli kare gerekir;
        /// her kenar için karelerin en az %75'inde bulunan değer alınır (çift sayıya aşağı yuvarlanır).
        /// Dönen dizi: üst, alt, sol, sağ (kaynak pikseli) ya da null.
        /// </summary>
        public static int[] Combine(IList<int[]> frames, int previewWidth, int previewHeight, int sourceWidth, int sourceHeight)
        {
            List<int[]> valid = frames?.Where(f => f != null).ToList() ?? new List<int[]>();
            if (valid.Count < 3 || previewWidth <= 0 || previewHeight <= 0)
            {
                return null;
            }

            int index = valid.Count / 4; // artan sıralamada: karelerin >= %75'i en az bu kadar kenara sahip
            int[] result = new int[4];
            for (int side = 0; side < 4; side++)
            {
                int[] values = valid.Select(f => f[side]).OrderBy(v => v).ToArray();
                double scale = side < 2 ? (double)sourceHeight / previewHeight : (double)sourceWidth / previewWidth;
                int px = (int)Math.Floor(values[index] * scale);
                result[side] = px - (px % 2);
            }

            return result;
        }

        /// <summary>
        /// libhb sonucunu, belirgin biçimde daha büyük kenar bulunan taraflarda iyileştirilmiş değerle değiştirir.
        /// </summary>
        public static int[] Merge(int[] libhb, int[] smart, int sourceWidth, int sourceHeight)
        {
            if (smart == null)
            {
                return libhb;
            }

            int[] merged = (int[])libhb.Clone();
            for (int side = 0; side < 4; side++)
            {
                int dim = side < 2 ? sourceHeight : sourceWidth;
                int threshold = Math.Max(8, (int)(dim * 0.02));
                if (smart[side] > libhb[side] + threshold)
                {
                    merged[side] = smart[side];
                }
            }

            return merged;
        }

        private static bool RowIsBar(bool[] dark, int width, int y, int x0, int x1)
        {
            int count = 0;
            for (int x = x0; x < x1; x++)
            {
                if (dark[(y * width) + x])
                {
                    count++;
                }
            }

            return count >= BarFraction * (x1 - x0);
        }

        private static bool ColumnIsBar(bool[] dark, int width, int x, int y0, int y1)
        {
            if (y1 <= y0)
            {
                return false;
            }

            int count = 0;
            for (int y = y0; y < y1; y++)
            {
                if (dark[(y * width) + x])
                {
                    count++;
                }
            }

            return count >= BarFraction * (y1 - y0);
        }
    }
}
