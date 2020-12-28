using System.Diagnostics.CodeAnalysis;

namespace Noise.RemoteClient.Support {
    [SuppressMessage( "ReSharper", "InconsistentNaming" )]
    static class PreferenceNames {
        public const string UseSortPrefixes     = "useSortPrefixes";
        public const string SortPrefixes        = "sortPrefixes";

        public const string ArtistListSorting   = "artistListSorting";
        public const string AlbumListSorting    = "albumListSorting";
    }
}
