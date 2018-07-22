using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Functional.Option;

namespace gitter
{
    internal static class Utils
    {
        public static IEnumerable<string> SplitPath(this string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return Enumerable.Empty<string>();
            }
            return path.Split('/').Where(_ => !String.IsNullOrEmpty(_));
        }

        public static string Quote(this string x)
        {
            return "\"" + x + "\"";
        }

        public static void EnsureParentDirectoryExists(string path)
        {
            EnsureDirectoryExists(Path.GetDirectoryName(path));
        }

        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static Functional.Option.Option<Match> AsOption(this Match match)
        {
            return match.Success ? match : Option<Match>.None;
        }

        public static bool IsImage(this ContentPath cp)
        {
            return cp.IsExtension(new[] { ".png", ".svg", ".jpg", ".jpeg" });
        }

        public static IEnumerable<string> WhereNotEmpty(this IEnumerable<string> e)
        {
            return e.Where(_ => !String.IsNullOrEmpty(_));
        }

        public static Option<T> FirstOrNone<T>(this IEnumerable<T> e)
        {
            using (var i = e.GetEnumerator())
            {
                if (i.MoveNext())
                {
                    return i.Current;
                }
                else
                {
                    return Option.None;
                }
            }
        }

        public static Option<T> FirstOrNone<T>(this IEnumerable<Option<T>> e)
        {
            foreach (var i in e)
            {
                if (i.HasValue)
                {
                    return i;
                }
            }
            return Option.None;
        }

        public static Option<T> FirstOrNone<T>(this IEnumerable<T> e, Func<T, bool> condition)
        {
            foreach (var i in e)
            {
                if (condition(i))
                {
                    return i;
                }
            }
            return Option.None;
        }

        public static IEnumerable<T> WhereValue<T>(this IEnumerable<Option<T>> e)
        {
            return e.Where(_ => _.HasValue).Select(_ => _.Value);
        }

        public static string JoinLines(this IEnumerable<string> lines)
        {
            return String.Join("\r\n", lines);
        }
    }
}
