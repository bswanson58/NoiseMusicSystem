using System;

namespace ArchiveLoader.Interfaces {
    public interface IProcessManager : IDisposable {
        void    StartProcessing();
    }
}
