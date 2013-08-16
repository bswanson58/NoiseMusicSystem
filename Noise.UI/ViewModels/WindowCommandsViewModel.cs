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

	public class WindowCommandsViewModel : AutomaticCommandBase {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IDialogService			mDialogService;
		private readonly IDataExchangeManager	mDataExchangeMgr;
		private readonly DelegateCommand		mImportCommand;

		public WindowCommandsViewModel( IEventAggregator eventAggregator, IDialogService dialogService, IDataExchangeManager dataExchangeManager ) {
			mEventAggregator = eventAggregator;
			mDialogService = dialogService;
			mDataExchangeMgr = dataExchangeManager;

			mImportCommand = new DelegateCommand( OnImport );
			GlobalCommands.ImportFavorites.RegisterCommand( mImportCommand );
			GlobalCommands.ImportRadioStreams.RegisterCommand( mImportCommand );
		}

		public void Execute_Options() {
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( mDialogService.ShowDialog( DialogNames.NoiseOptions, configuration ) == true ) {
				NoiseSystemConfiguration.Current.Save( configuration );
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
