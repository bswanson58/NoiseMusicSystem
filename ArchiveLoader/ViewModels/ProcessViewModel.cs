using ArchiveLoader.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class ProcessViewModel : AutomaticCommandBase {
        private readonly IProcessManager    mProcessManager;

        public ProcessViewModel( IProcessManager processManager ) {
            mProcessManager = processManager;
        }

        public void Execute_StartProcessing() {
            mProcessManager.StartProcessing();
        }
    }
}
