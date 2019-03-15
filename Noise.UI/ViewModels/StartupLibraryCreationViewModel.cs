using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class StartupLibraryCreationViewModel : AutomaticCommandBase {
		private readonly IEventAggregator		mEventAggregator;
		private readonly ILibraryConfiguration	mLibraryConfiguration;
		private readonly IDialogService			mDialogService;
		private readonly ILibraryBuilder		mLibraryBuilder;

		public StartupLibraryCreationViewModel( IEventAggregator eventAggregator, IDialogService dialogService,
												ILibraryConfiguration libraryConfiguration, ILibraryBuilder libraryBuilder ) {
			mEventAggregator = eventAggregator;
			mLibraryConfiguration = libraryConfiguration;
			mLibraryBuilder = libraryBuilder;
			mDialogService = dialogService;

			LibraryName = string.Empty;
			LibraryPath = string.Empty;
		}

		public async void Execute_CreateLibrary() {
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
			set{ Set( () => LibraryName, value ); }
		}

		public string LibraryPath {
			get { return ( Get( () => LibraryPath )); }
			set { Set( () => LibraryPath, value ); }
		}

		public bool IsLoading {
			get { return ( Get( () => IsLoading ) ); }
			set { Set( () => IsLoading, value ); }
		}

		[DependsUpon( "IsLoading" )]
		public bool IsNotLoading {
			get { return ( !IsLoading ); }
		}

		[DependsUpon( "LibraryName" )]
		[DependsUpon( "LibraryPath" )]
		public bool CanExecute_CreateLibrary() {
			return((!string.IsNullOrWhiteSpace( LibraryName )) &&
				   (!string.IsNullOrWhiteSpace( LibraryPath )));
		}

		public void Execute_Browse( object sender ) {
			string path = LibraryPath;

			if( mDialogService.SelectFolderDialog( "Select Music Location", ref path ).GetValueOrDefault( false )) {
				LibraryPath = path;
			}
		}

		public void Execute_Exit() {
			mEventAggregator.PublishOnUIThread( new Events.WindowLayoutRequest( Constants.ExploreLayout ));
		}
	}
}
