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

namespace Noise.Service {
	static class ServiceMain {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main( string[] args ) {
			var	container = new UnityContainer();
			var catalog = new ModuleCatalog();

	        ILoggerFacade	loggerFacade = new TextLogger();

			catalog.AddModule( typeof( Core.NoiseCoreModule ));
			container.RegisterInstance<IModuleCatalog>( catalog );
            container.AddNewExtension<UnityBootstrapperExtension>();

            container.RegisterInstance( loggerFacade );
			container.RegisterType<IServiceLocator, UnityServiceLocatorAdapter>( new ContainerControlledLifetimeManager());
			container.RegisterType<IModuleInitializer, ModuleInitializer>( new ContainerControlledLifetimeManager());
            container.RegisterType<IModuleManager, ModuleManager>( new ContainerControlledLifetimeManager());
            container.RegisterType<IEventAggregator, EventAggregator>( new ContainerControlledLifetimeManager());

            ExceptionExtensions.RegisterFrameworkExceptionType( typeof( ActivationException));
            ExceptionExtensions.RegisterFrameworkExceptionType( typeof( ResolutionFailedException));
            ExceptionExtensions.RegisterFrameworkExceptionType( typeof(Microsoft.Practices.ObjectBuilder2.BuildFailedException));

            ServiceLocator.SetLocatorProvider( container.Resolve<IServiceLocator>);

            var manager = container.Resolve<IModuleManager>();

            manager.Run();

			using( var implementation = new LibraryServiceImpl( container ) ) {
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
	}
}
