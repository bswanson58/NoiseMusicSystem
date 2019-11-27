using Caliburn.Micro;
using Prism.Ioc;
using Prism.Modularity;
using ReusableBits.Mvvm.VersionSpinner;
using TuneRenamer.Interfaces;
using TuneRenamer.Logging;
using TuneRenamer.Models;
using TuneRenamer.Platform;

namespace TuneRenamer {
    class ApplicationModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();

            containerRegistry.Register<IEnvironment, OperatingEnvironment>();
            containerRegistry.Register<IFileWriter, JsonObjectWriter>();
            containerRegistry.Register<IPlatformLog, SeriLogAdapter>();
            containerRegistry.Register<IPreferences, PreferencesManager>();
            containerRegistry.Register<IFileTypes, FileTypes>();
            containerRegistry.Register<ISourceScanner, SourceScanner>();
            containerRegistry.Register<IPlatformDialogService, PlatformDialogService>();
            containerRegistry.Register<ITextHelpers, TextHelpers>();

            containerRegistry.Register<IVersionFormatter, VersionSpinnerViewModel>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
