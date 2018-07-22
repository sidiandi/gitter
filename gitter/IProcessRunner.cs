using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gitter
{
    public class ProcessResult
    {
        public int ExitCode;
        public string Output;
        public string Error;
        public DateTime ExitTime;
        public DateTime StartTime;
    }

    public interface IProcessRunner
    {
        Task<ProcessResult> Run(string file, IEnumerable<string> args);
    }
}
