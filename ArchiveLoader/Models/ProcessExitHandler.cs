using System;
using System.IO;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Models {
    abstract class BaseProcessExitHandler : IProcessExitHandler {
        public ProcessState HandleProcessExitState( ProcessHandler handler ) {
            var retValue = DetermineExitState( handler );

            if(( retValue == ProcessState.Completed ) &&
               ( handler.Handler.DeleteInputFileOnSuccess )) {
                retValue = DeleteInputFile( handler );
            }

            return retValue;
        }

        protected abstract ProcessState DetermineExitState( ProcessHandler handler );

        private ProcessState DeleteInputFile( ProcessHandler handler ) {
            var retValue = ProcessState.Completed;

            try {
                if( File.Exists( handler.InputFile )) {
                    File.SetAttributes( handler.InputFile, FileAttributes.Normal );
                    File.Delete( handler.InputFile );
                }
            }
            catch( Exception ) {
                retValue = ProcessState.Error;
            }

            return retValue;
        }
    }

    class CopyFileExitHandler : BaseProcessExitHandler {
        protected override ProcessState DetermineExitState( ProcessHandler handler ) {
            return ProcessState.Completed;
        }
    }

    class ProcessExitCodeHandler : BaseProcessExitHandler {
        protected override ProcessState DetermineExitState( ProcessHandler handler ) {
            return handler.ExitCode == 0 ? ProcessState.Completed : ProcessState.Error;
        }
    }
}
