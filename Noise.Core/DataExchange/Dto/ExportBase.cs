using System;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataExchange.Dto {
	public class ExportBase {
		public	long			CreationDateTicks { get; set; }
		public	long			OriginDb { get; set; }
		public	long			SequenceId { get; set; }
		public	string			Artist { get; set; }
		public	string			Album { get; set; }
		public	string			Track { get; set; }
		public	string			Stream { get; set; }

		private ExportBase( long originDb ) :
			this() {
			OriginDb = originDb;
		}

		protected ExportBase() {
			CreationDateTicks = DateTime.Now.Ticks;

			Artist = "";
			Album = "";
			Track = "";
			Stream = "";
		}

		protected ExportBase( long originDb, string artist ) :
			this( originDb ) {
			Artist = artist;
		}

		protected ExportBase( long originDb, string artist, string album ) :
			this( originDb ) {
			Artist = artist;
			Album = album;
		}

		protected ExportBase( long originDb, string artist, string album, string track ) :
			this( originDb ) {
			Artist = artist;
			Album = album;
			Track = track;
		}

		protected ExportBase( string stream, long originDb ) :
			this( originDb ) {
			Stream = stream;
		}

		protected ExportBase( DataFindResults results ) :
				this( results.DatabaseId ) {
			Track = results.Track != null ? results.Track.Name : "";
			Album = results.Album != null ? results.Album.Name : "";
			Artist = results.Artist != null ? results.Artist.Name : "";
		}

		public bool Compare( ExportBase two ) {
			var retValue = true;

			if( two != null ) {
				retValue &= ( string.IsNullOrWhiteSpace( Artist ) && string.IsNullOrWhiteSpace( two.Artist ) ) ||
							( Artist.Equals( two.Artist, StringComparison.CurrentCultureIgnoreCase ) );
				retValue &= ( string.IsNullOrWhiteSpace( Album ) && string.IsNullOrWhiteSpace( two.Album ) ) ||
							( Album.Equals( two.Album, StringComparison.CurrentCultureIgnoreCase ) );
				retValue &= ( string.IsNullOrWhiteSpace( Track ) && string.IsNullOrWhiteSpace( two.Track ) ) ||
							( Track.Equals( two.Track, StringComparison.CurrentCultureIgnoreCase ) );
				retValue &= ( string.IsNullOrWhiteSpace( Stream ) && string.IsNullOrWhiteSpace( two.Stream ) ) ||
							( Stream.Equals( two.Stream, StringComparison.CurrentCultureIgnoreCase ) );
			}
			else {
				retValue = false;
			}

			return( retValue );
		}
	}
}
