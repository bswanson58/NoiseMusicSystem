using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Text;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using CSharpTest.Net.Processes;

namespace ArchiveLoader.Models {
    [DebuggerDisplay("Handler: {" + nameof( DebugString ) + "}" )]
    internal class ProcessShell : IDisposable {
        private readonly StringBuilder  mStdOutput;
        private readonly StringBuilder  mStdError;

        public  ProcessHandler          ProcessHandler { get; }
        public  ProcessRunner           ProcessRunner { get; private set; }

        public  delegate                void ProcessCompleteEvent( ProcessShell item );
        public  event                   ProcessCompleteEvent OnProcessCompleted;

        public  string                  DebugString => $"{ProcessHandler.Handler.HandlerName} - {ProcessRunner.IsRunning}";

        public ProcessShell( ProcessHandler handler, ProcessRunner runner ) {
            ProcessHandler = handler;
            ProcessRunner = runner;

            mStdOutput = new StringBuilder();
            mStdError = new StringBuilder();

            ProcessRunner.ProcessExited += OnProcessExited;
            ProcessRunner.OutputReceived += OnOutputReceived;
        }

        public void StartProcess() {
            ProcessRunner.StartWithFormattedArgs( ProcessHandler.InstanceArguments );

            ProcessHandler.SetProcessRunning();
        }

        private void OnOutputReceived( object sender, ProcessOutputEventArgs args ) {
            if( args.Error ) {
                mStdError.AppendLine( args.Data );
            }
            else {
                mStdOutput.AppendLine( args.Data );
            }
        }

        private void OnProcessExited( object sender, ProcessExitedEventArgs args ) {
            ProcessHandler.SetProcessOutput( mStdOutput.ToString(), mStdError.ToString(), args.ExitCode );

            OnProcessCompleted?.Invoke( this );
        }

        public void Dispose() {
            if( ProcessRunner != null ) {
                ProcessRunner.OutputReceived -= OnOutputReceived;
                ProcessRunner.ProcessExited -= OnProcessExited;

                ProcessRunner.Dispose();
                ProcessRunner = null;
            }
        }
    }

    class ProcessQueue : IProcessQueue {
        private readonly IPlatformLog               mLog;
        private readonly List<ProcessShell>         mProcessList;
        private readonly Subject<ProcessHandler>    mProcessCompletedSubject;
        private readonly int                        mProcessLimit;

        public  IObservable<ProcessHandler>         OnProcessCompleted => mProcessCompletedSubject;

        public ProcessQueue( IPlatformLog log ) {
            mLog = log;

            mProcessList = new List<ProcessShell>();
            mProcessCompletedSubject = new Subject<ProcessHandler>();

            mProcessLimit = DetermineProcessLimit();
        }

        private int DetermineProcessLimit() {
            return 3;
        }

        public bool CanAddProcessItem() {
            bool    retValue;

            lock( mProcessList ) {
                retValue = mProcessList.Count < mProcessLimit;
            }

            return retValue;
        }

        public void AddProcessItem( ProcessHandler handler ) {
            var shell = new ProcessShell( handler, new ProcessRunner( handler.Handler.ExePath ));

            shell.OnProcessCompleted += OnShellCompleted;

            lock( mProcessList ) {
                mProcessList.Add( shell );
            }

//            mLog.LogMessage( "ProcessQueue: Started " + shell.ProcessRunner + shell.ProcessHandler.InstanceArguments );
            shell.StartProcess();
        }

        private void OnShellCompleted( ProcessShell item ) {
            lock( mProcessList ) {
                mProcessList.Remove( item );
            }

//            mLog.LogMessage( $"ProcessQueue: Completed {item.ProcessHandler.Handler.HandlerName} with exit code: '{item.ProcessHandler.ExitCode}' - {item.ProcessHandler.ProcessErrOut}" );
            mProcessCompletedSubject.OnNext( item.ProcessHandler );

            item.OnProcessCompleted -= OnShellCompleted;
            item.Dispose();
        }
    }
}
