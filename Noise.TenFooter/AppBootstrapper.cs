using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Practices.Unity;
using Noise.AppSupport;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFooter {
	public class AppBootstrapper : UnityBootstrapper<ShellViewModel> {
		private INoiseManager	mNoiseManager;

		protected override void ConfigureBootstrapper() {
			CreateWindowManager = () => new WindowManager();
			// EventAggregator is registered by Noise.Core.
		}

		protected override void ConfigureContainer( IUnityContainer container ) {
			AddModule( typeof( Core.NoiseCoreModule ));
			AddModule( typeof( BlobStorage.BlobStorageModule ));
			AddModule( typeof( RemoteHost.RemoteHostModule ));
			AddModule( typeof( EloqueraDatabase.EloqueraDatabaseModule ));

			var iocConfig = new IocConfiguration( Container );
			iocConfig.InitializeIoc( ApplicationUsage.TenFootUi );
		}

		protected override System.Collections.Generic.IEnumerable<Assembly> SelectAssemblies() {
			return( new []{ Assembly.GetExecutingAssembly(),
			              	Assembly.GetAssembly( typeof( TenFoot.Ui.ViewModels.HomeViewModel ))
			              } );
		}

		protected override void OnStartup( object sender, StartupEventArgs e ) {
			NoiseLogger.Current.LogMessage( "============================" );
			NoiseLogger.Current.LogMessage( "Noise.TenFooter is starting." );

			mNoiseManager = Container.Resolve<INoiseManager>();
			mNoiseManager.Initialize();

			// Create the ShellViewModel
			base.OnStartup( sender, e );
		}

		protected override void OnExit( object sender, System.EventArgs e ) {
			if( mNoiseManager != null ) {
				mNoiseManager.Shutdown();
			}

			base.OnExit( sender, e );
		}

		protected override void OnUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e ) {
			if( Debugger.IsAttached ) {
				Clipboard.SetText( e.Exception.ToString());
			}
	
			NoiseLogger.Current.LogException( "Noise.TenFooter:Application unhandled exception:", e.Exception );

			if( e.Exception is ReflectionTypeLoadException ) {
				var tle = e.Exception as ReflectionTypeLoadException;

				foreach( var ex in tle.LoaderExceptions ) {
					NoiseLogger.Current.LogException( "LoaderException:", ex );
				}
			}

			e.Handled = true;
		}
	}
}
