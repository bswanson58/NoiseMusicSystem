using System;
using ArchiveLoader.Dto;

namespace ArchiveLoader.Interfaces {
    public interface ICopyProcessor : IDisposable {
        void    StartCopyProcess( Events.JobTargets targets );

        IObservable<Events.ProcessItemEvent>    OnProcessingItemChanged { get; }
    }
}
