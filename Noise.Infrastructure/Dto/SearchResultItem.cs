using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Dto {
	public class SearchResultItem {
		public DbArtist			Artist { get; private set; }
		public DbAlbum			Album { get; private set; }
		public DbTrack			Track { get; private set; }
		public eSearchItemType	ItemType { get; private set; }

		public SearchResultItem( DbArtist artist, DbAlbum album, DbTrack track, eSearchItemType itemType ) {
			Artist = artist;
			Album = album;
			Track = track;
			ItemType = itemType;
		}

		public string ItemDescription {
			get {
				var retValue = "";

				switch( ItemType ) {
					case eSearchItemType.Artist:
						retValue = string.Format( "Artist {0}", Artist.Name );
						break;

					case eSearchItemType.Album:
						retValue = string.Format( "Album {0} from artist {1}", Album.Name, Artist.Name );
						break;

					case eSearchItemType.BandMember:
						retValue = string.Format( "Band member of {0}", Artist.Name );
						break;

					case eSearchItemType.Biography:
						retValue = string.Format( "Biography of {0}", Artist.Name );
						break;

					case eSearchItemType.Discography:
						retValue = string.Format( "Discography of {0}", Artist.Name );
						break;

					case eSearchItemType.Lyrics:
						retValue = string.Format( "Lyrics for {0} - {1}", Artist.Name, Track.Name );
						break;

					case eSearchItemType.SimilarArtist:
						retValue = string.Format( "Artist similar to {0}", Artist.Name );
						break;

					case eSearchItemType.TextInfo:
						retValue = string.Format( "Album information for {0} - {1}", Artist.Name, Album.Name );
						break;

					case eSearchItemType.TopAlbum:
						retValue = string.Format( "Top album of {0}", Artist.Name );
						break;

					case eSearchItemType.Track:
						retValue = string.Format( "Track {0} of album {1} - {2}", Track.Name, Artist.Name, Album.Name );
						break;
				}

				return( retValue );

			}
		}
	}
}
