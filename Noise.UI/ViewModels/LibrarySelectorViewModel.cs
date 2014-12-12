using System;
using System.Linq;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Behaviours;
using Noise.UI.Support;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class LibraryConfigurationInfo : InteractionRequestData<LibraryConfigurationDialogModel> {
		public LibraryConfigurationInfo( LibraryConfigurationDialogModel viewModel ) : base( viewModel ) { }
	}

	public class LibrarySelectorViewModel : AutomaticCommandBase,
											IHandle<Events.LibraryUpdateStarted>, IHandle<Events.LibraryUpdateCompleted>,
											IHandle<Events.LibraryChanged>, IHandle<Events.LibraryListChanged>, IHandle<Events.DatabaseStatisticsUpdated> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly ILibraryConfiguration	mLibraryConfiguration;
		private readonly ILibraryBuilder		mLibraryBuilder;
		private readonly IDialogService			mDialogService;
		private TaskHandler						mLibraryOpenTask;  
		private	readonly InteractionRequest<LibraryConfigurationInfo>	mLibraryConfigurationRequest;
		private readonly BindableCollection<LibraryConfiguration>		mLibraries;
		private string							mDatabaseStatistics;

		public LibrarySelectorViewModel( IEventAggregator eventAggregator, IDialogService dialogService,
										 ILibraryConfiguration libraryConfiguration, ILibraryBuilder libraryBuilder ) {
			mEventAggregator = eventAggregator;
			mDialogService = dialogService;
			mLibraryConfiguration = libraryConfiguration;
			mLibraryBuilder = libraryBuilder;

			mLibraryConfigurationRequest = new InteractionRequest<LibraryConfigurationInfo>();
			mLibraries = new BindableCollection<LibraryConfiguration>();
			mDatabaseStatistics = string.Empty;
			LoadLibraries();

			mEventAggregator.Subscribe( this );
		}

		private void LoadLibraries() {
			mLibraries.IsNotifying = false;
			mLibraries.Clear();
			mLibraries.AddRange( from library in mLibraryConfiguration.Libraries orderby library.LibraryName select library );
			mLibraries.IsNotifying = true;
			mLibraries.Refresh();
		}

		internal TaskHandler LibraryOpenTask {
			get {
				if( mLibraryOpenTask == null ) {
					Execute.OnUIThread( () => mLibraryOpenTask = new TaskHandler());
				}

				return( mLibraryOpenTask );
			}
			set{ mLibraryOpenTask = value; }
		}

		private void OpenLibrary( long libraryId ) {
			LibraryOpenTask.StartTask( () => mLibraryConfiguration.Open( libraryId ),
									   () => { },
									   ex => NoiseLogger.Current.LogException( "LibrarySelectorViewModel:OpenLibrary", ex ));
		}

		public void Handle( Events.LibraryChanged args ) {
			RaisePropertyChanged( () => CurrentLibrary );
			RaiseCanExecuteChangedEvent( "CanExecute_UpdateLibrary" );
		}

		public void Handle( Events.LibraryListChanged args ) {
			LoadLibraries();
		}

		public void Handle( Events.LibraryUpdateStarted message ) {
			RaiseCanExecuteChangedEvent( "CanExecute_UpdateLibrary" );
		}

		public void Handle( Events.LibraryUpdateCompleted message ) {
			RaiseCanExecuteChangedEvent( "CanExecute_UpdateLibrary" );
		}

		public void Handle( Events.DatabaseStatisticsUpdated message ) {
			mDatabaseStatistics = string.Format( "Artists: {0}, Albums: {1}", message.DatabaseStatistics.ArtistCount, message.DatabaseStatistics.AlbumCount );
			RaisePropertyChanged( () => LibraryStatistics );
		}

		public string LibraryStatistics {
			get {  return( mDatabaseStatistics ); }
		}

		public BindableCollection<LibraryConfiguration> LibraryList {
			get{ return( mLibraries ); }
		} 

		public LibraryConfiguration CurrentLibrary {
			get{ return( mLibraryConfiguration.Current ); }
			set {
				if( mLibraryConfiguration.Current != value ) {
					OpenLibrary( value.LibraryId );
				}

				RaisePropertyChanged( () => CurrentLibrary );
			}
		}

		public void Execute_UpdateLibrary() {
			mLibraryBuilder.StartLibraryUpdate();
		}

		public bool CanExecute_UpdateLibrary() {
			return(( mLibraryConfiguration.Current != null ) &&
			       (!mLibraryBuilder.LibraryUpdateInProgress ));
		}

		public void Execute_LibraryConfiguration() {
			try {
				var dialogModel = new LibraryConfigurationDialogModel( mEventAggregator, mDialogService, mLibraryConfiguration, mLibraryBuilder );

				mLibraryConfigurationRequest.Raise( new LibraryConfigurationInfo( dialogModel ));
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LibrarySelectorViewModel:Execute_LibraryConfiguration", ex );
			}
		}

		public IInteractionRequest LibraryConfigurationRequest {
			get{ return( mLibraryConfigurationRequest ); }
		}
	}
}
