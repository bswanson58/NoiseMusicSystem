using System;

namespace Noise.Infrastructure.Configuration {
	public class UserInterfacePreferences {
		public	string		ArtistListSortOrder { get; set; }
		public	string		AlbumListSortOrder {  get; set; }
		public	bool		EnableGlobalHotkeys {  get; set; }
		public	bool		EnablePlaybackLibraryFocus { get; set; }
		public	bool		EnableSortPrefixes { get; set; }
		public	string		SortPrefixes { get; set; }
		public	bool		MinimizeToTray { get; set; }
		public	UInt16		NewAdditionsHorizonDays { get; set; }
		public	UInt32		NewAdditionsHorizonCount { get; set; }

		public UserInterfacePreferences() {
			ArtistListSortOrder = string.Empty;
			AlbumListSortOrder = string.Empty;

			EnablePlaybackLibraryFocus = true;
			NewAdditionsHorizonDays = 90;
			NewAdditionsHorizonCount = 500;

			SortPrefixes = string.Empty;
		}
	}
}
