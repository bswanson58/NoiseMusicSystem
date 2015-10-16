// Based on: http://geekswithblogs.net/BlackRabbitCoder/archive/2010/10/07/c-windows-services-2-of-2-self-installing-windows-service-template.aspx
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace ReusableBits.Service {
	[RunInstaller(true)]
	public class WindowsServiceInstaller : Installer {
		// Gets or sets the type of the windows service to install.
		public WindowsServiceAttribute Configuration { get; set; }


		// Creates a blank windows service installer with configuration in ServiceImplementation
		//public WindowsServiceInstaller() : this(typeof(ServiceImplementation)) {
		//   }

		// Creates a windows service installer using the type specified.
		public WindowsServiceInstaller( IWindowsService service ) {
			var attribute = service.GetType().GetAttribute<WindowsServiceAttribute>();
			if( attribute == null ) {
				throw new ArgumentException( "Type to install must be marked with a WindowsServiceAttribute." );
			}

			Configuration = attribute;
		}

		// Performs a transacted installation at run-time of the AutoCounterInstaller and any other listed installers.
		public static void InstallService( IWindowsService service ) {
			string path = "/assemblypath=" + Assembly.GetEntryAssembly().Location;

			using( var ti = new TransactedInstaller()) {
				ti.Installers.Add( new WindowsServiceInstaller( service ));
				ti.Context = new InstallContext( null, new[] { path } );
				ti.Install( new Hashtable());
			}
		}

		// Performs a transacted un-installation at run-time of the AutoCounterInstaller and any other listed installers.
		public static void UnInstallService( IWindowsService service, params Installer[] otherInstallers ) {
			string path = "/assemblypath=" + Assembly.GetEntryAssembly().Location;

			using( var ti = new TransactedInstaller()) {
				ti.Installers.Add( new WindowsServiceInstaller( service ));
				ti.Context = new InstallContext( null, new[] { path } );
				ti.Uninstall( null );
			}
		}

		// Installer class, to use run InstallUtil against this .exe
		public override void Install( IDictionary savedState ) {
			// install the service 
			ConfigureInstallers();

			base.Install( savedState );
		}

		// Removes the counters, then calls the base uninstall.
		public override void Uninstall( IDictionary savedState ) {
			// load the assembly file name and the config
			ConfigureInstallers();

			base.Uninstall( savedState );
		}

		// Method to configure the installers
		private void ConfigureInstallers() {
			// load the assembly file name and the config
			Installers.Add( ConfigureProcessInstaller());
			Installers.Add( ConfigureServiceInstaller());
		}

		// Helper method to configure a process installer for this windows service
		private ServiceProcessInstaller ConfigureProcessInstaller() {
			var result = new ServiceProcessInstaller();

			// if a user name is not provided, will run under local service acct
			if( string.IsNullOrEmpty( Configuration.UserName )) {
				result.Account = ServiceAccount.LocalSystem;
				result.Username = null;
				result.Password = null;
			}
			else {
				// otherwise, runs under the specified user authority
				result.Account = ServiceAccount.User;
				result.Username = Configuration.UserName;
				result.Password = Configuration.Password;
			}

			return result;
		}

		// Helper method to configure a service installer for this windows service
		private ServiceInstaller ConfigureServiceInstaller() {
			// create and config a service installer
			var result = new ServiceInstaller {
				ServiceName = Configuration.ServiceName,
				DisplayName = Configuration.DisplayName,
				Description = Configuration.Description,
				StartType = Configuration.StartMode,
			};

			return result;
		}
	}
}