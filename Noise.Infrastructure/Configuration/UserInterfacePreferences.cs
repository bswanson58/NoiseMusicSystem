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
		public	bool		MinimizeOnSwitchToCompanionApp { get; set; }
		public	UInt16		NewAdditionsHorizonDays { get; set; }
		public	UInt32		NewAdditionsHorizonCount { get; set; }
        public  string      ThemeName { get; set; }
        public  string      ThemeAccent { get; set; }
        public  string      ThemeSignature { get; set; }
		public	bool		DisplayBuildDate { get; set; }

		public UserInterfacePreferences() {
			ArtistListSortOrder = string.Empty;
			AlbumListSortOrder = string.Empty;

			EnablePlaybackLibraryFocus = true;
			NewAdditionsHorizonDays = 90;
			NewAdditionsHorizonCount = 50;

			MinimizeToTray = false;
			MinimizeOnSwitchToCompanionApp = true;

			SortPrefixes = string.Empty;

            ThemeName = "BaseDark";
            ThemeAccent = "Steel";
            ThemeSignature = "pack://application:,,,/Noise.UI.Style;component/Themes/Signature_Orange.xaml";
		}
	}
}
