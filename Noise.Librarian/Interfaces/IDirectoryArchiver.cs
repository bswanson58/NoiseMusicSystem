namespace Noise.Librarian.Interfaces {
	public interface IDirectoryArchiver {
		void	BackupDirectory( string sourcePath, string destinationName );
		void	RestoreDirectory( string sourceName, string destinationPath );
	}
}
