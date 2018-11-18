using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace gitter
{
    internal class RealProcessRunner : IProcessRunner
    {
        public RealProcessRunner(IEnumerable<string> pathDirectories, IDictionary<string, string> environment)
        {
            this.PathDirectories = pathDirectories.ToList();
            Environment = environment;
        }

        public static string QuoteIfRequired(string x)
        {
            const string quote = "\"";
            if (x.StartsWith(quote) && x.EndsWith(quote))
            {
                return x;
            }
            return x.Quote();
        }

        List<string> PathDirectories;

        const string PATH = "PATH";

        IDictionary<string, string> Environment { get; }

        string GetArgumentString(IEnumerable<string> args)
        {
            return string.Join(" ", args.Select(QuoteIfRequired));
        }

        public async Task<ProcessResult> Run(string file, IEnumerable<string> args)
        {
            var argString = GetArgumentString(args);

            Console.WriteLine($"{file} {argString}");
            var startTime = DateTime.UtcNow;

            var startInfo = new ProcessStartInfo()
            {
                FileName = file,
                Arguments = argString,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            foreach (var i in this.Environment)
            {
                startInfo.EnvironmentVariables[i.Key] = i.Value;
                Console.WriteLine("{0}={1}", i.Key, i.Value);
            }

            foreach (var i in this.PathDirectories)
            {
                var p = System.Environment.GetEnvironmentVariable(PATH);
                if (!p.Contains(i))
                {
                    p = p + ";" + i;
                    System.Environment.SetEnvironmentVariable(PATH, p);
                }
            }

            Console.WriteLine(startInfo.EnvironmentVariables[PATH]);
            Console.WriteLine($"{startInfo.FileName} {startInfo.Arguments}");

            var process = Process.Start(startInfo);

            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();

            process.WaitForExit();

            Console.WriteLine(process.ExitCode);

            var result = new ProcessResult
            {
                ExitCode = process.ExitCode,
                StartTime = startTime,
                ExitTime = DateTime.UtcNow,
                Output = await output,
                Error = await error,
                Arguments = argString,
                FileName = file
            };
            Console.WriteLine(result);
            return result;
        }
    }
}