namespace TuneArchiver.Interfaces {
    public interface IFileWriter {
        void	Write<T>( string path, T item );
        T		Read<T>( string path );
    }
}
