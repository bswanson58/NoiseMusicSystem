using System;
using ArchiveLoader.Dto;

namespace ArchiveLoader.Interfaces {
    public interface IProcessQueue {
        IObservable<ProcessHandler> OnProcessCompleted { get; }

        bool    CanAddProcessItem();
        void    AddProcessItem( ProcessHandler handler );
    }
}
