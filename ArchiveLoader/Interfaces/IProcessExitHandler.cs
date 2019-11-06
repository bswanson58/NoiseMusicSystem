using ArchiveLoader.Dto;

namespace ArchiveLoader.Interfaces {
    public interface IProcessExitHandler {
        ProcessState    HandleProcessExitState( ProcessHandler handler );
    }
}
