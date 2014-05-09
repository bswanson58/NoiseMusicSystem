using System;
using System.IO;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Librarian.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.Librarian.ViewModels {
	public class ImportDatabaseViewModel : AutomaticCommandBase {
		private readonly ILibrarian		mLibrarian;

		public ImportDatabaseViewModel( ILibrarian librarian ) {
			mLibrarian = librarian;

			ImportPath = @"D:\Import\Library.config";
			LibraryName = "Imported Library";
		}

		public string ImportPath {
			get {  return( Get( () => ImportPath )); }
			set {  Set( () => ImportPath, value ); }
		}

		public string LibraryName {
			get {  return( Get( () => LibraryName )); }
			set {  Set( () => LibraryName, value ); }
		}

		public void Execute_ImportLibrary() {
			if( !string.IsNullOrWhiteSpace( ImportPath )) {
				var backup = new LibraryBackup( DateTime.Now, Path.GetDirectoryName( ImportPath ));
				var library = LibraryConfiguration.LoadConfiguration( ImportPath );

				mLibrarian.ImportDatabase( library, backup );
			}
		}

		[DependsUpon( "ImportPath" )]
		[DependsUpon( "LibraryName" )]
		public bool CanExecute_ImportLibrary() {
			return(!string.IsNullOrWhiteSpace( ImportPath ) &&
				  (!string.IsNullOrWhiteSpace( LibraryName )));
		}
	}
}
