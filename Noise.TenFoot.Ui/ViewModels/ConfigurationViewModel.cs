using Caliburn.Micro;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.UI.Support;

namespace Noise.TenFoot.Ui.ViewModels {
	public class ConfigurationViewModel : Screen {
		private readonly IDialogService	mDialogService;
		private string					mDatabaseName;
		private	string					mLibraryLocation;
		private	bool					mAllowInternet;
		private	bool					mEnableRemote;

		public ConfigurationViewModel( IDialogService dialogService ) {
			mDialogService = dialogService;
		}

		protected override void OnActivate() {
			base.OnActivate();

			DisplayName = "Configuration";

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				EnableRemote = configuration.EnableRemoteAccess;
				AllowInternet = configuration.HasNetworkAccess;
			}

			var databaseConfig = NoiseSystemConfiguration.Current.RetrieveConfiguration<DatabaseConfiguration>( DatabaseConfiguration.SectionName );

			if( databaseConfig != null ) {
				DatabaseName = databaseConfig.DatabaseName;
			}

			var libraryConfig = NoiseSystemConfiguration.Current.RetrieveConfiguration<StorageConfiguration>( StorageConfiguration.SectionName );

			if(( libraryConfig != null ) &&
			   ( libraryConfig.RootFolders != null )) {
				if( libraryConfig.RootFolders.Count == 0 ) {
					libraryConfig.RootFolders.Add( new RootFolderConfiguration());
				}

				LibraryLocation = libraryConfig.RootFolders[0].Path;
			}
		}

		private void UpdateConfiguration() {
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				configuration.EnableRemoteAccess = EnableRemote;
				configuration.HasNetworkAccess = AllowInternet;

				NoiseSystemConfiguration.Current.Save( configuration );
			}

			var databaseConfig = NoiseSystemConfiguration.Current.RetrieveConfiguration<DatabaseConfiguration>( DatabaseConfiguration.SectionName );

			if( databaseConfig != null ) {
				databaseConfig.DatabaseName = DatabaseName;

				NoiseSystemConfiguration.Current.Save( databaseConfig );
			}

			var libraryConfig = NoiseSystemConfiguration.Current.RetrieveConfiguration<StorageConfiguration>( StorageConfiguration.SectionName );
			if(( libraryConfig != null ) &&
			   ( libraryConfig.RootFolders != null )) {
				if( libraryConfig.RootFolders.Count == 0 ) {
					libraryConfig.RootFolders.Add( new RootFolderConfiguration());

					libraryConfig.RootFolders[0].StorageStrategy.Add( new FolderStrategyConfiguration( 0, eFolderStrategy.Artist ));
					libraryConfig.RootFolders[0].StorageStrategy.Add( new FolderStrategyConfiguration( 1, eFolderStrategy.Album ));
					libraryConfig.RootFolders[0].StorageStrategy.Add( new FolderStrategyConfiguration( 2, eFolderStrategy.Volume ));
				}

				var rootFolder = libraryConfig.RootFolders[0];
				rootFolder.Path = LibraryLocation;

				NoiseSystemConfiguration.Current.Save( libraryConfig );
			}
		}

		public string DatabaseName {
			get{ return( mDatabaseName ); }
			set {
				mDatabaseName = value;

				NotifyOfPropertyChange( () => DatabaseName );
			}
		}

		public string LibraryLocation {
			get{ return( mLibraryLocation ); }
			set {
				mLibraryLocation = value;

				NotifyOfPropertyChange( () => LibraryLocation );
			}
		}

		public bool AllowInternet {
			get{ return( mAllowInternet ); }
			set {
				mAllowInternet = value;

				NotifyOfPropertyChange( () => AllowInternet );
			}
		}

		public bool EnableRemote {
			get{ return( mEnableRemote ); }
			set {
				mEnableRemote = value;

				NotifyOfPropertyChange( () => EnableRemote );
			}
		}

		public void BrowseFolder() {
			string path = LibraryLocation;

			if( mDialogService.SelectFolderDialog( "Select Music Library", ref path ).GetValueOrDefault( false )) {
				LibraryLocation = path;
			}
		}

		public void Close() {
			TryClose( true );

			UpdateConfiguration();
		}

		public void Cancel() {
			TryClose( false );
		}
	}
}
