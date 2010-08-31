using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	internal class LibraryConfigurationDialogModel : DialogModelBase {
		private readonly IDialogService	mDialogService;

		public LibraryConfigurationDialogModel( IDialogService dialogService ) {
			mDialogService = dialogService;
		}

		public void Execute_Browse( object sender ) {
			if( EditObject != null ) {
				string path = EditObject.Path;

				if( mDialogService.SelectFolderDialog( "Select Music Library", ref path ).GetValueOrDefault( false )) {
					EditObject.Path = path;
				}
			}
		}
	}
}
