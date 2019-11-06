using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Models {
    class ExitHandlerFactory : IExitHandlerFactory {
        public IProcessExitHandler GetExitHandler( FileTypeHandler forHandler ) {
            IProcessExitHandler retValue;

            if( forHandler is CopyFileHandler ) {
                retValue = new CopyFileExitHandler();
            }
            else {
                retValue = new ProcessExitCodeHandler();
            }

            return retValue;
        }
    }
}
