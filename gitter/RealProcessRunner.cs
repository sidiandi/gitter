using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace gitter
{
    internal class RealProcessRunner : IProcessRunner
    {
        public static string QuoteIfRequired(string x)
        {
            const string quote = "\"";
            if (x.StartsWith(quote) && x.EndsWith(quote))
            {
                return x;
            }
            return x.Quote();
        }

        public async Task<ProcessResult> Run(string file, IEnumerable<string> args)
        {
            var startTime = DateTime.UtcNow;

            var p = Process.Start(new ProcessStartInfo()
            {
                FileName = file,
                Arguments = string.Join(" ", args.Select(QuoteIfRequired)),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            var output = p.StandardOutput.ReadToEndAsync();
            var error = p.StandardError.ReadToEndAsync();

            p.WaitForExit();

            return new ProcessResult
            {
                ExitCode = p.ExitCode,
                StartTime = startTime,
                ExitTime = DateTime.UtcNow,
                Output = await output,
                Error = await error
            };
        }
    }
}