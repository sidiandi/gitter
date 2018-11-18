using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gitter
{
    public class ProcessResult
    {
        public int ExitCode { get; internal set; }
        public string Output { get; internal set; }
        public string Error { get; internal set; }
        public DateTime ExitTime { get; internal set; }
        public DateTime StartTime { get; internal set; }

        public bool Success => ExitCode == 0;

        public string Arguments { get; internal set; }
        public string FileName { get; internal set; }

        public override string ToString()
        {
            return Extensions.ToString(_ => this.PrintProperties(_));
        }
    }

    public interface IProcessRunner
    {
        Task<ProcessResult> Run(string file, IEnumerable<string> args);
    }
}
