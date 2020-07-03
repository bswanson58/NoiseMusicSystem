namespace Album4Matter.Interfaces {
    public interface IFileWriter {
        void	Write<T>( string path, T item );
        T		Read<T>( string path );
    }
}
