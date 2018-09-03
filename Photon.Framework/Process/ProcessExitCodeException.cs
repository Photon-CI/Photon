using System;

namespace Photon.Framework.Process
{
    public class ProcessExitCodeException : Exception
    {
        public ProcessResult Result {get;}
        public int ExitCode {get;}


        public ProcessExitCodeException(int exitCode) : base($"Process returned a non-zero exit code! [{exitCode}]")
        {
            this.ExitCode = exitCode;
        }

        public ProcessExitCodeException(ProcessResult result) : this(result.ExitCode)
        {
            this.Result = result;
        }
    }
}
