using System;
using System.Linq;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Behaviours;
using Noise.UI.Logging;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class LibraryConfigurationInfo : InteractionRequestData<LibraryConfigurationDialogModel> {
		public LibraryConfigurationInfo( LibraryConfigurationDialogModel viewModel ) : base( viewModel ) { }
	}

	internal class LibraryBackupInfo : InteractionRequestData<LibraryBackupDialogModel> {
		public LibraryBackupInfo( LibraryBackupDialogModel viewModel ) : base( viewModel ) { }
    }

	internal class LibrarySelectorViewModel : AutomaticCommandBase,
											  IHandle<Events.LibraryUpdateStarted>, IHandle<Events.LibraryUpdateCompleted>,
											  IHandle<Events.LibraryChanged>, IHandle<Events.LibraryListChanged>, 
                                              IHandle<Events.DatabaseStatisticsUpdated>, IHandle<Events.LibraryBackupPressureThreshold> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IUiLog					mLog;
		private readonly ILibraryConfiguration	mLibraryConfiguration;
		private readonly ILibraryBuilder		mLibraryBuilder;
		private readonly IPreferences			mPreferences;
		private readonly LibraryBackupDialogModel						mLibraryBackupDialog;
		private readonly LibraryConfigurationDialogModel				mLibraryConfigurationDialog;
		private readonly InteractionRequest<LibraryBackupInfo>			mLibraryBackupRequest;
		private	readonly InteractionRequest<LibraryConfigurationInfo>	mLibraryConfigurationRequest;
		private readonly BindableCollection<LibraryConfiguration>		mLibraries;
	    private TaskHandler						mLibraryOpenTask;  
		private string							mDatabaseStatistics;

	    public  string                                      LibraryStatistics => mDatabaseStatistics;
	    public  BindableCollection<LibraryConfiguration>    LibraryList => mLibraries;
		public	bool										BackupNeeded { get; private set; }

		public	IInteractionRequest							LibraryBackupRequest => mLibraryBackupRequest;
	    public  IInteractionRequest                         LibraryConfigurationRequest => mLibraryConfigurationRequest;

		public LibrarySelectorViewModel( IEventAggregator eventAggregator,
                                         LibraryBackupDialogModel libraryBackupDialog, LibraryConfigurationDialogModel libraryConfigurationDialog,
										 ILibraryConfiguration libraryConfiguration, ILibraryBuilder libraryBuilder, IPreferences preferences, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mPreferences = preferences;
			mLibraryConfiguration = libraryConfiguration;
			mLibraryBuilder = libraryBuilder;
			mLibraryBackupDialog = libraryBackupDialog;
			mLibraryConfigurationDialog = libraryConfigurationDialog;

			mLibraryBackupRequest = new InteractionRequest<LibraryBackupInfo>();
			mLibraryConfigurationRequest = new InteractionRequest<LibraryConfigurationInfo>();
			mLibraries = new BindableCollection<LibraryConfiguration>();

			LoadLibraries();
		    SetLibraryStatistics( mLibraryBuilder.LibraryStatistics );
            UpdateBackupNeeded();

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
			set => mLibraryOpenTask = value;
		}

		private void OpenLibrary( long libraryId ) {
			LibraryOpenTask.StartTask( () => mLibraryConfiguration.Open( libraryId ),
									   () => { },
									   ex => mLog.LogException( "Opening Library", ex ));
		}

		public void Handle( Events.LibraryChanged args ) {
			RaisePropertyChanged( () => CurrentLibrary );
			RaiseCanExecuteChangedEvent( "CanExecute_UpdateLibrary" );

			UpdateBackupNeeded();
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
            SetLibraryStatistics( message.DatabaseStatistics );
		}

		public void Handle( Events.LibraryBackupPressureThreshold args ) {
			UpdateBackupNeeded();
		}

        private void SetLibraryStatistics( IDatabaseStatistics statistics ) {
            mDatabaseStatistics = $"Artists: {statistics.ArtistCount}, Albums: {statistics.AlbumCount}";

            RaisePropertyChanged( () => LibraryStatistics );
        }

	    public LibraryConfiguration CurrentLibrary {
			get => ( mLibraryConfiguration.Current );
	        set {
				if( mLibraryConfiguration.Current != value ) {
					OpenLibrary( value.LibraryId );
				}

                RaisePropertyChanged( () => CurrentLibrary );
			}
		}

		private void UpdateBackupNeeded() {
            var preferences = mPreferences.Load<NoiseCorePreferences>();

			if(( preferences != null ) &&
               ( mLibraryConfiguration.Current != null )) {
                BackupNeeded = mLibraryConfiguration.Current.BackupPressure >= preferences.MaximumBackupPressure;
			}
			else {
				BackupNeeded = false;
            }

            RaisePropertyChanged( () => BackupNeeded );
        }

		public void Execute_UpdateLibrary() {
			mLibraryBuilder.StartLibraryUpdate();
		}

		public bool CanExecute_UpdateLibrary() {
			return(( mLibraryConfiguration.Current != null ) &&
			       (!mLibraryBuilder.LibraryUpdateInProgress ));
		}

		public void Execute_LibraryBackup() {
			try {
                mLibraryBackupDialog.Initialize();

				mLibraryBackupRequest.Raise( new LibraryBackupInfo( mLibraryBackupDialog ), OnLibraryBackupCompleted );
            }
			catch( Exception ex ) {
                mLog.LogException( "Executing Library Backup", ex );
            }
        }

		private void OnLibraryBackupCompleted( LibraryBackupInfo info ) {
			if( info.Confirmed ) {
				info.ViewModel.SaveOptions();
            }
        }

		public void Execute_LibraryConfiguration() {
			try {
                mLibraryConfigurationDialog.Initialize();

				mLibraryConfigurationRequest.Raise( new LibraryConfigurationInfo( mLibraryConfigurationDialog ));
			}
			catch( Exception ex ) {
				mLog.LogException( "Executing Library Configuration", ex );
			}
		}
	}
}
