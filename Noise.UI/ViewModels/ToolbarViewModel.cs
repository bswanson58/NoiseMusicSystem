using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class ToolbarViewModel : ViewModelBase {
		private IUnityContainer		mContainer;
		private IEventAggregator	mEvents;

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				if( mContainer != null ) {
					mEvents = mContainer.Resolve<IEventAggregator>();
				}
			}
		}

		public void Execute_NoiseOptions() {
			if( mContainer != null ) {
				var	dialogService = mContainer.Resolve<IDialogService>();
				var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
				var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

				if( dialogService.ShowDialog( DialogNames.NoiseOptions, configuration ) == true ) {
					systemConfig.Save( configuration );
				}
			}
		}

		public void Execute_DatabaseConfiguration() {
			if( mContainer != null ) {
				var	dialogService = mContainer.Resolve<IDialogService>();
				var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
				var configuration = systemConfig.RetrieveConfiguration<DatabaseConfiguration>( DatabaseConfiguration.SectionName );

				if( dialogService.ShowDialog( DialogNames.DatabaseConfiguration, configuration, new DatabaseConfigurationDialogModel( dialogService )) == true ) {
					systemConfig.Save( configuration );
				}
			}
		}

		public void Execute_ServerConfiguration() {
			if( mContainer != null ) {
				var	dialogService = mContainer.Resolve<IDialogService>();
				var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
				var configuration = systemConfig.RetrieveConfiguration<ServerConfiguration>( ServerConfiguration.SectionName );

				if( dialogService.ShowDialog( DialogNames.ServerConfiguration, configuration ) == true ) {
					systemConfig.Save( configuration );
				}
			}
		}

		public void Execute_LibraryConfiguration() {
			if( mContainer != null ) {
				var	dialogService = mContainer.Resolve<IDialogService>();
				var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
				var configuration = systemConfig.RetrieveConfiguration<StorageConfiguration>( StorageConfiguration.SectionName );
				var noiseManager = mContainer.Resolve<INoiseManager>();

				if( configuration.RootFolders.Count == 0 ) {
					configuration.RootFolders.Add( new RootFolderConfiguration());
				}
				var rootFolder = configuration.RootFolders[0];

				if( dialogService.ShowDialog( DialogNames.LibraryConfiguration, rootFolder,
											  new LibraryConfigurationDialogModel( dialogService, noiseManager.LibraryBuilder )) == true ) {
					if( rootFolder.PreferFolderStrategy ) {
						rootFolder.StorageStrategy.Add( new FolderStrategyConfiguration( 0, eFolderStrategy.Artist ));
						rootFolder.StorageStrategy.Add( new FolderStrategyConfiguration( 1, eFolderStrategy.Album ));
						rootFolder.StorageStrategy.Add( new FolderStrategyConfiguration( 2, eFolderStrategy.Volume ));
					}

					systemConfig.Save( configuration );
				}
			}
		}

		public void Execute_SmallPlayerView() {
			if( mEvents != null ) {
				mEvents.GetEvent<Events.WindowLayoutRequest>().Publish( Constants.SmallPlayerViewToggle );
			}
		}

		public void Execute_ExploreLayout() {
			if( mEvents != null ) {
				mEvents.GetEvent<Events.WindowLayoutRequest>().Publish( Constants.ExploreLayout );
			}
		}

		public void Execute_ListenLayout() {
			if( mEvents != null ) {
				mEvents.GetEvent<Events.WindowLayoutRequest>().Publish( Constants.ListenLayout );
			}
		}

		public void Execute_DisplayLog() {
			if( mContainer != null ) {
				var applicationLog = new ApplicationLogReader();

				if( applicationLog.ReadLog( Constants.ApplicationLogName )) {
					var dialogModel = new	ApplicationLogDialogModel();

					if( dialogModel.Initialize()) {
						var	dialogService = mContainer.Resolve<IDialogService>();
					
						dialogService.ShowDialog( DialogNames.ApplicationLogView, dialogModel );
					}
				}
			}
		}
	}
}
