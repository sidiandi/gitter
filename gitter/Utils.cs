using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Functional.Option;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gitter
{
    public static class Utils
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

        public static Option<string> LookUpwardsForSubdirectory(string name)
        {
            return LookUpwardsForSubdirectory(System.Environment.CurrentDirectory, name);
        }

        public static Option<string> LookUpwardsForSubdirectory(string startDirectory, string name)
        {
            for (var d = startDirectory; Directory.Exists(d); d = Path.GetDirectoryName(d))
            {
                var subDirectory = Path.Combine(d, name);
                if (Directory.Exists(subDirectory))
                {
                    return subDirectory;
                }
            }
            return Option.None;
        }

        public static IHtmlContent RenderTildeSlash(this IHtmlHelper htmlHelper, IUrlHelper urlHelper, string html)
        {
            var basePath = urlHelper.ActionContext.HttpContext.Request.PathBase;
            html = Regex.Replace(html, @"\""\/", @"""" + basePath + "/");
            html = Regex.Replace(html, @"\""\~\/", @"""" + basePath + "/");
            return htmlHelper.Raw(html);
        }

        public static string Truncate(this string x, int maxLength)
        {
            if (x.Length > maxLength)
            {
                return x.Substring(0, maxLength);
            }
            else
            {
                return x;
            }
        }

        public static string[] SplitArguments(string commandLine)
        {
            if (String.IsNullOrEmpty(commandLine))
            {
                return new string[] { };
            }

            var parmChars = commandLine.ToCharArray();
            var inSingleQuote = false;
            var inDoubleQuote = false;
            for (var index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    parmChars[index] = '\n';
                }
                if (parmChars[index] == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    parmChars[index] = '\n';
                }
                if (!inSingleQuote && !inDoubleQuote && parmChars[index] == ' ')
                    parmChars[index] = '\n';
            }
            return (new string(parmChars)).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        internal static object JoinTrailingSeparator(string separator, IEnumerable<string> items)
        {
            var s = new StringWriter();
            foreach (var i in items)
            {
                s.Write(separator);
                s.Write(i);
            }
            s.Write(separator);
            return s.ToString();
        }
    }
}
