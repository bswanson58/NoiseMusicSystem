using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Models;
using Noise.UI.Support;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class DisabledWindowCommandsViewModel :AutomaticCommandBase {
        public  ObservableCollection<UiCompanionApp>    CompanionApplications => null;
        public  bool									HaveCompanionApplications => false;

        public DisabledWindowCommandsViewModel( IPreferences preferences ) {
            var interfacePreferences = preferences.Load<UserInterfacePreferences>();

            ThemeManager.SetApplicationTheme( interfacePreferences.ThemeName, interfacePreferences.ThemeAccent, interfacePreferences.ThemeSignature );
        }

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

	    public void Execute_PlaybackLayout() { }
	    public bool CanExecute_PlaybackLayout() {
	        return (false);
	    }
	}

	public class WindowCommandsViewModel : AutomaticCommandBase {
		private readonly IDialogService			mDialogService;
		private readonly INoiseWindowManager	mWindowManager;
		private readonly IPreferences			mPreferences;
		private readonly IDataExchangeManager	mDataExchangeMgr;
		private readonly DelegateCommand		mImportCommand;

        public  ObservableCollection<UiCompanionApp>    CompanionApplications => mWindowManager.CompanionApplications;
        public  bool									HaveCompanionApplications => CompanionApplications.Any();

		public WindowCommandsViewModel( INoiseWindowManager windowManager, IDialogService dialogService, IPreferences preferences,
										IDataExchangeManager dataExchangeManager ) {
			mWindowManager = windowManager;
			mDialogService = dialogService;
			mPreferences = preferences;
			mDataExchangeMgr = dataExchangeManager;

			mImportCommand = new DelegateCommand( OnImport );
			GlobalCommands.ImportFavorites.RegisterCommand( mImportCommand );
			GlobalCommands.ImportRadioStreams.RegisterCommand( mImportCommand );
            GlobalCommands.ImportUserTags.RegisterCommand( mImportCommand );

			CompanionApplications.CollectionChanged += OnCollectionChanged;
		}

		private void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs args ) {
			RaisePropertyChanged( () => HaveCompanionApplications );
        }

		public void Execute_Options() {
			var dialogModel = new ConfigurationViewModel( mPreferences );

			if( mDialogService.ShowDialog( DialogNames.NoiseOptions, dialogModel ) == true ) {
                dialogModel.UpdatePreferences();
			}
		}

		public void Execute_LibraryLayout() {
			mWindowManager.ChangeWindowLayout( Constants.ExploreLayout );
		}

		public void Execute_ListenLayout() {
			mWindowManager.ChangeWindowLayout( Constants.ListenLayout );
		}

		public void Execute_TimelineLayout() {
			mWindowManager.ChangeWindowLayout( Constants.TimeExplorerLayout );
		}

		public void Execute_PlaybackLayout() {
			mWindowManager.ChangeWindowLayout( Constants.ListenLayout );
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
