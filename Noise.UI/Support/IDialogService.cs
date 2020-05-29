namespace Noise.UI.Support {
	public interface IDialogService {
//		bool?	ShowDialog( string dialogName, object editObject );
//		bool?	ShowDialog( string dialogName, DialogModelBase viewModel );
//		bool?	ShowDialog( string dialogName, object editObject, DialogModelBase viewModel );

		bool?	OpenFileDialog( string title, string extensions, string filter, out string fileName );
		bool?	SaveFileDialog( string title, string extensions, string filter, out string fileName );
		bool?	SelectFolderDialog( string title, ref string path );

		void	MessageDialog( string title, string message );
	}
}
