using System;
using System.ServiceProcess;
using Noise.AppSupport;
using Noise.Infrastructure.Support.Service;

namespace Noise.Service {
	static class ServiceMain {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main( string[] args ) {
			var iocConfig = new IocConfiguration();

			iocConfig.InitializeIoc( ApplicationUsage.Server );

			using( var serviceImpl = iocConfig.Container.Resolve<IWindowsService>()) {
				// if started from console, file explorer, etc, run as console app.
				if( Environment.UserInteractive ) {
					ConsoleServiceHarness.Run( args, serviceImpl );
				}

				// otherwise run as a windows service
				else {
					ServiceBase.Run( new WindowsServiceHarness( serviceImpl ));
				}
			}
		}
	}
}
