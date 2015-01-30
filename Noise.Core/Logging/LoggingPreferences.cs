﻿namespace Noise.Core.Logging {
	public class LoggingPreferences {
		public bool LogExceptions { get; set; }

		public bool BuildingDiscovery { get; set; }
		public bool BuildingDiscoverFolders { get; set; }
		public bool BuildingDiscoverFiles { get; set; }

		public bool MetadataCleaning { get; set; }
		public bool MetadataCleaningDomainObjects { get; set; }
		public bool MetadataCleaningFileObjects { get; set; }

		public bool FileClassification { get; set; }
		public bool FileClassificationFiles { get; set; }
		public bool FileClassificationSteps { get; set; }
		public bool FileClassificationArtists { get; set; }
		public bool FileClassificationAlbums { get; set; }
		public bool FileClassificationTracks { get; set; }
		public bool FileClassificationArtwork { get; set; }
		public bool FileClassificationTextInfo { get; set; }
		public bool FileClassificationUnknown { get; set; }

		public bool LogAnyBuildingDiscovery {
			get { return( BuildingDiscovery || BuildingDiscoverFiles || BuildingDiscoverFolders ); }
		}

		public bool LogAnyCleaning {
			get {  return( MetadataCleaning || MetadataCleaningDomainObjects || MetadataCleaningFileObjects ); }
		}

		public bool LogAnyClassification {
			get {  return( FileClassification || FileClassificationFiles || FileClassificationSteps || FileClassificationUnknown ||
						   FileClassificationArtists || FileClassificationAlbums || FileClassificationTracks || FileClassificationArtwork || FileClassificationTextInfo );
			}
		}

		public bool LogAnyBuilding {
			get {  return( LogAnyBuildingDiscovery || LogAnyCleaning || LogAnyClassification ); }
		}

		public LoggingPreferences() {
			LogExceptions = true;
		}
	}
}
