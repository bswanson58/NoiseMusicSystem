using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class ToolbarViewModel : ViewModelBase,
									IHandle<Events.LibraryChanged>, IHandle<Events.LibraryListChanged> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly ILibraryConfiguration	mLibraryConfiguration;
		private readonly ICloudSyncManager		mCloudSyncMgr;
		private readonly IDataExchangeManager	mDataExchangeMgr;
		private readonly ILibraryBuilder		mLibraryBuilder;
		private readonly IDialogService			mDialogService;
		private readonly BindableCollection<LibraryConfiguration>	mLibraries;

		public ToolbarViewModel( IEventAggregator eventAggregator, IDialogService dialogService, ILibraryConfiguration libraryConfiguration,
								 ICloudSyncManager cloudSyncManager, IDataExchangeManager dataExchangeManager, ILibraryBuilder libraryBuilder ) {
			mEventAggregator = eventAggregator;
			mLibraryConfiguration = libraryConfiguration;
			mCloudSyncMgr = cloudSyncManager;
			mDataExchangeMgr = dataExchangeManager;
			mLibraryBuilder = libraryBuilder;
			mDialogService = dialogService;

			mLibraries = new BindableCollection<LibraryConfiguration>();
			LoadLibraries();

			mEventAggregator.Subscribe( this );

			var expConfig = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
			if(( expConfig != null ) &&
			   ( expConfig.LoadLastLibraryOnStartup )) {
				mLibraryConfiguration.Open( expConfig.LastLibraryUsed );
			}
		}

		private void LoadLibraries() {
			mLibraries.IsNotifying = false;
			mLibraries.Clear();
			mLibraries.AddRange( from library in mLibraryConfiguration.Libraries orderby library.LibraryName select library );
			mLibraries.IsNotifying = true;
			mLibraries.Refresh();
		}

		public void Handle( Events.LibraryChanged args ) {
			RaisePropertyChanged( () => CurrentLibrary );
		}

		public void Handle( Events.LibraryListChanged args ) {
			LoadLibraries();
		}

		public BindableCollection<LibraryConfiguration> LibraryList {
			get{ return( mLibraries ); }
		} 

		public LibraryConfiguration CurrentLibrary {
			get{ return( mLibraryConfiguration.Current ); }
			set {
				if( mLibraryConfiguration.Current != value ) {
					mLibraryConfiguration.Open( value );
				}

				RaisePropertyChanged( () => CurrentLibrary );
			}
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

		public void Execute_LibraryConfiguration() {
			mDialogService.ShowDialog( DialogNames.LibraryConfiguration,
											new LibraryConfigurationDialogModel( mEventAggregator, mDialogService,
																				 mLibraryConfiguration, mLibraryBuilder ));
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
