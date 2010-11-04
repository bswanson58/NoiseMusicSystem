using System;
using System.ServiceProcess;
using Noise.Service.LibraryService;

namespace Noise.Service {
	static class ServiceMain {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main( string[] args ) {
			using( var implementation = new LibraryServiceImpl() ) {
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
