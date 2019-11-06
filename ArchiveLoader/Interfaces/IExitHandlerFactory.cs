using ArchiveLoader.Dto;

namespace ArchiveLoader.Interfaces {
    public interface IExitHandlerFactory {
        IProcessExitHandler     GetExitHandler( FileTypeHandler forHandler );
    }
}
