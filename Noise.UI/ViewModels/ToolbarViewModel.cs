using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class ToolbarViewModel : ViewModelBase {
		private readonly IEventAggregator		mEventAggregator;
		private readonly ICloudSyncManager		mCloudSyncMgr;
		private readonly IDataExchangeManager	mDataExchangeMgr;
		private readonly IDialogService			mDialogService;

		public ToolbarViewModel( IEventAggregator eventAggregator, IDialogService dialogService,
								 ICloudSyncManager cloudSyncManager, IDataExchangeManager dataExchangeManager ) {
			mEventAggregator = eventAggregator;
			mCloudSyncMgr = cloudSyncManager;
			mDataExchangeMgr = dataExchangeManager;
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

		public void Execute_ServerConfiguration() {
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ServerConfiguration>( ServerConfiguration.SectionName );

			if( mDialogService.ShowDialog( DialogNames.ServerConfiguration, configuration ) == true ) {
				NoiseSystemConfiguration.Current.Save( configuration );
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

		public void Execute_TimeExplorerLayout() {
			mEventAggregator.Publish( new Events.WindowLayoutRequest( Constants.TimeExplorerLayout ));
		}

		public void Execute_DisplayLog() {
			var dialogModel = new	ApplicationLogDialogModel();

			if( dialogModel.Initialize()) {
				mDialogService.ShowDialog( DialogNames.ApplicationLogView, dialogModel );
			}
		}
	}
}
