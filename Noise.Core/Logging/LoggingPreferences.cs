namespace Noise.Core.Logging {
	public class LoggingPreferences {
		public bool LogExceptions { get; set; }

		public bool BuildingDiscovery { get; set; }
		public bool BuildingDiscoverFolders { get; set; }
		public bool BuildingDiscoverFiles { get; set; }

		public bool LogAnyBuildingDiscovery {
			get { return( BuildingDiscovery || BuildingDiscoverFiles || BuildingDiscoverFolders ); }
		}

		public bool LogAnyBuilding {
			get {  return( LogAnyBuildingDiscovery ); }
		}

		public LoggingPreferences() {
			LogExceptions = true;
		}
	}
}
