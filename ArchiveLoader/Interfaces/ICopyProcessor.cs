﻿using System;
using ArchiveLoader.Dto;

namespace ArchiveLoader.Interfaces {
    public interface ICopyProcessor : IDisposable {
        void    StartCopyProcess( Events.JobTargets targets );
        void    ContinueErroredProcess( string processKey, string handlerName );

        IObservable<Events.ProcessItemEvent>    OnProcessingItemChanged { get; }
    }
}
