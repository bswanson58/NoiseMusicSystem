﻿using System;

namespace Noise.Infrastructure {
	public class Constants {
		public	const int				cRemoteApiVersion = 2;

		public	const long				cDatabaseNullOid = 0;

		public	const Int32				cUnknownYear = 0;
		public	const Int32				cVariousYears = 1;

		public	static DateTime			cNoExpirationDate { get; }

		public	static string			NewInstance = "NewInstance";

		public	const string			SmallPlayerViewToggle = "SmallPlayerView";

		public	const string			StartupLayout = "StartupLayout";
		public	const string			LibrarySelectionLayout = "LibrarySelectionLayout";
		public	const string			LibraryCreationLayout = "LibraryCreationLayout";
		public	const string			ExploreLayout = "ExploreLayout";
		public	const string			ListenLayout = "ListenLayout";
		public	const string			TimeExplorerLayout = "TimeExplorerLayout";

		public	const string			LogFileDirectory = "Logs";

        public const string				EcosystemName = "Noise Music System";
        public const string				ApplicationName = "Noise Music System";
		public const string				CompanyName = "Secret Squirrel Software";
		public const string				LibraryConfigurationDirectory = "Libraries";
		public const string				LibraryBackupDirectory = "Backups";
		public const string				ConfigurationDirectory = "Configuration";
		public const string				LibraryConfigurationFile = "Library.config";
		public const string				BlobDatabaseDirectory = "Blob Database";
		public const string				LibraryDatabaseDirectory = "Library Database";
		public const string				SearchDatabaseDirectory = "Search Database";
		public const string				SearchDatabaseBackupName = "Search Database.bak";
		public const string				MetadataDirectory = "Metadata";
		public const string				MetadataBackupName = "Metadata.bak";
		public const string				DesktopPreferencesDirectory = "Desktop";
		public const string				HeadlessPreferencesDirectory = "Headless";
		public const string				TenFootPreferencesDirectory = "TenFoot";

		public const string				Id3FrameUserName = "Noise Music System";
		public const string				FavoriteFrameDescription = "Noise Music System - Favorite Track Flag";

		public const string				SidecarExtension = ".nsc";
		public const string				ArtistSidecarName = "Artist Info" + SidecarExtension;
		public const string				AlbumSidecarName = "Album Info" + SidecarExtension;

		public const string				LibraryMetadataFolder = "_metadata";

		public const string				EqPresetsFile = "EqPresets.xml";
		public const string				LicenseKeyFile = "LicenseKeys.xml";

		public const string				CloudDatabaseName = @"Noise Music System Database";
		public const string				CloudSyncTable = "Noise Music Sync Entries";
		public const string				CloudSyncFavoritesTable = "Noise Music Favorites";
		public const string				CloudSyncStreamsTable = "Noise Music Streams";

		public const string				ExportFileExtension = ".noise";

		public const string				Ef_DatabaseFileExtension = "_noise.mdf";
		public const string				Ef_DatabaseBackupExtension = ".bak";

		public const string				MetadataDatabaseName = "Noise Metadata.db";
		public const string				ArtistArtworkStorageName = "ArtistArtwork";

		static Constants() {
			cNoExpirationDate = DateTime.MaxValue;
		}
	}
}
