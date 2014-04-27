using System;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Noise.AudioSupport;
using Noise.Core.DataBuilders;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteHost;
using Noise.Metadata;

namespace Noise.AppSupport {
	public enum ApplicationUsage {
		Desktop,
		TenFootUi,
		Server
	}

	public class IocConfiguration {
		private readonly IUnityContainer	mContainer;

		public IocConfiguration() :
			this( new UnityContainer()) {
		}

		public IocConfiguration( IUnityContainer container ) {
			mContainer = container;
		}

		public IUnityContainer Container {
			get{ return( mContainer ); }
		}

		public bool InitializeIoc( ApplicationUsage appUsage ) {
			mContainer.RegisterType<IIoc, IocProvider>( new ContainerControlledLifetimeManager());
			mContainer.RegisterType<INoiseEnvironment, NoiseEnvironment>();

#if DEBUG
			const int portOffset = 10;
#else
			const int portOffset = 0;
#endif

			switch( appUsage ) {
				case ApplicationUsage.Desktop:
					mContainer.RegisterType<ILibraryBuilder, LibraryBuilder>();
					mContainer.RegisterInstance( new RemoteHostConfiguration( 71 + portOffset, "Noise Desktop System" ));

					break;

				case ApplicationUsage.Server:
					InitializeUnity();

					mContainer.RegisterType<ILibraryBuilder, LibraryBuilder>();
					mContainer.RegisterInstance( new RemoteHostConfiguration( 73 + portOffset, "Noise Headless Service" ));

					break;

				case ApplicationUsage.TenFootUi:
					mContainer.RegisterType<ILibraryBuilder, LibraryBuilder>();
					mContainer.RegisterInstance( new RemoteHostConfiguration( 72 + portOffset, "Noise TenFoot System" ));

					break;
			}

			return( true );
		}

		private void InitializeUnity() {
			try {

				mContainer.RegisterInstance<ILoggerFacade>( new TextLogger());

				var catalog = new ModuleCatalog();

				catalog.AddModule( typeof( Core.NoiseCoreModule ))
						.AddModule( typeof( EntityFrameworkDatabase.EntityFrameworkDatabaseModule ))
						.AddModule( typeof( AudioSupportModule ))
						.AddModule( typeof( BlobStorage.BlobStorageModule ))
						.AddModule( typeof( NoiseMetadataModule ))
						.AddModule( typeof( RemoteHost.RemoteHostModule ));
				mContainer.RegisterInstance<IModuleCatalog>( catalog );

				mContainer.RegisterType<IServiceLocator, UnityServiceLocatorAdapter>( new ContainerControlledLifetimeManager());
				mContainer.RegisterType<IModuleInitializer, ModuleInitializer>( new ContainerControlledLifetimeManager());
				mContainer.RegisterType<IModuleManager, ModuleManager>( new ContainerControlledLifetimeManager());

				ExceptionExtensions.RegisterFrameworkExceptionType( typeof( ActivationException));
				ExceptionExtensions.RegisterFrameworkExceptionType( typeof( ResolutionFailedException));
				ExceptionExtensions.RegisterFrameworkExceptionType( typeof(Microsoft.Practices.ObjectBuilder2.ILifetimeContainer));

				ServiceLocator.SetLocatorProvider( () => mContainer.Resolve<IServiceLocator>());

				mContainer.Resolve<IModuleManager>().Run();
			}
			catch( Exception ex ) {
				if( mContainer != null ) {
					var logger = mContainer.Resolve<ILoggerFacade>();

					logger.Log( String.Format( "Failed to initialize Unity: {0}", ex.Message ), Category.Exception, Priority.High );
				}
			}
		}
	}
}
