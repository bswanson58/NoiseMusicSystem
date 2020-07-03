using System;
using ArchiveLoader.Dto;

namespace ArchiveLoader.Interfaces {
    interface IProcessReadyNotifier : IDisposable {
        void                            StartNotifying();
        void                            StopNotifying();

        IObservable<Events.JobTargets>  OnJobReady { get; }
        void                            JobCompleted( string sourceDrive );
    }
}
