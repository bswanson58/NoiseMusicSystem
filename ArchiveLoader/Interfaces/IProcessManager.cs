using System;
using ArchiveLoader.Dto;

namespace ArchiveLoader.Interfaces {
    public enum ProcessingState {
        Running,
        Stopped
    }

    public interface IProcessManager : IDisposable {
        IObservable<ProcessingState>            OnProcessingStateChanged { get; }
        IObservable<Events.ProcessItemEvent>    OnProcessingItemChanged { get; }

        void    StartProcessing();
        void    StopProcessing();

        void    ContinueErroredProcess( string processKey, string handlerName );
        void    AbortErroredProcess( string processKey, string handlerName );    }
}
