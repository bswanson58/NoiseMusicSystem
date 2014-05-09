using System;
using System.IO;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Librarian.Interfaces;
using Noise.UI.Support;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.Librarian.ViewModels {
	public class ImportDatabaseViewModel : AutomaticCommandBase {
		private readonly IDialogService	mDialogService;
		private readonly ILibrarian		mLibrarian;

		public ImportDatabaseViewModel( ILibrarian librarian, IDialogService dialogService ) {
			mLibrarian = librarian;
			mDialogService = dialogService;
		}

		public string ImportPath {
			get {  return( Get( () => ImportPath )); }
			set {
				Set( () => ImportPath, value );

				if(( string.IsNullOrWhiteSpace( LibraryName )) &&
				   ( File.Exists( ImportPath ))) {
					var library = LibraryConfiguration.LoadConfiguration( ImportPath );

					if( library != null ) {
						LibraryName = library.LibraryName;
					}
				}
			}
		}

		public string LibraryName {
			get {  return( Get( () => LibraryName )); }
			set {  Set( () => LibraryName, value ); }
		}

		public void Execute_ImportLibrary() {
			if( !string.IsNullOrWhiteSpace( ImportPath )) {
				var backup = new LibraryBackup( DateTime.Now, Path.GetDirectoryName( ImportPath ));
				var library = LibraryConfiguration.LoadConfiguration( ImportPath );

				if( library != null ) {
					library.LibraryName = LibraryName;

					mLibrarian.ImportLibrary( library, backup );
				}
			}
		}

		[DependsUpon( "ImportPath" )]
		[DependsUpon( "LibraryName" )]
		public bool CanExecute_ImportLibrary() {
			return(!string.IsNullOrWhiteSpace( ImportPath ) &&
				  (!string.IsNullOrWhiteSpace( LibraryName )));
		}

		public void Execute_Browse( object sender ) {
			string path;

			if( mDialogService.OpenFileDialog( "Select library to import:", "*.*", "Configuration Files|" + Constants.LibraryConfigurationFile, out path ).GetValueOrDefault( false )) {
				ImportPath = path;
			}
		}
	}
}
