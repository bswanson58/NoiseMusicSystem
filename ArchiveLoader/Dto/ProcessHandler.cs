using System;
using System.Diagnostics;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Dto {
    public enum ProcessState {
        Pending,
        Running,
        Completed,
        Error
    }

    [DebuggerDisplay("Handler: {" + nameof( DebugString ) + "}" )]
    public class ProcessHandler {
        public  string              ParentKey { get; }
        public  FileTypeHandler     Handler { get; }
        public  string              InputFile { get; }
        public  string              OutputFile { get; }
        public  string              InstanceArguments {  get; }
        public  ProcessState        ProcessState { get; private set; }
        public  string              ProcessStdOut { get; private set; }
        public  string              ProcessErrOut { get; private set; }
        public  int                 ExitCode { get; private set; }
        public  IProcessExitHandler ExitHandler { get; }

        public  string              DebugString => $"{Handler.HandlerName} - {ProcessState}";

        public ProcessHandler( ProcessItem item, FileTypeHandler handler, string inputFile, string outputFile, IProcessExitHandler exitHandler ) {
            ParentKey = item.Key;
            Handler = handler;
            InputFile = inputFile;
            OutputFile = outputFile;
            ExitHandler = exitHandler;

            InstanceArguments = handler.CommandArguments.Replace( "{input}", InputFile ).Replace( "{output}", OutputFile );
            foreach( var key in item.Metadata ) {
                InstanceArguments = InstanceArguments.Replace( key.Key, item.Metadata[key.Key]);
            }

            ProcessState = ProcessState.Pending;
            ProcessStdOut = String.Empty;
            ProcessErrOut = String.Empty;
            ExitCode = 0;
        }

        public void SetProcessRunning() {
            ProcessState = ProcessState.Running;
        }

        public void SetProcessOutput( string stdOutput, string errOutput, int exitCode ) {
            ProcessStdOut = stdOutput;
            ProcessErrOut = errOutput;
            ExitCode = exitCode;

            ProcessState = ExitHandler?.HandleProcessExitState( this ) ?? ProcessState.Completed;
        }
    }
}
