using System;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using Noise.AppSupport;
using Noise.Librarian.Views;
using Noise.UI.Support;

namespace Noise.Librarian {
	public class Bootstrapper : UnityBootstrapper {
		private Window		mShell;

		protected override DependencyObject CreateShell() {
			mShell = Container.Resolve<Shell>();
			mShell.Show();
			mShell.Closing += OnShellClosing;

#if( DEBUG )
			BindingErrorListener.Listen( message => MessageBox.Show( message ));
#endif

			return ( mShell );
		}

		private void OnShellClosing( object sender, System.ComponentModel.CancelEventArgs e ) {
			Shutdown();
		}

		private void Shutdown() {
			
		}

		protected override void ConfigureContainer() {
			base.ConfigureContainer();

			Container.RegisterType<Shell, Shell>();

			var iocConfig = new IocConfiguration( Container );
			iocConfig.InitializeIoc( ApplicationUsage.Librarian );

			ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver( ViewModelTypeResolver );
			ViewModelLocationProvider.SetDefaultViewModelFactory( CreateViewModel );

			Container.Resolve<IModuleManager>().Run();
		}

		private Type ViewModelTypeResolver( Type viewType ) {
			var viewModelName = viewType.Name.Replace( "View", "ViewModel" );
			var viewModelType = GetType().Assembly.GetTypes().FirstOrDefault( type => type.Name == viewModelName );

			return( viewModelType );
		}

		private object CreateViewModel( Type modelType ) {
			return( Container.Resolve( modelType ));
		}

		protected override IModuleCatalog CreateModuleCatalog() {
			var catalog = new ModuleCatalog();

			catalog.AddModule( typeof( Core.NoiseCoreModule ))
				.AddModule( typeof( UI.NoiseUiModule ), "NoiseCoreModule" )
				.AddModule( typeof( BlobStorage.BlobStorageModule ))
				.AddModule( typeof( Metadata.NoiseMetadataModule ))
				.AddModule( typeof( EntityFrameworkDatabase.EntityFrameworkDatabaseModule ));

			return ( catalog );
		}
	}
}
