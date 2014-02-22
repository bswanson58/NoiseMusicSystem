using System;
using System.ServiceProcess;

namespace ReusableBits.Service {
	[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = true )]
	public class WindowsServiceAttribute : Attribute {
		// The name of the service.
		public string ServiceName { get; set; }

		// The displayable name that shows in service manager (defaults to Name).
		public string DisplayName { get; set; }

		// A textural description of the service name (defaults to Name).
		public string Description { get; set; }

		// The user to run the service under (defaults to null).  A null or empty
		// UserName field causes the service to run as ServiceAccount.LocalService.
		public string UserName { get; set; }

		// The password to run the service under (defaults to null).  Ignored
		// if the UserName is empty or null, this property is ignored.
		public string Password { get; set; }

		// Specifies the event log source to set the service's EventLog to.  If this is
		// empty or null (the default) no event log source is set.  If set, will auto-log
		// start and stop events.
		public string EventLogSource { get; set; }

		// The method to start the service when the machine reboots (defaults to Manual).
		public ServiceStartMode StartMode { get; set; }

		// True if service supports pause and continue (defaults to true).
		public bool CanPauseAndContinue { get; set; }

		// True if service supports shutdown event (defaults to true).
		public bool CanShutdown { get; set; }

		// True if service supports stop event (defaults to true).
		public bool CanStop { get; set; }

		// Marks an IWindowsService with configuration and installation attributes.
		public WindowsServiceAttribute( string name ) {
			// set name and default description and display name to name.
			ServiceName = name;
			Description = name;
			DisplayName = name;

			// default all other attributes.
			CanStop = true;
			CanShutdown = true;
			CanPauseAndContinue = true;
			StartMode = ServiceStartMode.Manual;
			EventLogSource = null;
			Password = null;
			UserName = null;
		}
	}
}