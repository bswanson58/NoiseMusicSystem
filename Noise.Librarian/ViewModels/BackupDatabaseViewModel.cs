using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Librarian.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.Librarian.ViewModels {
	public class BackupDatabaseViewModel : AutomaticCommandBase,
										   IHandle<Events.SystemInitialized>, IHandle<Events.LibraryListChanged> {
		private readonly ILibraryConfiguration						mLibraryConfiguration;
		private readonly ILibrarian									mLibrarian;
		private readonly BindableCollection<LibraryConfiguration>	mLibraries; 
		private LibraryConfiguration								mCurrentLibrary;

		public BackupDatabaseViewModel( IEventAggregator eventAggregator, ILibrarian librarian, ILibraryConfiguration libraryConfiguration ) {
			mLibrarian = librarian;
			mLibraryConfiguration = libraryConfiguration;

			mLibraries = new BindableCollection<LibraryConfiguration>();

			eventAggregator.Subscribe( this );
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

		public void Execute_BackupLibrary() {
			if( mCurrentLibrary != null ) {
				mLibrarian.BackupLibrary( mCurrentLibrary );
			}
		}

		[DependsUpon( "CurrentLibrary" )]
		public bool CanExecute_BackupLibrary() {
			return( mCurrentLibrary != null );
		}
	}
}
