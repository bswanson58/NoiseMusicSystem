using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Logging;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
    class LibraryBackupDialogModel : DialogModelBase {
        private readonly ILibraryConfiguration  mLibraryConfiguration;
        private readonly IUiLog                 mLog;
        private LibraryConfiguration            mCurrentLibrary;

        public  ObservableCollectionEx<LibraryConfiguration>   Libraries { get; }

        public LibraryBackupDialogModel( ILibraryConfiguration configuration, IUiLog log ) {
            mLibraryConfiguration = configuration;
            mLog = log;

            Libraries = new ObservableCollectionEx<LibraryConfiguration>();
            Libraries.AddRange( mLibraryConfiguration.Libraries );
            CurrentLibrary = mLibraryConfiguration.Current;
        }

        public LibraryConfiguration CurrentLibrary {
            get => mCurrentLibrary;
            set {
                mCurrentLibrary = value;

                OnLibrarySelected();
                RaisePropertyChanged( () => CurrentLibrary );
            }
        }

        private void OnLibrarySelected() { }
    }
}
