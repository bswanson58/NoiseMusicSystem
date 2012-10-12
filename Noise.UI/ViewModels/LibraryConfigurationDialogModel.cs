using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	internal class LibraryConfigurationDialogModel : DialogModelBase, 
													 IHandle<Events.LibraryListChanged> {
		private readonly IEventAggregator							mEventAggregator;
		private readonly IDialogService								mDialogService;
		private readonly ILibraryConfiguration						mLibraryConfiguration;
		private readonly ILibraryBuilder							mLibraryBuilder;
		private readonly BindableCollection<LibraryConfiguration>	mLibraries;
		private LibraryConfiguration								mSelectedLibrary;
		private string												mLibraryName;
		private string												mDatabaseName;
		private string												mMediaPath;
		private bool												mLibraryDirty;

		public LibraryConfigurationDialogModel( IEventAggregator eventAggregator, IDialogService dialogService,
												ILibraryConfiguration libraryConfiguration, ILibraryBuilder libraryBuilder ) {
			mEventAggregator = eventAggregator;
			mDialogService = dialogService;
			mLibraryConfiguration = libraryConfiguration;
			mLibraryBuilder = libraryBuilder;
			mLibraries = new BindableCollection<LibraryConfiguration>();

			LoadLibraries();
			SelectedLibrary = mLibraryConfiguration.Current ?? mLibraryConfiguration.Libraries.FirstOrDefault();

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.LibraryListChanged args ) {
			LoadLibraries();
		}

		private void LoadLibraries() {
			var selectedLibrary = mSelectedLibrary;

			mLibraries.Clear();
			mLibraries.AddRange( from library in mLibraryConfiguration.Libraries orderby library.LibraryName select library );

			SelectedLibrary = selectedLibrary;
		}

		public IObservableCollection<LibraryConfiguration> LibraryList {
			get{ return( mLibraries ); }
		} 

		public LibraryConfiguration SelectedLibrary {
			get{ return( mSelectedLibrary ); }
			set {
				mSelectedLibrary = value;

				if( mSelectedLibrary != null ) {
					mLibraryName = mSelectedLibrary.LibraryName;
					mDatabaseName = mSelectedLibrary.DatabaseName;

					if( mSelectedLibrary.MediaLocations.Any()) {
						mMediaPath = mSelectedLibrary.MediaLocations[0].Path;
					}
				}
				else {
					mLibraryName = string.Empty;
					mDatabaseName = string.Empty;
					mMediaPath = string.Empty;
				}

				RaisePropertyChanged( () => SelectedLibrary );
			}
		}

		[DependsUpon( "SelectedLibrary")]
		public string DatabaseName {
			get { return( mDatabaseName ); }
			set {
				mDatabaseName = value;
				mLibraryDirty = true;

				RaisePropertyChanged( () => DatabaseName );
			}
		}

		[DependsUpon( "SelectedLibrary")]
		public string LibraryName {
			get { return( mLibraryName ); }
			set {
				mLibraryName = value;
				mLibraryDirty = true;

				RaisePropertyChanged( () => LibraryName );
			}
		}

		[DependsUpon( "SelectedLibrary" )]
		public string MediaPath {
			get{ return( mMediaPath ); }
			set {
				mMediaPath = value;
				mLibraryDirty = true;

				RaisePropertyChanged( () => MediaPath );
			}
		}

		public void Execute_UpdateConfiguration() {
			if(( mLibraryDirty ) &&
			   ( mSelectedLibrary != null )) {
				mSelectedLibrary.LibraryName = mLibraryName;
				mSelectedLibrary.DatabaseName = mDatabaseName;

				if(!mSelectedLibrary.MediaLocations.Any()) {
					var mediaLocation = new MediaLocation{ PreferFolderStrategy = true };

					mediaLocation.FolderStrategy[0] = eFolderStrategy.Artist;
					mediaLocation.FolderStrategy[1] = eFolderStrategy.Album;
					mediaLocation.FolderStrategy[2] = eFolderStrategy.Volume;

					mSelectedLibrary.MediaLocations.Add( mediaLocation );
				}
				mSelectedLibrary.MediaLocations[0].Path = mMediaPath;

				mLibraryConfiguration.UpdateLibrary( mSelectedLibrary );
				mLibraryDirty = false;

				RaiseCanExecuteChangedEvent( "CanExecute_UpdateConfiguration" );
			}
		}

		[DependsUpon( "MediaPath" )]
		[DependsUpon( "LibraryName" )]
		[DependsUpon( "DatabaseName" )]
		public bool CanExecute_UpdateConfiguration() {
			return(( mSelectedLibrary != null ) &&
				   ( mLibraryDirty ));
		}

		public void Execute_Browse( object sender ) {
			if( mSelectedLibrary != null ) {
				string path = MediaPath;

				if( mDialogService.SelectFolderDialog( "Select Music Library", ref path ).GetValueOrDefault( false )) {
					MediaPath = path;
				}
			}
		}

		public void Execute_CreateLibrary() {
			var newLibrary = new LibraryConfiguration { LibraryName = "New Library", DatabaseName = "Noise Database" };

			mLibraryConfiguration.AddLibrary( newLibrary );

			LoadLibraries();
			SelectedLibrary = newLibrary;
		}

		[DependsUpon( "MediaPath" )]
		[DependsUpon( "LibraryName" )]
		[DependsUpon( "DatabaseName" )]
		[DependsUpon( "SelectedLibrary" )]
		public bool CanExecute_CreateLibrary() {
			return(!mLibraryDirty );
		}

		public void Execute_OpenLibrary() {
			if( mSelectedLibrary != null ) {
				mLibraryConfiguration.Open( mSelectedLibrary );

				RaiseCanExecuteChangedEvent( "CanExecute_OpenLibrary" );
				RaiseCanExecuteChangedEvent( "CanExecute_CloseLibrary" );
			}
		}

		[DependsUpon( "SelectedLibrary" )]
		public bool CanExecute_OpenLibrary() {
			return(( mSelectedLibrary != null ) &&
			       ( mSelectedLibrary != mLibraryConfiguration.Current ));
		}

		public void Execute_CloseLibrary() {
			if(( mSelectedLibrary != null ) &&
			   ( mSelectedLibrary == mLibraryConfiguration.Current )) {
				mLibraryConfiguration.Close( mSelectedLibrary );

				RaiseCanExecuteChangedEvent( "CanExecute_OpenLibrary" );
				RaiseCanExecuteChangedEvent( "CanExecute_CloseLibrary" );
			}
		}

		[DependsUpon( "SelectedLibrary" )]
		public bool CanExecute_CloseLibrary() {
			return(( mSelectedLibrary != null ) &&
			       ( mSelectedLibrary == mLibraryConfiguration.Current ));
		}

		public void Execute_UpdateLibrary() {
			if(!mLibraryBuilder.LibraryUpdateInProgress ) {
				mLibraryBuilder.StartLibraryUpdate();

				RaiseCanExecuteChangedEvent( "CanExecute_UpdateLibrary" );
			}
		}

		public bool CanExecute_UpdateLibrary() {
			return(!mLibraryBuilder.LibraryUpdateInProgress );
		}
	}
}
