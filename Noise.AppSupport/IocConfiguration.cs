using System;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Noise.Core.DataBuilders;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support.Service;
using Noise.Service.Infrastructure.Clients;
using Noise.ServiceImpl.LibraryUpdate;

namespace Noise.AppSupport {
	public enum ApplicationUsage {
		Desktop,
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
			mContainer.RegisterType<IIoc, IocProvider>();

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ServerConfiguration>( ServerConfiguration.SectionName );

			switch( appUsage ) {
				case ApplicationUsage.Desktop:
					if(( configuration != null ) &&
					   ( configuration.UseServer )) {
						mContainer.RegisterType<ILibraryBuilder, LibraryServiceUpdateClient>();
					}
					else {
						mContainer.RegisterType<ILibraryBuilder, LibraryBuilder>();
					}

					break;

				case ApplicationUsage.Server:
					InitializeUnity();

					mContainer.RegisterType<ILibraryBuilder, LibraryBuilder>();
					mContainer.RegisterType<IWindowsService, LibraryServiceImpl>();

					break;
			}

			return( true );
		}

		private void InitializeUnity() {
			try {

				mContainer.RegisterInstance<ILoggerFacade>( new TextLogger());

				var catalog = new ModuleCatalog();

				catalog.AddModule( typeof( Core.NoiseCoreModule ));
				catalog.AddModule( typeof( Service.Infrastructure.ServiceInfrastructureModule ));
				catalog.AddModule( typeof( ServiceImpl.NoiseServiceImplModule ));
				catalog.AddModule( typeof( EloqueraDatabase.EloqueraDatabaseModule ));
				mContainer.RegisterInstance<IModuleCatalog>( catalog );

				mContainer.RegisterType<IServiceLocator, UnityServiceLocatorAdapter>( new ContainerControlledLifetimeManager());
				mContainer.RegisterType<IModuleInitializer, ModuleInitializer>( new ContainerControlledLifetimeManager());
				mContainer.RegisterType<IModuleManager, ModuleManager>( new ContainerControlledLifetimeManager());
				mContainer.RegisterType<IEventAggregator, EventAggregator>( new ContainerControlledLifetimeManager());

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
