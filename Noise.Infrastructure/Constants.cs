using System;

namespace Noise.Infrastructure {
	public class Constants {
		public	const long				cDatabaseNullOid = 0;

		public	const UInt32			cUnknownYear = 0;
		public	const UInt32			cVariousYears = 9999;

		public	static DateTime			cNoExpirationDate { get; private set; }

		public	static string			NewInstance = "NewInstance";

		public	const string			SmallPlayerViewToggle = "SmallPlayerView";

		public	const string			LibraryLayout = "DefaultLayout";
		public	const string			StreamLayout = "StreamLayout";

		public const string				ApplicationLogName = "noise.log";

		public const string				ApplicationName = "Noise";
		public const string				CompanyName = "Secret_Squirrel_Products";

		static Constants() {
			cNoExpirationDate = DateTime.MaxValue;
		}
	}
}
