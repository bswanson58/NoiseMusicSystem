using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Librarian.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.Librarian.ViewModels {
	public class RestoreDatabaseViewModel : AutomaticCommandBase,
											IHandle<Events.SystemInitialized> {
		private readonly ILibraryConfiguration						mLibraryConfiguration;
		private readonly ILibrarian									mLibrarian;
		private readonly BindableCollection<LibraryConfiguration>	mLibraries; 
		private readonly BindableCollection<LibraryBackup>			mLibraryBackups; 
		private LibraryConfiguration								mCurrentLibrary;
		private LibraryBackup										mCurrentBackup;

		public RestoreDatabaseViewModel( IEventAggregator eventAggregator, ILibrarian librarian, ILibraryConfiguration libraryConfiguration ) {
			mLibrarian = librarian;
			mLibraryConfiguration = libraryConfiguration;

			mLibraries = new BindableCollection<LibraryConfiguration>();
			mLibraryBackups = new BindableCollection<LibraryBackup>();

			eventAggregator.Subscribe( this );
		}

		public void Handle( Events.SystemInitialized message ) {
			mLibraries.Clear();
			mLibraries.AddRange( mLibraryConfiguration.Libraries );

			CurrentLibrary = mLibraries.FirstOrDefault();

			mLibraryBackups.Clear();
			if( CurrentLibrary != null ) {
				mLibraryBackups.AddRange( from backup in mLibraryConfiguration.GetLibraryBackups( CurrentLibrary )
										  orderby backup.BackupDate descending 
										  select backup );
			}

			CurrentBackup = mLibraryBackups.FirstOrDefault();
		}

		public void Execute_RestoreLibrary() {
			if(( CurrentLibrary != null ) &&
			   ( CurrentBackup != null )) {
				mLibrarian.RestoreLibrary( CurrentLibrary, CurrentBackup );
			}
		}

		[DependsUpon( "CurrentBackup" )]
		public bool CanExecute_RestoreLibrary() {
			return(( CurrentLibrary != null ) &&
				   ( CurrentBackup != null ));
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

		public BindableCollection<LibraryBackup> BackupList {
			get {  return( mLibraryBackups ); }
		}

		public LibraryBackup CurrentBackup {
			get {  return( mCurrentBackup ); }
			set {
				mCurrentBackup = value;

				RaisePropertyChanged( () => CurrentBackup );
			}
		}
	}
}
