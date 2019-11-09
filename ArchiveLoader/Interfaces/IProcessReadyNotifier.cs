using System;
using ArchiveLoader.Dto;

namespace ArchiveLoader.Interfaces {
    interface IProcessReadyNotifier : IDisposable {
        void                            StartProcessing();

        IObservable<Events.JobTargets>  OnJobReady { get; }
    }
}
