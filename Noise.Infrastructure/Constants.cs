using System;

namespace Noise.Infrastructure {
	public class Constants {
		public	const long				cDatabaseNullOid = 0;

		public	const Int32				cUnknownYear = 0;
		public	const Int32				cVariousYears = 1;

		public	static DateTime			cNoExpirationDate { get; private set; }

		public	static string			NewInstance = "NewInstance";

		public	const string			SmallPlayerViewToggle = "SmallPlayerView";

		public	const string			ExploreLayout = "ExploreLayout";
		public	const string			ListenLayout = "ListenLayout";
		public	const string			TimeExplorerLayout = "TimeExplorerLayout";

		public const string				ApplicationLogName = "noise.log";

		public const string				ApplicationName = "Noise";
		public const string				CompanyName = "Secret_Squirrel_Products";
		public const string				LibraryConfigurationDirectory = "Noise Libraries";
		public const string				LibraryConfigurationFile = "Library.config";
		public const string				BlobDatabaseDirectory = "Blob Database";
		public const string				LibraryDatabaseDirectory = "Library Database";
		public const string				SearchDatabaseDirectory = "Search Database";

		public const string				Id3FrameUserName = "Noise Music System";
		public const string				FavoriteFrameDescription = "Noise Music System - Favorite Track Flag";

		public const string				EqPresetsFile = "EqPresets.xml";
		public const string				LicenseKeyFile = "LicenseKeys.xml";

		public const string				CloudDatabaseName = @"Noise Music System Database";
		public const string				CloudSyncTable = "Noise Music Sync Entries";
		public const string				CloudSyncFavoritesTable = "Noise Music Favorites";
		public const string				CloudSyncStreamsTable = "Noise Music Streams";

		public const string				ExportFileExtension = ".noise";

		static Constants() {
			cNoExpirationDate = DateTime.MaxValue;
		}
	}
}
