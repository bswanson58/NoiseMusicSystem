using System;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Ui.Platform;

namespace Noise.UI.ViewModels {
	internal class LibraryConfigurationDialogModel : AutomaticCommandBase, IDialogAware, IDisposable,
													 IHandle<Events.LibraryListChanged> {
		private readonly IEventAggregator							mEventAggregator;
		private readonly IPlatformDialogService						mDialogService;
		private readonly ILibraryConfiguration						mLibraryConfiguration;
		private readonly ILibraryBuilder							mLibraryBuilder;
		private readonly BindableCollection<LibraryConfiguration>	mLibraries;
		private LibraryConfiguration								mSelectedLibrary;
		private string												mLibraryName;
		private string												mDatabaseName;
		private string												mMediaPath;
		private bool												mIsDefaultLibrary;
        private bool                                                mCopyMetadata;
		private bool												mLibraryDirty;

        public IObservableCollection<LibraryConfiguration>			LibraryList => mLibraries;

        public  string                              Title { get; }
        public  DelegateCommand                     Ok { get; }
        public  event Action<IDialogResult>         RequestClose;

		public	DelegateCommand						UpdateLibrary { get; }

        public LibraryConfigurationDialogModel( IEventAggregator eventAggregator, IPlatformDialogService dialogService,
												ILibraryConfiguration libraryConfiguration, ILibraryBuilder libraryBuilder ) {
			mEventAggregator = eventAggregator;
			mDialogService = dialogService;
			mLibraryConfiguration = libraryConfiguration;
			mLibraryBuilder = libraryBuilder;
			mLibraries = new BindableCollection<LibraryConfiguration>();

			Ok = new DelegateCommand( OnOk );
			UpdateLibrary = new DelegateCommand( OnUpdateLibrary, CanUpdateLibrary );

			Title = "Library Configuration";

			mEventAggregator.Subscribe( this );
		}

        public void OnDialogOpened( IDialogParameters parameters ) {
            LoadLibraries();

            SelectedLibrary = mLibraryConfiguration.Current ?? mLibraryConfiguration.Libraries.FirstOrDefault();
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

        public LibraryConfiguration SelectedLibrary {
			get => mSelectedLibrary;
            set {
				mSelectedLibrary = value;

				if( mSelectedLibrary != null ) {
					mLibraryName = mSelectedLibrary.LibraryName;
					mDatabaseName = mSelectedLibrary.DatabaseName;
					mIsDefaultLibrary = mSelectedLibrary.IsDefaultLibrary;
                    mCopyMetadata = !mSelectedLibrary.IsMetadataInPlace;

					if( mSelectedLibrary.MediaLocations.Any()) {
						mMediaPath = mSelectedLibrary.MediaLocations[0].Path;
					}
				}
				else {
					mLibraryName = string.Empty;
					mDatabaseName = string.Empty;
					mMediaPath = string.Empty;
					mIsDefaultLibrary = false;
                    mCopyMetadata = true;
				}

				RaisePropertyChanged( () => SelectedLibrary );
			}
		}

		[DependsUpon( "SelectedLibrary")]
		public string DatabaseName {
			get => mDatabaseName;
            set {
				mDatabaseName = value;
				mLibraryDirty = true;

				RaisePropertyChanged( () => DatabaseName );
			}
		}

		[DependsUpon( "SelectedLibrary")]
		public string LibraryName {
			get => mLibraryName;
            set {
				mLibraryName = value;
				mLibraryDirty = true;

				RaisePropertyChanged( () => LibraryName );
			}
		}

		[DependsUpon( "SelectedLibrary" )]
		public string MediaPath {
			get => mMediaPath;
            set {
				mMediaPath = value;
				mLibraryDirty = true;

				RaisePropertyChanged( () => MediaPath );
			}
		}

		[DependsUpon( "SelectedLibrary" )]
		public bool IsDefaultLibrary {
			get => mIsDefaultLibrary;
            set {
				mIsDefaultLibrary = value;
				mLibraryDirty = true;

				RaisePropertyChanged( () => IsDefaultLibrary );
			}
		}

        [DependsUpon( "SelectedLibrary" )]
        public bool CopyMetadata {
            get => mCopyMetadata;
            set {
                mCopyMetadata = value;
                mLibraryDirty = true;

                RaisePropertyChanged( () => CopyMetadata );
            }
        }

		public void Execute_UpdateConfiguration() {
			if(( mLibraryDirty ) &&
			   ( mSelectedLibrary != null )) {
				mSelectedLibrary.LibraryName = mLibraryName;
				mSelectedLibrary.DatabaseName = mDatabaseName;
				mSelectedLibrary.IsDefaultLibrary = mIsDefaultLibrary;
                mSelectedLibrary.IsMetadataInPlace = !mCopyMetadata;

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
        [DependsUpon( "IsDefaultLibrary" )]
        [DependsUpon( "CopyMetadata" )]
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
		[DependsUpon( "IsDefaultLibrary" )]
        [DependsUpon( "CopyMetadata" )]
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

		private void OnUpdateLibrary() {
			if(!mLibraryBuilder.LibraryUpdateInProgress ) {
				mLibraryBuilder.StartLibraryUpdate();

				RaiseCanExecuteChangedEvent( "CanExecute_UpdateLibrary" );
			}
		}

		private bool CanUpdateLibrary() {
			return(!mLibraryBuilder.LibraryUpdateInProgress );
		}

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnOk() {

            RaiseRequestClose( new DialogResult( ButtonResult.OK ));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }

        public void Dispose() {
			mEventAggregator.Unsubscribe( this );
        }
    }
}
