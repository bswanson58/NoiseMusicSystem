using System;
using System.Diagnostics;

namespace ArchiveLoader.Dto {
    public enum ProcessState {
        Pending,
        Running,
        Completed
    }

    [DebuggerDisplay("Handler: {" + nameof( DebugString ) + "}" )]
    public class ProcessHandler {
        public  FileTypeHandler     Handler { get; }
        public  ProcessState        ProcessState { get; private set; }
        public  string              ProcessStdOut { get; private set; }
        public  string              ProcessErrOut { get; private set; }
        public  int                 ExitCode {  get; private set; }

        public  bool                IsRunnable => ProcessState == ProcessState.Pending;

        public  string              DebugString => $"{Handler.HandlerName} - {ProcessState}";

        public ProcessHandler( FileTypeHandler handler ) {
            Handler = handler;

            ProcessState = ProcessState.Pending;
            ProcessStdOut = String.Empty;
            ProcessErrOut = String.Empty;
            ExitCode = 0;
        }

        public void SetProcessOutput( string stdOutput, string errOutput, int exitCode ) {
            ProcessStdOut = stdOutput;
            ProcessErrOut = errOutput;
            ExitCode = exitCode;

            ProcessState = ProcessState.Completed;
        }
    }
}
