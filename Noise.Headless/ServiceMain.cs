using System;
using System.Linq;
using System.ServiceProcess;
using Microsoft.Practices.Unity;
using Noise.AppSupport;
using ReusableBits.Service;

namespace Noise.Headless {
	static class ServiceMain {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main( string[] args ) {
			var iocConfig = new IocConfiguration();

			iocConfig.InitializeIoc( ApplicationUsage.Server );

			using( var serviceImpl = iocConfig.Container.Resolve<IWindowsService>()) {
				// if install was a command line flag, then run the installer at runtime.
				if( args.Contains( "-install", StringComparer.InvariantCultureIgnoreCase )) {
					WindowsServiceInstaller.InstallService( serviceImpl );
				}
				else if( args.Contains( "-uninstall", StringComparer.InvariantCultureIgnoreCase )) {
					WindowsServiceInstaller.UnInstallService( serviceImpl );
				}
				else {
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
}
