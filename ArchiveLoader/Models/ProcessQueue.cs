using System.Collections.Generic;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using CSharpTest.Net.Processes;

namespace ArchiveLoader.Models {
    internal class ProcessShell {
        private string          mStdOutput;
        private string          mStdError;

        public  ProcessHandler  ProcessHandler { get; }
        public  ProcessRunner   ProcessRunner { get; }

        public ProcessShell( ProcessHandler handler, ProcessRunner runner ) {
            ProcessHandler = handler;
            ProcessRunner = runner;

            runner.ProcessExited += OnProcessExited;
            runner.OutputReceived += OnOutputReceived;
        }

        private void OnOutputReceived( object sender, ProcessOutputEventArgs args ) {
            if( args.Error ) {
                mStdError = args.Data;
            }
            else {
                mStdOutput = args.Data;
            }
        }

        private void OnProcessExited( object sender, ProcessExitedEventArgs args ) {
            ProcessHandler.SetProcessOutput( mStdOutput, mStdError, args.ExitCode );
        }
    }

    class ProcessQueue : IProcessQueue {
        private int                         mProcessLimit;
        private readonly List<ProcessShell> mProcessList;

        public ProcessQueue() {
            mProcessLimit = 3;
            mProcessList = new List<ProcessShell>();
        }

        public bool CanAddProcessItem() {
            bool    retValue;

            lock( mProcessList ) {
                retValue = mProcessList.Count < mProcessLimit;
            }

            return retValue;
        }

        public void AddProcessItem( ProcessHandler handler ) {
            var shell = new ProcessShell( handler, new ProcessRunner( handler.Handler.ExePath, handler.Handler.CommandArguments ));

            lock( mProcessList ) {
                mProcessList.Add( shell );
            }
        }
    }
}
