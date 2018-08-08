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

        class MarkdownTableImpl<T> : IMarkdownTable<T>
        {
            class Column
            {
                public Column(string header, Func<T, string> value)
                {
                    Header = header;
                    Value = value;
                }

                public string Header { get; }
                public Func<T, string> Value { get; }
            }

            Column[] columns;

            public string GetMarkdown()
            {
                if (columns.Length == 0)
                {
                    columns = GetAutoColumns().ToArray();
                }

                var s = new StringWriter();
                const string tableSep = "|";
                s.WriteLine();
                s.WriteLine(Utils.JoinTrailingSeparator(tableSep, columns.Select(_ => _.Header)));
                s.WriteLine(Utils.JoinTrailingSeparator(tableSep, columns.Select(_ => " --- ")));
                foreach (var i in rows)
                {
                    s.WriteLine(Utils.JoinTrailingSeparator(tableSep, columns.Select(_ => ToMarkdownTableContent(_.Value(i)) + " ")));
                }
                s.WriteLine();
                return s.ToString();
            }

            IEnumerable<Column> GetAutoColumns()
            {
                return typeof(T).GetProperties().Select(property => 
                    new Column(
                        property.Name, 
                        new Func<T, string>(item => SafeToString(property.GetValue(item)))));
            }

            public MarkdownTableImpl(IEnumerable<T> rows)
            {
                this.columns = new Column[] { };
                this.rows = rows;
            }

            IEnumerable<T> rows;

            MarkdownTableImpl(IEnumerable<T> rows, IEnumerable<Column> columns)
            {
                this.rows = rows;
                this.columns = columns.ToArray();
            }

            public IMarkdownTable<T> With(string header, Func<T, object> value)
            {
                return new MarkdownTableImpl<T>(rows, columns.Concat(new[] { new Column(header, _ => value(_).ToString()) }));
            }

            public override string ToString()
            {
                return GetMarkdown();
            }
        }

        public static IMarkdownTable<T> MarkdownTable<T>(this IEnumerable<T> items, params Func<T, string>[] columns)
        {
            return new MarkdownTableImpl<T>(items);
        }
    }

    public interface IMarkdownTable<T>
    {
        IMarkdownTable<T> With(string header, Func<T, object> value);
        string GetMarkdown();
    }
}
