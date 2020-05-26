using System;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using Noise.AppSupport;
using Noise.Desktop.Properties;
using Noise.Desktop.ViewModels;
using Noise.Desktop.Views;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Models;
using Noise.UI.Support;
using Noise.UI.Views;

namespace Noise.Desktop {
	public class Bootstrapper : UnityBootstrapper {
		private INoiseManager		mNoiseManager;
		private INoiseWindowManager	mWindowManager;
		private Window				mShell;
		private	ApplicationSupport	mAppSupport;
		private IApplicationLog		mLog;

		protected override DependencyObject CreateShell() {
            mShell = Container.Resolve<ShellView>();
			mShell.Closing += OnShellClosing;

#if( DEBUG )
			BindingErrorListener.Listen( message => MessageBox.Show( message ));
#endif
			var preferences = Container.Resolve<IPreferences>();
            var interfacePreferences = preferences.Load<UserInterfacePreferences>();

            ThemeManager.SetApplicationTheme( interfacePreferences.ThemeName, interfacePreferences.ThemeAccent, interfacePreferences.ThemeSignature );

            mShell.Show();

			return ( mShell );
		}

		private void OnShellClosing( object sender, System.ComponentModel.CancelEventArgs e ) {
			StopNoise();
		}

		public void ActivateInstance() {
			mWindowManager.ActivateShell();
        }

		protected override IModuleCatalog CreateModuleCatalog() {
			var catalog = new ModuleCatalog();

			catalog
                .AddModule( typeof( DesktopModule ))
                .AddModule( typeof( Core.NoiseCoreModule ))
				.AddModule( typeof( UI.NoiseUiModule ), "NoiseCoreModule" )
				.AddModule( typeof( AudioSupport.AudioSupportModule ))
				.AddModule( typeof( BlobStorage.BlobStorageModule ))
				.AddModule( typeof( Metadata.NoiseMetadataModule ))
				.AddModule( typeof( RemoteHost.RemoteHostModule ))
				.AddModule( typeof( EntityFrameworkDatabase.EntityFrameworkDatabaseModule ));

			return ( catalog );
		}

		protected override void ConfigureContainer() {
		    // Caliburn Micro dispatcher initialize.
		    PlatformProvider.Current = new XamlPlatformProvider();

			base.ConfigureContainer();

			var iocConfig = new IocConfiguration( Container );
			iocConfig.InitializeIoc( ApplicationUsage.Desktop );

			mLog = Container.Resolve<IApplicationLog>();
		}

		protected override void InitializeModules() {
            base.InitializeModules();

            ViewModelResolver.TypeResolver = ( type => Container.Resolve( type ));
            DialogServiceResolver.Current = Container.Resolve<IDialogService>();

            mWindowManager = Container.Resolve<INoiseWindowManager>();
			mWindowManager.Initialize( mShell );

			mLog.ApplicationStarting();

            mShell.DataContext = Container.Resolve<ShellViewModel>();
			StartNoise();
            RegisterRegions();
		}

		private void RegisterRegions() {
			var regionManager = Container.Resolve<IRegionManager>();

            regionManager.RegisterViewWithRegion( RegionNames.ShellView, typeof( StartupView ));
            regionManager.RegisterViewWithRegion( RegionNames.ShellView, typeof( StartupLibraryCreationView ));
            regionManager.RegisterViewWithRegion( RegionNames.ShellView, typeof( StartupLibrarySelectionView ));
            regionManager.RegisterViewWithRegion( RegionNames.ShellView, typeof( LibraryView ));
            regionManager.RegisterViewWithRegion( RegionNames.ShellView, typeof( ListeningView ));
            regionManager.RegisterViewWithRegion( RegionNames.ShellView, typeof( TimelineView ));

			regionManager.RegisterViewWithRegion( RegionNames.LibraryLeftPanel, typeof( LibraryArtistView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryLeftPanel, typeof( LibraryRecentView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryLeftPanel, typeof( FavoritesView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryLeftPanel, typeof( TagsView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryLeftPanel, typeof( SearchView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryLeftPanel, typeof( PlaybackRelatedView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryLeftPanel, typeof( LibraryAdditionsView ));

//            regionManager.RegisterViewWithRegion( RegionNames.LibraryAlbumPanel, typeof( ArtistInfoView ));
//            regionManager.RegisterViewWithRegion( RegionNames.LibraryAlbumPanel, typeof( AlbumInfoView ));
//            regionManager.RegisterViewWithRegion( RegionNames.LibraryAlbumPanel, typeof( ArtistTracksView ));
//            regionManager.RegisterViewWithRegion( RegionNames.LibraryAlbumPanel, typeof( RatedTracksView ));

            regionManager.RegisterViewWithRegion( RegionNames.LibraryRightPanel, typeof( PlayQueueView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryRightPanel, typeof( PlayHistoryView ));
        }

		private async void StartNoise() {
			mNoiseManager = Container.Resolve<INoiseManager>();
			mAppSupport = Container.Resolve<ApplicationSupport>();

			await mNoiseManager.AsyncInitialize();
			mAppSupport.Initialize();
		}

		public void StopNoise() {
			mAppSupport.Shutdown();

			if( mNoiseManager != null ) {
				mNoiseManager.Shutdown();

				mNoiseManager = null;
			}
			if( mWindowManager != null ) {
				mWindowManager.Shutdown();

				mWindowManager = null;
			}

			Settings.Default.Save();

			mLog.ApplicationExiting();
		}

		public void LogException( string reason, Exception exception ) {
            mLog?.LogException( reason, exception );
        }
	}
}
