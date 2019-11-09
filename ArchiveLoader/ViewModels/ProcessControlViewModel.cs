using ArchiveLoader.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class ProcessControlViewModel : AutomaticCommandBase {
        private readonly IProcessManager    mProcessManager;

        public ProcessControlViewModel( IProcessManager processManager ) {
            mProcessManager = processManager;
        }

        public void Execute_StartProcessing() {
            mProcessManager.StartProcessing();
        }
    }
}
