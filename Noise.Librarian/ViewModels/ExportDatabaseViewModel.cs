using System.IO;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Librarian.Interfaces;
using Noise.Librarian.Models;
using Noise.UI.Support;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.Librarian.ViewModels {
	public class ExportDatabaseViewModel : AutomaticCommandBase,
										   IHandle<Events.SystemInitialized>, IHandle<Events.LibraryListChanged> {
		private readonly IEventAggregator							mEventAggregator;
		private readonly IDialogService								mDialogService;
		private readonly ILibraryConfiguration						mLibraryConfiguration;
		private readonly ILibrarian									mLibrarian;
		private readonly BindableCollection<LibraryConfiguration>	mLibraries; 
		private LibraryConfiguration								mCurrentLibrary;

		public ExportDatabaseViewModel( IEventAggregator eventAggregator, ILibrarian librarian, ILibraryConfiguration libraryConfiguration, IDialogService dialogService ) {
			mEventAggregator = eventAggregator;
			mLibrarian = librarian;
			mLibraryConfiguration = libraryConfiguration;
			mDialogService = dialogService;

			mLibraries = new BindableCollection<LibraryConfiguration>();
			ProgressActive = false;

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.SystemInitialized message ) {
			LoadLibraries();
		}

		public void Handle( Events.LibraryListChanged args ) {
			LoadLibraries();
		}

		private void LoadLibraries() {
			mLibraries.Clear();
			mLibraries.AddRange( mLibraryConfiguration.Libraries );

			CurrentLibrary = mLibraries.FirstOrDefault();
		}

		public BindableCollection<LibraryConfiguration> LibraryList {
			get {  return( mLibraries ); }
		}

		public LibraryConfiguration CurrentLibrary {
			get {  return( mCurrentLibrary ); }
			set {
				mCurrentLibrary = value;

				RaisePropertyChanged( () => CurrentLibrary );
			}
		}

		public string ExportPath {
			get {  return( Get( () => ExportPath )); }
			set {  Set( () => ExportPath, value ); }
		}

		public void Execute_ExportLibrary() {
			if(( mCurrentLibrary != null ) &&
			   ( Directory.Exists( ExportPath ))) {
				mEventAggregator.PublishOnUIThread( new ProgressEvent( 0.0D, true ));

				mLibrarian.ExportLibrary( mCurrentLibrary, ExportPath, OnExportProgress );
			}
		}

		[DependsUpon( "CurrentLibrary" )]
		[DependsUpon( "ExportPath" )]
		[DependsUpon( "ProgressActive")]
		public bool CanExecute_ExportLibrary() {
			return(( mCurrentLibrary != null ) &&
				   (!ProgressActive ) &&
				   ( Directory.Exists( ExportPath )));
		}

		private void OnExportProgress( ProgressReport args ) {
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

		public void Execute_Browse( object sender ) {
			string path = ExportPath;

			if( mDialogService.SelectFolderDialog( "Select export location:", ref path ).GetValueOrDefault( false )) {
				ExportPath = path;
			}
		}
	}
}
