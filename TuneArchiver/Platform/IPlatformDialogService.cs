namespace TuneArchiver.Platform {
	public interface IDialogService {
		bool?	OpenFileDialog( string title, string extensions, string filter, out string fileName );
		bool?	SaveFileDialog( string title, string extensions, string filter, out string fileName );
		bool?	SelectFolderDialog( string title, ref string path );

		void	MessageDialog( string title, string message );
	}
}
