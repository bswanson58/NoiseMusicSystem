namespace ArchiveLoader.Dto {
    public class FileTypeHandler {
        public  string  HandlerName { get; set; }
        public  string  InputExtension { get; set; }
        public  string  OutputExtension { get; set; }
        public  string  ExePath { get; set; }
        public  string  CommandArguments { get; set; }
        public  bool    DeleteInputFileOnSuccess { get; set; }
    }
}
