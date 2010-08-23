using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Support;
using Noise.UI.Support;
using Noise.UI.Views;

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

				if( dialogService.ShowDialog( new ConfigurationDialog(), configuration ) == true ) {
					systemConfig.Save( configuration );
				}
			}
		}

		public void Execute_DatabaseConfiguration() {
			if( mContainer != null ) {
				var	dialogService = mContainer.Resolve<IDialogService>();
				var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
				var configuration = systemConfig.RetrieveConfiguration<DatabaseConfiguration>( DatabaseConfiguration.SectionName );

				if( dialogService.ShowDialog( new DatabaseConfigurationDialog(), configuration ) == true ) {
					systemConfig.Save( configuration );
				}
			}
		}

		public void Execute_SmallPlayerView() {
			if( mEvents != null ) {
				mEvents.GetEvent<Events.WindowLayoutRequest>().Publish( Constants.SmallPlayerView );
			}
		}

		public void Execute_LibraryLayout() {
			if( mEvents != null ) {
				mEvents.GetEvent<Events.WindowLayoutRequest>().Publish( Constants.LibraryLayout );
			}
		}

		public void Execute_StreamLayout() {
			if( mEvents != null ) {
				mEvents.GetEvent<Events.WindowLayoutRequest>().Publish( Constants.StreamLayout );
			}
		}
	}
}
