using System.Diagnostics.CodeAnalysis;

namespace Noise.RemoteClient.Support {
    [SuppressMessage( "ReSharper", "InconsistentNaming" )]
    static class PreferenceNames {
        public const string UseSortPrefixes         = "useSortPrefixes";
        public const string SortPrefixes            = "sortPrefixes";

        public const string ArtistListSorting       = "artistListSorting";
        public const string AlbumListSorting        = "albumListSorting";
        public const string FavoritesListSorting    = "favoritesListSorting";

        public const string ApplicationFont         = "applicationFont";
        public const string ApplicationTheme        = "applicationTheme";

        public const string PlaybackTimeFormat      = "playbackTimeFormat";

        public const string TransportBackground     = "transportBackground";
    }
}
