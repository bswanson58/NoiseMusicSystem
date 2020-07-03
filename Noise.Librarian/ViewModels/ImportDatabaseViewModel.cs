using System;
using System.IO;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Librarian.Models;
using Noise.UI.Logging;
using Noise.UI.Support;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.Librarian.ViewModels {
	public class ImportDatabaseViewModel : AutomaticCommandBase {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IUiLog				mLog;
		private readonly IDialogService		mDialogService;
		private readonly ILibrarian			mLibrarian;

		public ImportDatabaseViewModel( IEventAggregator eventAggregator, ILibrarian librarian, IDialogService dialogService, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mLibrarian = librarian;
			mDialogService = dialogService;

			ProgressActive = false;
		}

		public string ImportPath {
			get {  return( Get( () => ImportPath )); }
			set {
				Set( () => ImportPath, value );

				if(( string.IsNullOrWhiteSpace( LibraryName )) &&
				   ( File.Exists( ImportPath ))) {
				   try {
						var library = LibraryConfiguration.LoadConfiguration( ImportPath );

						if( library != null ) {
							LibraryName = library.LibraryName;
						}
				   }
				   catch( Exception exception ) {
					   mLog.LogException( $"Loading configuration from \"{ImportPath}\"", exception );
				   }
				}
			}
		}

		public string LibraryName {
			get {  return( Get( () => LibraryName )); }
			set {  Set( () => LibraryName, value ); }
		}

		private void OnImportProgress( LibrarianProgressReport args ) {
			ProgressPhase = args.CurrentPhase;
			ProgressItem = args.CurrentItem;
			Progress = args.Progress;
			ProgressActive = !args.Completed;

			mEventAggregator.PublishOnUIThread( new ProgressEvent( (double)args.Progress / 1000, !args.Completed ));
		}

		public string ProgressPhase {
			get {  return( Get( () => ProgressPhase )); }
			set {  Set( () => ProgressPhase, value ); }
		}

		public string ProgressItem {
			get {  return( Get( () => ProgressItem )); }
			set {  Set( () => ProgressItem, value ); }
		}

		public int Progress {
			get {  return( Get( () => Progress )); }
			set {  Set( () => Progress, value ); }
		}

		public bool ProgressActive {
			get {  return( Get( () => ProgressActive )); }
			set {
				Set( () => ProgressActive, value );
				CanEdit = !ProgressActive;
			}
		}

		public bool CanEdit {
			get {  return( Get( () => CanEdit )); }
			set {  Set( () => CanEdit, value ); }
		}

		public void Execute_ImportLibrary() {
			if(!string.IsNullOrWhiteSpace( ImportPath )) {
				try {
					var backup = new LibraryBackup( DateTime.Now, Path.GetDirectoryName( ImportPath ));
					var library = LibraryConfiguration.LoadConfiguration( ImportPath );

					if( library != null ) {
						library.LibraryName = LibraryName;

						mEventAggregator.PublishOnUIThread( new ProgressEvent( 0.0D, true ));

						mLibrarian.ImportLibrary( library, backup, OnImportProgress );
					}
				}
				catch( Exception exception ) {
					mLog.LogException( $"Loading configuration from \"{ImportPath}\"", exception );
				}
			}
		}

		[DependsUpon( "ImportPath" )]
		[DependsUpon( "LibraryName" )]
		[DependsUpon( "ProgressActive" )]
		public bool CanExecute_ImportLibrary() {
			return(!string.IsNullOrWhiteSpace( ImportPath ) &&
				  (!ProgressActive ) &&
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
