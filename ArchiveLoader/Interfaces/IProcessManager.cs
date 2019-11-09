﻿using System;
using ArchiveLoader.Dto;

namespace ArchiveLoader.Interfaces {
    public interface IProcessManager : IDisposable {
        IObservable<Events.ProcessItemEvent>    OnProcessingItemChanged { get; }

        void    StartProcessing();
    }
}
