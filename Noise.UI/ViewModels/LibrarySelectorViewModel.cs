using System;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Logging;
using Noise.UI.Views;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class LibrarySelectorViewModel : PropertyChangeBase, IDisposable,
											  IHandle<Events.LibraryUpdateStarted>, IHandle<Events.LibraryUpdateCompleted>,
											  IHandle<Events.LibraryChanged>, IHandle<Events.LibraryListChanged>, IHandle<Events.DatabaseStatisticsUpdated>, 
                                              IHandle<Events.LibraryBackupPressure>, IHandle<Events.LibraryBackupPressureThreshold> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IUiLog					mLog;
		private readonly ILibraryConfiguration	mLibraryConfiguration;
		private readonly ILibraryBuilder		mLibraryBuilder;
		private readonly IPreferences			mPreferences;
		private readonly IDialogService			mDialogService;
		private readonly BindableCollection<LibraryConfiguration>	mLibraries;
	    private TaskHandler						mLibraryOpenTask;  
		private string							mDatabaseStatistics;

	    public  string                                      LibraryStatistics => mDatabaseStatistics;
	    public  BindableCollection<LibraryConfiguration>    LibraryList => mLibraries;
		public	bool										BackupNeeded { get; private set; }
		public	double										BackupPressurePercentage { get; private set; }

		public	DelegateCommand								LibraryConfiguration { get; }
		public	DelegateCommand								LibraryBackup { get; }
		public	DelegateCommand								UpdateLibrary { get; }

		public LibrarySelectorViewModel( IEventAggregator eventAggregator, IDialogService dialogService,
										 ILibraryConfiguration libraryConfiguration, ILibraryBuilder libraryBuilder, IPreferences preferences, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mPreferences = preferences;
			mLibraryConfiguration = libraryConfiguration;
			mLibraryBuilder = libraryBuilder;
			mDialogService = dialogService;

            mLibraries = new BindableCollection<LibraryConfiguration>();

			LibraryBackup = new DelegateCommand( OnLibraryBackup );
			LibraryConfiguration = new DelegateCommand( OnLibraryConfiguration );
			UpdateLibrary = new DelegateCommand( OnUpdateLibrary, CanUpdateLibrary );

			LoadLibraries();
		    SetLibraryStatistics( mLibraryBuilder.LibraryStatistics );
            UpdateBackupNeeded();
			UpdateBackupPressure();

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
			UpdateLibrary.RaiseCanExecuteChanged();

			UpdateBackupNeeded();
			UpdateBackupPressure();
		}

		public void Handle( Events.LibraryListChanged args ) {
			LoadLibraries();
		}

		public void Handle( Events.LibraryUpdateStarted message ) {
			UpdateLibrary.RaiseCanExecuteChanged();
		}

		public void Handle( Events.LibraryUpdateCompleted message ) {
			UpdateLibrary.RaiseCanExecuteChanged();
		}

		public void Handle( Events.DatabaseStatisticsUpdated message ) {
            SetLibraryStatistics( message.DatabaseStatistics );
		}

		public void Handle( Events.LibraryBackupPressure args ) {
			UpdateBackupPressure();
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

        private void UpdateBackupPressure() {
            if( CurrentLibrary != null ) {
                var preferences = mPreferences.Load<NoiseCorePreferences>();

                BackupPressurePercentage = Math.Min( 1.0, (double)CurrentLibrary.BackupPressure / preferences.MaximumBackupPressure );

                RaisePropertyChanged( () => BackupPressurePercentage );
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

		private void OnUpdateLibrary() {
			mLibraryBuilder.StartLibraryUpdate();
		}

		private bool CanUpdateLibrary() {
			return(( mLibraryConfiguration.Current != null ) &&
			       (!mLibraryBuilder.LibraryUpdateInProgress ));
		}

		private void OnLibraryBackup() {
			try {
				mDialogService.ShowDialog( nameof( LibraryBackupDialog ), new DialogParameters(), result => { });
            }
			catch( Exception ex ) {
                mLog.LogException( "Executing Library Backup", ex );
            }
        }

        private void OnLibraryConfiguration() {
			try {
				mDialogService.ShowDialog( nameof( LibraryConfigurationDialog ), new DialogParameters(), result => {});
			}
			catch( Exception ex ) {
				mLog.LogException( "Executing Library Configuration", ex );
			}
		}

        public void Dispose() {
			mEventAggregator.Unsubscribe( this );
        }
    }
}
