using Caliburn.Micro;
using Microsoft.Practices.Prism.Commands;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class DisabledWindowCommandsViewModel :AutomaticCommandBase {
		public void Execute_Options() { }
		public bool CanExecute_Options() {
			return( false );
		}

		public void Execute_LogView() { }
		public bool CanExecute_LogView() {
			return ( false );
		}

		public void Execute_LibraryLayout() { }
		public bool CanExecute_LibraryLayout() {
			return ( false );
		}

		public void Execute_ListenLayout() { }
		public bool CanExecute_ListenLayout() {
			return ( false );
		}

		public void Execute_TimelineLayout() { }
		public bool CanExecute_TimelineLayout() {
			return ( false );
		}
	}

	internal class ConfigurationViewModel {
		public bool EnableGlobalHotkeys { get; set; }
		public bool EnableRemoteAccess { get; set; }
		public bool EnableSortPrefixes { get; set; }
		public bool HasNetworkAccess { get; set; }
		public bool LoadLastLibraryOnStartup { get; set; }
		public bool MinimizeToTray { get; set; }
		public string SortPrefixes { get; set; }
	}

	public class WindowCommandsViewModel : AutomaticCommandBase {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IDialogService			mDialogService;
		private readonly IPreferences			mPreferences;
		private readonly IDataExchangeManager	mDataExchangeMgr;
		private readonly DelegateCommand		mImportCommand;

		public WindowCommandsViewModel( IEventAggregator eventAggregator, IDialogService dialogService, IPreferences preferences,
										IDataExchangeManager dataExchangeManager ) {
			mEventAggregator = eventAggregator;
			mDialogService = dialogService;
			mPreferences = preferences;
			mDataExchangeMgr = dataExchangeManager;

			mImportCommand = new DelegateCommand( OnImport );
			GlobalCommands.ImportFavorites.RegisterCommand( mImportCommand );
			GlobalCommands.ImportRadioStreams.RegisterCommand( mImportCommand );
		}

		public void Execute_Options() {
			var interfacePreferences = mPreferences.Load<UserInterfacePreferences>();
			var corePreferences = mPreferences.Load<NoiseCorePreferences>();
			var dialogModel = new ConfigurationViewModel { EnableGlobalHotkeys = interfacePreferences.EnableGlobalHotkeys,
														   EnableRemoteAccess = corePreferences.EnableRemoteAccess,
														   EnableSortPrefixes = interfacePreferences.EnableSortPrefixes,
														   HasNetworkAccess = corePreferences.HasNetworkAccess,
														   LoadLastLibraryOnStartup = corePreferences.LoadLastLibraryOnStartup,
														   MinimizeToTray = interfacePreferences.MinimizeToTray,
														   SortPrefixes = interfacePreferences.SortPrefixes };

			if( mDialogService.ShowDialog( DialogNames.NoiseOptions, dialogModel ) == true ) {
				corePreferences.EnableRemoteAccess = dialogModel.EnableRemoteAccess;
				corePreferences.HasNetworkAccess = dialogModel.HasNetworkAccess;
				corePreferences.LoadLastLibraryOnStartup = dialogModel.LoadLastLibraryOnStartup;

				interfacePreferences.EnableGlobalHotkeys = dialogModel.EnableGlobalHotkeys;
				interfacePreferences.EnableSortPrefixes = dialogModel.EnableSortPrefixes;
				interfacePreferences.SortPrefixes = dialogModel.SortPrefixes;
				interfacePreferences.MinimizeToTray = dialogModel.MinimizeToTray;

				mPreferences.Save( corePreferences );
				mPreferences.Save( interfacePreferences );
			}
		}

		public void Execute_LogView() {
			var dialogModel = new	ApplicationLogDialogModel();

			if( dialogModel.Initialize()) {
				mDialogService.ShowDialog( DialogNames.ApplicationLogView, dialogModel );
			}
		}

		public void Execute_LibraryLayout() {
			mEventAggregator.Publish( new Events.WindowLayoutRequest( Constants.ExploreLayout ));
		}

		public void Execute_ListenLayout() {
			mEventAggregator.Publish( new Events.WindowLayoutRequest( Constants.ListenLayout ));
		}

		public void Execute_TimelineLayout() {
			mEventAggregator.Publish( new Events.WindowLayoutRequest( Constants.TimeExplorerLayout ));
		}

		public void Execute_PlaybackLayout() {
			mEventAggregator.Publish( new Events.WindowLayoutRequest( Constants.ListenLayout ));
		}

		private void OnImport() {
			string	fileName;

			if( mDialogService.OpenFileDialog( "Import", Constants.ExportFileExtension, "Export Files|*" + Constants.ExportFileExtension, out fileName ) == true ) {

				var	importCount = mDataExchangeMgr.Import( fileName, true );
				var	importMessage = importCount > 0 ? string.Format( "{0} item(s) were imported.", importCount ) : "No items were imported.";

				mDialogService.MessageDialog( "Import Results", importMessage );
			}
		}
	}
}
