﻿using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Ui.Platform;

namespace Noise.UI.ViewModels {
	public class StartupLibraryCreationViewModel : AutomaticPropertyBase {
		private readonly IEventAggregator		mEventAggregator;
		private readonly ILibraryConfiguration	mLibraryConfiguration;
		private readonly IPlatformDialogService	mDialogService;
		private readonly ILibraryBuilder		mLibraryBuilder;

        public	bool							IsNotLoading => !IsLoading;

        public	DelegateCommand					CreateLibrary { get; }
		public	DelegateCommand					Browse { get; }
		public	DelegateCommand					Exit {  get; }

		public StartupLibraryCreationViewModel( IEventAggregator eventAggregator, IPlatformDialogService dialogService,
												ILibraryConfiguration libraryConfiguration, ILibraryBuilder libraryBuilder ) {
			mEventAggregator = eventAggregator;
			mLibraryConfiguration = libraryConfiguration;
			mLibraryBuilder = libraryBuilder;
			mDialogService = dialogService;

			CreateLibrary = new DelegateCommand( OnCreateLibrary, CanCreateLibrary );
			Browse = new DelegateCommand( OnBrowse );
			Exit = new DelegateCommand( OnExit );

            LibraryName = string.Empty;
            LibraryPath = string.Empty;
		}

		private async void OnCreateLibrary() {
			IsLoading = true;

			var mediaLocation = new MediaLocation { Path = LibraryPath, PreferFolderStrategy = true };

			mediaLocation.FolderStrategy[0] = eFolderStrategy.Artist;
			mediaLocation.FolderStrategy[1] = eFolderStrategy.Album;
			mediaLocation.FolderStrategy[2] = eFolderStrategy.Volume;

			var configuration = new LibraryConfiguration { LibraryName = LibraryName, DatabaseName = LibraryName };

			configuration.MediaLocations.Add( mediaLocation );

			mLibraryConfiguration.AddLibrary( configuration );
			await mLibraryConfiguration.AsyncOpen( configuration );

			mEventAggregator.PublishOnUIThread( new Events.WindowLayoutRequest( Constants.ExploreLayout ));
			mLibraryBuilder.StartLibraryUpdate();
		}

		public string LibraryName {
			get{ return( Get( () => LibraryName )); }
			set {
                Set( () => LibraryName, value );

				CreateLibrary.RaiseCanExecuteChanged();
            }
		}

		public string LibraryPath {
			get { return ( Get( () => LibraryPath )); }
			set {
                Set( () => LibraryPath, value );

				CreateLibrary.RaiseCanExecuteChanged();
            }
		}

		public bool IsLoading {
			get { return ( Get( () => IsLoading ) ); }
			set {
                Set( () => IsLoading, value );

				RaisePropertyChanged( () => IsLoading );
				RaisePropertyChanged( () => IsNotLoading );
            }
		}


		private bool CanCreateLibrary() {
			return((!string.IsNullOrWhiteSpace( LibraryName )) &&
				   (!string.IsNullOrWhiteSpace( LibraryPath )));
		}

		private void OnBrowse() {
			string path = LibraryPath;

			if( mDialogService.SelectFolderDialog( "Select Music Location", ref path ).GetValueOrDefault( false )) {
				LibraryPath = path;
			}
		}

		private void OnExit() {
			mEventAggregator.PublishOnUIThread( new Events.WindowLayoutRequest( Constants.ExploreLayout ));
		}
	}
}
