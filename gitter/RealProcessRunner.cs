using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace gitter
{
    internal class RealProcessRunner : IProcessRunner
    {
        ILogger _log;

        public RealProcessRunner(ILogger<RealProcessRunner> logger, IEnumerable<string> pathDirectories, IDictionary<string, string> environment)
        {
            this.PathDirectories = pathDirectories.ToList();
            _log = logger;
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
            _log.LogInformation("start {0} {1}", file, argString);
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
                _log.LogDebug("{0}={1}", i.Key, i.Value);
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

            var process = Process.Start(startInfo);

            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();

            process.WaitForExit();

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
            _log.LogInformation("result: {0}", result);
            return result;
        }
    }
}