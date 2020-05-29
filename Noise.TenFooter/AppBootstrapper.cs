using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using Noise.AppSupport;
using Noise.AudioSupport;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Interfaces;
using ReusableBits.Mvvm.CaliburnSupport;
using Unity;
using UnityConfiguration;

namespace Noise.TenFooter {
	public class AppBootstrapper : UnityBootstrapper<ShellViewModel> {
		private INoiseManager	mNoiseManager;
		private INoiseLog		mLog;

		protected override void ConfigureBootstrapper() {
			CreateWindowManager = () => new WindowManager();
			// EventAggregator is registered by Noise.Core.
		}

		protected override void ConfigureContainer( IUnityContainer container ) {
			AddModule( typeof( Core.NoiseCoreModule ));
			AddModule( typeof( AudioSupportModule ));
			AddModule( typeof( BlobStorage.BlobStorageModule ));
			AddModule( typeof( Metadata.NoiseMetadataModule ));
			AddModule( typeof( RemoteHost.RemoteHostModule ));
			AddModule( typeof( EntityFrameworkDatabase.EntityFrameworkDatabaseModule ));
			AddModule( typeof( UI.NoiseUiModule ));
			AddModule( typeof( TenFoot.Ui.TenFootUiModule ));

			var iocConfig = new IocConfiguration( Container );
			iocConfig.InitializeIoc( ApplicationUsage.TenFootUi );

			mLog = Container.Resolve<INoiseLog>();

			container.Configure( c => c.AddRegistry<UnityClassRegistration>());

			// Use the Noise.UI PlayerViewModel, but use the TenFoot.Ui PlayerView to display it.
			ViewLocator.AddNamespaceMapping( "Noise.UI.ViewModels", "Noise.TenFoot.Ui.Views" );
			
			// NoiseLogger.Current.LogInfo( "Unity Configuration: {0}", container.WhatDoIHave());
		}

		protected override System.Collections.Generic.IEnumerable<Assembly> SelectAssemblies() {
			return( new []{ Assembly.GetExecutingAssembly(),
			              	Assembly.GetAssembly( typeof( IHome ))
			              } );
		}

		protected override void OnStartup( object sender, StartupEventArgs e ) {
			mLog.LogMessage( "+++++ Noise.TenFooter starting. +++++" );

			mNoiseManager = Container.Resolve<INoiseManager>();
			mNoiseManager.Initialize();

			LogManager.GetLog = type => new CaliburnDebugLogger( type) ;

			// Create the ShellViewModel
			base.OnStartup( sender, e );
		}

		protected override void OnExit( object sender, System.EventArgs e ) {
			if( mNoiseManager != null ) {
				mNoiseManager.Shutdown();
			}

			mLog.LogMessage( "##### NoiseTenFooter stopped. #####" );

			base.OnExit( sender, e );
		}

		protected override void OnUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e ) {
			if( Debugger.IsAttached ) {
				Clipboard.SetText( e.Exception.ToString());
			}
	
			mLog.LogException( "Unhandled exception:", e.Exception );

			if( e.Exception is ReflectionTypeLoadException ) {
				var tle = e.Exception as ReflectionTypeLoadException;

				foreach( var ex in tle.LoaderExceptions ) {
					mLog.LogException( "LoaderException:", ex );
				}
			}

			e.Handled = true;
		}
	}
}
