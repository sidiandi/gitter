using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gitter
{
    public static class MarkdownExtensions
    {
        public static string ToMarkdownTableContent(string text)
        {
            return Regex.Replace(text, @"\n", "<br>");
        }

        static Func<T, string>[] GetAutoColumns<T>()
        {
            return typeof(T).GetProperties()
                .Select(property => new Func<T, string>(item => SafeToString(property.GetValue(item))))
                .ToArray();
        }

        static string SafeToString(object x)
        {
            try
            {
                return x.ToString();
            }
            catch
            {
                return String.Empty;
            }
        }

        public static string MarkdownTable<T>(this IEnumerable<T> items, params Func<T, string>[] columns)
        {
            if (columns.Length == 0)
            {
                columns = GetAutoColumns<T>();
            }

            var s = new StringWriter();
            const string tableSep = "|";
            s.WriteLine();
            s.WriteLine(Utils.JoinTrailingSeparator(tableSep, columns.Select(_ => "   ")));
            s.WriteLine(Utils.JoinTrailingSeparator(tableSep, columns.Select(_ => " --- ")));
            foreach (var i in items)
            {
                s.WriteLine(Utils.JoinTrailingSeparator(tableSep, columns.Select(_ => ToMarkdownTableContent(_(i)) + " ")));
            }
            s.WriteLine();
            return s.ToString();
        }
    }
}
