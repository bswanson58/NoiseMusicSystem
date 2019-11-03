using CSharpTest.Net.Processes;

namespace ArchiveLoader.Dto {
    public class ProcessHandler {
        public  FileTypeHandler     Handler { get; }
        public  ProcessRunner       ProcessRunner { get; set; }

        public ProcessHandler( FileTypeHandler handler ) {
            Handler = handler;
        }
    }
}
