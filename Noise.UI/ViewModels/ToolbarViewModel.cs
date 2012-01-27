using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class ToolbarViewModel : ViewModelBase {
		private readonly ICaliburnEventAggregator	mEventAggregator;
		private readonly ICloudSyncManager		mCloudSyncMgr;
		private readonly IDataExchangeManager	mDataExchangeMgr;
		private readonly ILibraryBuilder		mLibraryBuilder;
		private readonly IDialogService			mDialogService;

		public ToolbarViewModel( ICaliburnEventAggregator eventAggregator, IDialogService dialogService, 
								 ICloudSyncManager cloudSyncManager, IDataExchangeManager dataExchangeManager, ILibraryBuilder libraryBuilder ) {
			mEventAggregator = eventAggregator;
			mCloudSyncMgr = cloudSyncManager;
			mDataExchangeMgr = dataExchangeManager;
			mLibraryBuilder = libraryBuilder;
			mDialogService = dialogService;
		}

		public void Execute_NoiseOptions() {
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( mDialogService.ShowDialog( DialogNames.NoiseOptions, configuration ) == true ) {
				NoiseSystemConfiguration.Current.Save( configuration );
			}
		}

		public void Execute_CloudConfiguration() {
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<CloudSyncConfiguration>( CloudSyncConfiguration.SectionName );

			if( mDialogService.ShowDialog( DialogNames.CloudConfiguration, configuration, new CloudConfigurationDialogModel()) == true ) {
				NoiseSystemConfiguration.Current.Save( configuration );

				mCloudSyncMgr.MaintainSynchronization = configuration.UseCloud;
			}
		}

		public void Execute_DatabaseConfiguration() {
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<DatabaseConfiguration>( DatabaseConfiguration.SectionName );

			if( mDialogService.ShowDialog( DialogNames.DatabaseConfiguration, configuration, new DatabaseConfigurationDialogModel( mDialogService )) == true ) {
				NoiseSystemConfiguration.Current.Save( configuration );
			}
		}

		public void Execute_ServerConfiguration() {
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ServerConfiguration>( ServerConfiguration.SectionName );

			if( mDialogService.ShowDialog( DialogNames.ServerConfiguration, configuration ) == true ) {
				NoiseSystemConfiguration.Current.Save( configuration );
			}
		}

		public void Execute_LibraryConfiguration() {
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<StorageConfiguration>( StorageConfiguration.SectionName );

			if( configuration.RootFolders.Count == 0 ) {
				configuration.RootFolders.Add( new RootFolderConfiguration());
			}
			var rootFolder = configuration.RootFolders[0];

			if( mDialogService.ShowDialog( DialogNames.LibraryConfiguration, rootFolder,
											new LibraryConfigurationDialogModel( mDialogService, mLibraryBuilder )) == true ) {
				if( rootFolder.PreferFolderStrategy ) {
					rootFolder.StorageStrategy.Add( new FolderStrategyConfiguration( 0, eFolderStrategy.Artist ));
					rootFolder.StorageStrategy.Add( new FolderStrategyConfiguration( 1, eFolderStrategy.Album ));
					rootFolder.StorageStrategy.Add( new FolderStrategyConfiguration( 2, eFolderStrategy.Volume ));
				}

				NoiseSystemConfiguration.Current.Save( configuration );
				mEventAggregator.Publish( new Events.SystemConfigurationChanged());
			}
		}

		public void Execute_Import() {
			string	fileName;

			if( mDialogService.OpenFileDialog( "Import", Constants.ExportFileExtension, "Export Files|*" + Constants.ExportFileExtension, out fileName ) == true ) {

				var	importCount = mDataExchangeMgr.Import( fileName, true );
				var	importMessage = importCount > 0 ? string.Format( "{0} item(s) were imported.", importCount ) : "No items were imported.";

				mDialogService.MessageDialog( "Import Results", importMessage );
			}
		}

		public void Execute_SmallPlayerView() {
			mEventAggregator.Publish( new Events.WindowLayoutRequest( Constants.SmallPlayerViewToggle ));
		}

		public void Execute_ExploreLayout() {
			mEventAggregator.Publish( new Events.WindowLayoutRequest( Constants.ExploreLayout ));
		}

		public void Execute_ListenLayout() {
			mEventAggregator.Publish( new Events.WindowLayoutRequest( Constants.ListenLayout ));
		}

		public void Execute_DisplayLog() {
			var dialogModel = new	ApplicationLogDialogModel();

			if( dialogModel.Initialize()) {
				mDialogService.ShowDialog( DialogNames.ApplicationLogView, dialogModel );
			}
		}
	}
}
