using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.Librarian.Models;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.Librarian.ViewModels {
	public class RestoreDatabaseViewModel : AutomaticCommandBase,
											IHandle<Events.SystemInitialized>, IHandle<Events.LibraryListChanged>, IHandle<Events.LibraryBackupsChanged> {
		private readonly IEventAggregator							mEventAggregator;
		private readonly ILibraryConfiguration						mLibraryConfiguration;
		private readonly ILibrarian									mLibrarian;
		private readonly ObservableCollectionEx<LibraryConfiguration>	mLibraries; 
		private readonly ObservableCollectionEx<LibraryBackup>		mLibraryBackups; 
		private LibraryConfiguration								mCurrentLibrary;
		private LibraryBackup										mCurrentBackup;

        public ObservableCollectionEx<LibraryBackup>				BackupList => mLibraryBackups;
        public ObservableCollectionEx<LibraryConfiguration>			LibraryList => mLibraries;

		public RestoreDatabaseViewModel( IEventAggregator eventAggregator, ILibrarian librarian, ILibraryConfiguration libraryConfiguration ) {
			mEventAggregator = eventAggregator;
			mLibrarian = librarian;
			mLibraryConfiguration = libraryConfiguration;

			mLibraries = new ObservableCollectionEx<LibraryConfiguration>();
			mLibraryBackups = new ObservableCollectionEx<LibraryBackup>();
			ProgressActive = false;

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.SystemInitialized message ) {
			LoadLibraries();
		}

		public void Handle( Events.LibraryListChanged args ) {
			LoadLibraries();
		}

		public void Handle( Events.LibraryBackupsChanged args ) {
			LoadBackups();
		}

		private void LoadLibraries() {
			mLibraries.Clear();
			mLibraries.AddRange( mLibraryConfiguration.Libraries );

			CurrentLibrary = mLibraries.FirstOrDefault();
			LoadBackups();
		}

		private void LoadBackups() {
			Execute.OnUIThread( () => {
                mLibraryBackups.Clear();
                if( CurrentLibrary != null ) {
                    mLibraryBackups.AddRange( from backup in mLibraryConfiguration.GetLibraryBackups( CurrentLibrary )
                        orderby backup.BackupDate descending 
                        select backup );
                }

                CurrentBackup = mLibraryBackups.FirstOrDefault();
            } );
		}

		private void OnRestoreProgress( LibrarianProgressReport args ) {
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

		public void Execute_RestoreLibrary() {
			if(( CurrentLibrary != null ) &&
			   ( CurrentBackup != null )) {
				mEventAggregator.PublishOnUIThread( new ProgressEvent( 0.0D, true ));

				mLibrarian.RestoreLibrary( CurrentLibrary, CurrentBackup, OnRestoreProgress );
			}
		}

		[DependsUpon( "CurrentBackup" )]
		[DependsUpon( "ProgressActive" )]
		public bool CanExecute_RestoreLibrary() {
			return(( CurrentLibrary != null ) &&
				   (!ProgressActive ) &&
				   ( CurrentBackup != null ));
		}

        public LibraryConfiguration CurrentLibrary {
			get => mCurrentLibrary;
            set {
				mCurrentLibrary = value;

				LoadBackups();

				RaisePropertyChanged( () => CurrentLibrary );
			}
		}

        public LibraryBackup CurrentBackup {
			get => mCurrentBackup;
            set {
				mCurrentBackup = value;

				RaisePropertyChanged( () => CurrentBackup );
			}
		}
	}
}
