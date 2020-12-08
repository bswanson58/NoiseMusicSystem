namespace Noise.RemoteClient.Dto {
    public class LibraryStatus {
        public  bool    LibraryOpen { get; }
        public  string  LibraryName { get; }

        public LibraryStatus( bool open, string libraryName ) {
            LibraryOpen = open;
            LibraryName = libraryName;
        }
    }
}
