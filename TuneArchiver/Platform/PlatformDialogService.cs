using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace TuneArchiver.Platform {
	public class DialogService : IDialogService {
		public bool? OpenFileDialog( string title, string extensions, string filter, out string fileName ) {
			fileName = "";

			var dlg = new Microsoft.Win32.OpenFileDialog { Title = title, DefaultExt = extensions, Filter = filter };
			var	retValue = dlg.ShowDialog();

			if( retValue.GetValueOrDefault( false ) ) {
				fileName = dlg.FileName;
			}

			return ( retValue );
		}

		public bool? SaveFileDialog( string title, string extensions, string filter, out string fileName ) {
			fileName = "";

			var dlg = new Microsoft.Win32.SaveFileDialog { Title = title, DefaultExt = extensions, Filter = filter, AddExtension = true };
			var	retValue = dlg.ShowDialog();

			if( retValue.GetValueOrDefault( false ) ) {
				fileName = dlg.FileName;
			}

			return ( retValue );
		}

		public bool? SelectFolderDialog( string title, ref string path ) {
			bool?	retValue = false;

			var dialog = new FolderBrowserDialogEx {
				Description = title,
				ShowNewFolderButton = true,
				ShowEditBox = true,
				//NewStyle = false,
				SelectedPath = path,
				ShowFullPathInEditBox = false,
				RootFolder = System.Environment.SpecialFolder.MyComputer
			};

			if( dialog.ShowDialog() == DialogResult.OK ) {
				path = dialog.SelectedPath;
				retValue = true;
			}

			return( retValue );
		}

		public void MessageDialog( string title, string message ) {
			MessageBox.Show( message, title, MessageBoxButton.OK, MessageBoxImage.Information );
		}
	}
}
