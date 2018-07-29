using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gitter
{
    public class Git : IGit
    {
        private readonly IProcessRunner runner;
        private readonly global::System.String gitRepository;

        public Git(IProcessRunner runner, string gitRepository)
        {
            this.runner = runner;
            this.gitRepository = gitRepository;
        }

        public Task<ProcessResult> Run(IEnumerable<string> arguments)
        {
            return runner.Run("git", new[] { "-C", gitRepository }.Concat(arguments));
        }
    }
}
