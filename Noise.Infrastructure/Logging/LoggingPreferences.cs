namespace Noise.Infrastructure.Logging {
	public class LoggingPreferences {
		public bool LogExceptions { get; set; }
		public bool LogMessages { get; set; }

		public bool LibraryConfiguration { get; set; }

		public bool AudioPlayback { get; set; }
		public bool AudioSync { get; set; }

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

		public bool BuildingSummary { get; set; }
		public bool BuildingSummaryArtists { get; set; }
		public bool BuildingSummaryAlbums { get; set; }

		public bool PlayQueueAddRemove { get; set; }
		public bool PlayQueueStatusChange { get; set; }

		public bool PlayState { get; set; }

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

		public bool LogAnyBuildingSummary {
			get {  return( BuildingSummary || BuildingSummaryArtists || BuildingSummaryAlbums ); }
		}

		public bool LogAnyBuilding {
			get {  return( LogAnyBuildingDiscovery || LogAnyCleaning || LogAnyClassification || BuildingSummary ); }
		}

		public LoggingPreferences() {
			LogExceptions = true;
		}
	}
}
