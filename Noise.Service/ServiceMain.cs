using System;
using System.ServiceProcess;
using Microsoft.Practices.Composite;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.UnityExtensions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Noise.Service.LibraryService;
using Noise.Service.Support;

namespace Noise.Service {
	static class ServiceMain {
		private static IUnityContainer	mContainer;
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main( string[] args ) {
			InitializeUnity();

			using( var implementation = new LibraryServiceImpl( mContainer ) ) {
				// if started from console, file explorer, etc, run as console app.
				if( Environment.UserInteractive ) {
					ConsoleServiceHarness.Run( args, implementation );
				}

				// otherwise run as a windows service
				else {
					ServiceBase.Run( new WindowsServiceHarness( implementation ));
				}
			}
		}

		private static void InitializeUnity() {
			try {
				mContainer = new UnityContainer();

				mContainer.RegisterInstance<ILoggerFacade>( new TextLogger());

				var catalog = new ModuleCatalog();

				catalog.AddModule( typeof( Core.NoiseCoreModule ));
				mContainer.RegisterInstance<IModuleCatalog>( catalog );

				mContainer.RegisterType<IServiceLocator, UnityServiceLocatorAdapter>( new ContainerControlledLifetimeManager());
				mContainer.RegisterType<IModuleInitializer, ModuleInitializer>( new ContainerControlledLifetimeManager());
				mContainer.RegisterType<IModuleManager, ModuleManager>( new ContainerControlledLifetimeManager());
				mContainer.RegisterType<IEventAggregator, EventAggregator>( new ContainerControlledLifetimeManager());

				ExceptionExtensions.RegisterFrameworkExceptionType( typeof( ActivationException));
				ExceptionExtensions.RegisterFrameworkExceptionType( typeof( ResolutionFailedException));
				ExceptionExtensions.RegisterFrameworkExceptionType( typeof(Microsoft.Practices.ObjectBuilder2.BuildFailedException));

				ServiceLocator.SetLocatorProvider( mContainer.Resolve<IServiceLocator>);

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
