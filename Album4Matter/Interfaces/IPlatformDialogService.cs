namespace Album4Matter.Interfaces {
	public interface IPlatformDialogService {
		bool?	OpenFileDialog( string title, string extensions, string filter, out string fileName, string initialDirectory, bool allowMultipleFiles = false );
		bool?	SaveFileDialog( string title, string extensions, string filter, out string fileName );
		bool?	SelectFolderDialog( string title, ref string path );

		void	MessageDialog( string title, string message );
	}
}
