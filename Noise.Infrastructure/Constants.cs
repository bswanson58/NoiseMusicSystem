using System;

namespace Noise.Infrastructure {
	public class Constants {
		public	const long				cDatabaseNullOid = 0;

		public	const UInt32			cUnknownYear = 0;
		public	const UInt32			cVariousYears = 9999;

		public	static DateTime			cNoExpirationDate { get; private set; }

		public	static string			NewInstance = "NewInstance";

		public	const string			SmallPlayerView = "SmallPlayerView";

		public	const string			LibraryLayout = "DefaultLayout";
		public	const string			StreamLayout = "StreamLayout";

		static Constants() {
			cNoExpirationDate = DateTime.MaxValue;
		}
	}
}
