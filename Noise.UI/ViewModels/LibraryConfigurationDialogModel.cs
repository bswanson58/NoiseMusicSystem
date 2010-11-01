using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	internal class LibraryConfigurationDialogModel : DialogModelBase {
		private readonly IDialogService		mDialogService;
		private readonly ILibraryBuilder	mLibraryBuilder;

		public LibraryConfigurationDialogModel( IDialogService dialogService, ILibraryBuilder libraryBuilder ) {
			mDialogService = dialogService;
			mLibraryBuilder = libraryBuilder;
		}

		public void Execute_Browse( object sender ) {
			if( EditObject != null ) {
				string path = EditObject.Path;

				if( mDialogService.SelectFolderDialog( "Select Music Library", ref path ).GetValueOrDefault( false )) {
					EditObject.Path = path;
				}
			}
		}

		public void Execute_UpdateLibrary() {
			if(!mLibraryBuilder.LibraryUpdateInProgress ) {
				mLibraryBuilder.StartLibraryUpdate();

				RaiseCanExecuteChangedEvent( "CanExecute_UpdateLibrary" );
			}
		}

		public bool CanExecute_UpdateLibrary() {
			return(!mLibraryBuilder.LibraryUpdateInProgress );
		}
	}
}
