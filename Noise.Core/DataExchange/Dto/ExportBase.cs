using System;

namespace Noise.Core.DataExchange.Dto {
	public enum eExportClass {
		Artist = 1,
		Album = 2,
		Track = 3,
		Stream = 4
	}

	public class ExportBase {
		public	long			CreationDateTicks { get; private set; }
		public	long			OriginDb { get; private set; }
		public	eExportClass	ExportClass {get; private set; }
		public	long			SequenceId { get; set; }
		public	string			Artist { get; private set; }
		public	string			Album { get; private set; }
		public	string			Track { get; private set; }
		public	string			Stream { get; private set; }

		private ExportBase( long originDb, eExportClass exportClass ) {
			ExportClass = exportClass;
			OriginDb = originDb;
			CreationDateTicks = DateTime.Now.Ticks;
		}

		protected ExportBase( long originDb, string artist ) :
			this( originDb, eExportClass.Artist ) {
			Artist = artist;
		}

		protected ExportBase( long originDb, string artist, string album ) :
			this( originDb, eExportClass.Album ) {
			Artist = artist;
			Album = album;
		}

		protected ExportBase( long originDb, string artist, string album, string track ) :
			this( originDb, eExportClass.Track ) {
			Artist = artist;
			Album = album;
			Track = track;
		}

		protected ExportBase( string stream, long originDb ) :
			this( originDb, eExportClass.Stream ) {
			Stream = stream;
		}
	}
}
