// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueSortHelper.cs" company="HandBrake Project (https://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Fork: "Add All" / "Add Selection" ile kuyruğa eklenen başlıkların sırası.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrakeWPF.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;

    public static class QueueSortHelper
    {
        // Ayarda saklanan değerler (ForkQueueSortOrder).
        public const int DateOldestFirst = 0;
        public const int DateNewestFirst = 1;
        public const int NameAscending = 2;
        public const int NameDescending = 3;
        public const int SourceOrder = 4;

        public static string GetLabel(int mode)
        {
            switch (mode)
            {
                case DateOldestFirst: return "Tarih (eskiden yeniye)";
                case DateNewestFirst: return "Tarih (yeniden eskiye)";
                case NameAscending: return "Ad (A → Z)";
                case NameDescending: return "Ad (Z → A)";
                case SourceOrder: return "Kaynak sırası";
                default: return "Tarih (eskiden yeniye)";
            }
        }

        /// <summary>
        /// Öğeleri seçilen moda göre sıralar. Dosyası olmayan öğeler (DVD vb.) sona gider ve
        /// kendi aralarındaki sıra korunur (OrderBy kararlıdır).
        /// </summary>
        public static List<T> Order<T>(IEnumerable<T> items, Func<T, string> pathOf, int mode)
        {
            List<T> list = items.ToList();
            switch (mode)
            {
                case DateOldestFirst:
                    return list.OrderBy(i => HasFile(pathOf(i)) ? 0 : 1).ThenBy(i => GetSourceFileDate(pathOf(i))).ToList();
                case DateNewestFirst:
                    return list.OrderBy(i => HasFile(pathOf(i)) ? 0 : 1).ThenByDescending(i => GetSourceFileDate(pathOf(i))).ToList();
                case NameAscending:
                    return list.OrderBy(i => HasFile(pathOf(i)) ? 0 : 1).ThenBy(i => FileName(pathOf(i)), ExplorerComparer.Instance).ToList();
                case NameDescending:
                    return list.OrderBy(i => HasFile(pathOf(i)) ? 0 : 1).ThenByDescending(i => FileName(pathOf(i)), ExplorerComparer.Instance).ToList();
                default:
                    return list;
            }
        }

        /// <summary>
        /// Kaynak dosyanın tarihi: oluşturma ve değiştirme tarihinden erken olanı. Kopyalanmış dosyalarda
        /// oluşturma tarihi kopyalama anıdır, değiştirme tarihi ise çoğunlukla çekim tarihini korur.
        /// </summary>
        public static DateTime GetSourceFileDate(string path)
        {
            try
            {
                if (HasFile(path))
                {
                    DateTime created = File.GetCreationTimeUtc(path);
                    DateTime modified = File.GetLastWriteTimeUtc(path);
                    return created < modified ? created : modified;
                }
            }
            catch (Exception)
            {
                // Tarih okunamazsa sona koy.
            }

            return DateTime.MaxValue;
        }

        private static bool HasFile(string path)
        {
            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }

        private static string FileName(string path)
        {
            return string.IsNullOrEmpty(path) ? string.Empty : Path.GetFileName(path);
        }

        /// <summary>Windows Gezgini ile aynı "doğal" ad sıralaması (video2 &lt; video10).</summary>
        private sealed class ExplorerComparer : IComparer<string>
        {
            public static readonly ExplorerComparer Instance = new ExplorerComparer();

            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
            private static extern int StrCmpLogicalW(string x, string y);

            public int Compare(string x, string y)
            {
                return StrCmpLogicalW(x ?? string.Empty, y ?? string.Empty);
            }
        }
    }
}
