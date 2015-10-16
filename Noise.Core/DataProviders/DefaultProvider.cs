using Noise.Infrastructure.Dto;

namespace Noise.Core.DataProviders {
	internal class DefaultProvider {
		public IMetaDataProvider GetProvider( StorageFile forFile ) {
			return( new UnknownProvider());
		}
	}

	internal class UnknownProvider : IMetaDataProvider {
		public string Artist {
			get{ return( "Unknown Artist" ); }
		}

		public string Album {
			get{ return( string.Empty ); }
		}

		public string TrackName {
			get{ return( string.Empty ); }
		}

		public string VolumeName {
			get{ return( string.Empty ); }
		}

		public void AddAvailableMetaData( DbArtist artist, DbAlbum album, DbTrack track ) {
			
		}
	}
}
