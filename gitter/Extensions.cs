using System;
using System.IO;

namespace gitter
{
    public static class Extensions
    {
        public static string ToString(Action<TextWriter> o)
        {
            using (var w = new StringWriter())
            {
                o(w);
                return w.ToString();
            }
        }

        public static string SafeToString(this object x)
        {
            if (x == null)
            {
                return "<null>";
            }
            try
            {
                return x.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static void PrintProperties(this object x, TextWriter output)
        {
            foreach (var p in x.GetType().GetProperties())
            {
                output.WriteLine($"{p.Name}: {p.GetValue(x).SafeToString()}");
            }
        }
    }
}