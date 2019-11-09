using ArchiveLoader.Interfaces;
using ArchiveLoader.Logging;
using ArchiveLoader.Models;
using ArchiveLoader.Platform;
using ArchiveLoader.ViewModels;
using ArchiveLoader.Views;
using Caliburn.Micro;
using Prism.Ioc;
using Prism.Modularity;
using ReusableBits.Mvvm.VersionSpinner;

namespace ArchiveLoader {
    class ApplicationModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();

            containerRegistry.RegisterSingleton<IEnvironment, OperatingEnvironment>();
            containerRegistry.RegisterSingleton<IFileWriter, JsonObjectWriter>();
            containerRegistry.RegisterSingleton<IPlatformLog, SeriLogAdapter>();
            containerRegistry.RegisterSingleton<IPreferences, PreferencesManager>();
            containerRegistry.RegisterSingleton<IPlatformDialogService, PlatformDialogService>();

            containerRegistry.RegisterSingleton<IDriveManager, DriveManager>();
            containerRegistry.RegisterSingleton<IDriveNotifier, DriveNotifier>();
            containerRegistry.RegisterSingleton<ICopyProcessor, CopyProcessor>();
            containerRegistry.RegisterSingleton<IProcessReadyNotifier, ProcessReadyNotifier>();
            containerRegistry.RegisterSingleton<IProcessManager, ProcessManager>();
            containerRegistry.Register<IDriveEjector, DriveEjector>();
            containerRegistry.Register<IFileCopier, FileCopier>();
            containerRegistry.Register<IProcessBuilder, ProcessBuilder>();
            containerRegistry.Register<IProcessQueue, ProcessQueue>();
            containerRegistry.Register<IExitHandlerFactory, ExitHandlerFactory>();
            containerRegistry.Register<IFileMetadata, FileMetadata>();

            containerRegistry.Register<IVersionFormatter, VersionSpinnerViewModel>();


            containerRegistry.RegisterDialog<FileHandlerDialogView, FileHandlerDialogModel>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
