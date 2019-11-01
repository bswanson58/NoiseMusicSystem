﻿using ArchiveLoader.Interfaces;
using ArchiveLoader.Logging;
using ArchiveLoader.Models;
using ArchiveLoader.Platform;
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
            containerRegistry.Register<IDriveEjector, DriveEjector>();
            containerRegistry.Register<IFileCopier, FileCopier>();

            containerRegistry.Register<IVersionFormatter, VersionSpinnerViewModel>();

            containerRegistry.RegisterSingleton<IProcessManager, ProcessManager>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
