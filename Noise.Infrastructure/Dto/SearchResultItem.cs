using System;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Dto {
	public class SearchResultItem {
		public DbArtist			Artist { get; }
		public DbAlbum			Album { get; }
		public DbTrack			Track { get; }
		public eSearchItemType	ItemType { get; }

		public SearchResultItem( DbArtist artist, DbAlbum album, DbTrack track, eSearchItemType itemType ) {
			Artist = artist;
			Album = album;
			Track = track;
			ItemType = itemType;
		}

		public string ItemDescription {
			get {
				var retValue = String.Empty;

				switch( ItemType ) {
					case eSearchItemType.Artist:
						retValue = $"Artist {Artist?.Name}";
						break;

					case eSearchItemType.Album:
						retValue = $"Album {Album?.Name} from artist {Artist?.Name}";
						break;

					case eSearchItemType.BandMember:
						retValue = $"Band member of {Artist?.Name}";
						break;

					case eSearchItemType.Biography:
						retValue = $"Biography of {Artist?.Name}";
						break;

					case eSearchItemType.Discography:
						retValue = $"Discography of {Artist?.Name}";
						break;

					case eSearchItemType.Lyrics:
						retValue = $"Lyrics for {Artist?.Name} - {Track?.Name}";
						break;

					case eSearchItemType.SimilarArtist:
						retValue = $"Artist similar to {Artist?.Name}";
						break;

					case eSearchItemType.TextInfo:
						retValue = $"Album information for {Artist?.Name} - {Album?.Name}";
						break;

					case eSearchItemType.TopAlbum:
						retValue = $"Top album of {Artist?.Name}";
						break;

					case eSearchItemType.Track:
						retValue = $"Track {Track?.Name} of album {Artist?.Name} - {Album?.Name}";
						break;
				}

				return retValue;

			}
		}
	}
}
