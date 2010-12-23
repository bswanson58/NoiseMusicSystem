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

		private ExportBase( long originDb ) {
			OriginDb = originDb;
			CreationDateTicks = DateTime.Now.Ticks;
		}

		protected ExportBase() {
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

		protected ExportBase( DataFindResults results ) {
			CreationDateTicks = DateTime.Now.Ticks;
			OriginDb = results.DatabaseId;

			Track = results.Track != null ? results.Track.Name : "";
			Album = results.Album != null ? results.Album.Name : "";
			Artist = results.Artist != null ? results.Artist.Name : "";
		}

		public string ConstructQuery() {
			return(!string.IsNullOrWhiteSpace( Track ) ? string.Format( "Artist=\"{0}\" and Album=\"{1}\" and Track=\"{2}\"", Artist, Album, Track ) :
				   !string.IsNullOrWhiteSpace( Album ) ? string.Format( "Artist=\"{0}\" and Album=\"{1}\"", Artist, Album ) :
														 string.Format( "Artist=\"{0}\"", Artist ));
		}
	}
}
