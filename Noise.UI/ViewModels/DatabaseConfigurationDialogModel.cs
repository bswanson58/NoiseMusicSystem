using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	internal class DatabaseConfigurationDialogModel : DialogModelBase {
		private readonly IDialogService	mDialogService;

		public DatabaseConfigurationDialogModel( IDialogService dialogService ) {
			mDialogService = dialogService;
		}

		public void Execute_Browse( object sender ) {
			if( EditObject != null ) {
				string path = EditObject.SearchIndexLocation;

				if( mDialogService.SelectFolderDialog( "Select Search Index Location", ref path ).GetValueOrDefault( false )) {
					EditObject.SearchIndexLocation = path;
				}
			}
		}
	}
}
