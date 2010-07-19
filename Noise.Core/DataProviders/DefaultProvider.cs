﻿using Noise.Infrastructure.Dto;

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
			get{ return( "Unknown Album" ); }
		}

		public string TrackName {
			get{ return( "Unknown Track" ); }
		}

		public string VolumeName {
			get{ return( "" ); }
		}

		public void AddAvailableMetaData( DbArtist artist, DbAlbum album, DbTrack track ) {
			
		}
	}
}
