namespace Noise.Core.Logging {
	public class LoggingPreferences {
		public bool LogExceptions { get; set; }

		public bool BuildingDiscovery { get; set; }
		public bool BuildingDiscoverFolders { get; set; }
		public bool BuildingDiscoverFiles { get; set; }

		public bool MetadataCleaning {  get; set; }
		public bool MetadataCleaningDomainObjects { get; set; }
		public bool MetadataCleaningFileObjects { get; set; }

		public bool LogAnyBuildingDiscovery {
			get { return( BuildingDiscovery || BuildingDiscoverFiles || BuildingDiscoverFolders ); }
		}

		public bool LogAnyCleaning {
			get {  return( MetadataCleaning || MetadataCleaningDomainObjects || MetadataCleaningFileObjects ); }
		}

		public bool LogAnyBuilding {
			get {  return( LogAnyBuildingDiscovery || LogAnyCleaning ); }
		}

		public LoggingPreferences() {
			LogExceptions = true;
		}
	}
}
