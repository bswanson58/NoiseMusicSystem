﻿using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	public class DataProvider : IDataProvider {
		private readonly IDatabaseManager	mDatabase;

		public DataProvider( IDatabaseManager database ) {
			mDatabase = database;
		}

		public IEnumerable<DbArtist> GetArtistList() {
			return( from DbArtist artist in mDatabase.Database select artist );
		}

		public IEnumerable<DbAlbum> GetAlbumList( DbArtist forArtist ) {
			var artistId = mDatabase.Database.GetUid( forArtist );

			return( from DbAlbum album in mDatabase.Database where album.Artist == artistId select album );
		}

		public IEnumerable<DbTrack> GetTrackList( DbAlbum forAlbum ) {
			var albumId = mDatabase.Database.GetUid( forAlbum );

			return( from DbTrack track in mDatabase.Database where track.Album == albumId select track );
		}

		public IEnumerable<DbTrack> GetTrackList( DbArtist forArtist ) {
			var	retValue = new List<DbTrack>();
			var artistId = mDatabase.Database.GetUid( forArtist );
			var albumList = from DbAlbum album in mDatabase.Database where album.Artist == artistId select album;

			foreach( DbAlbum album in albumList ) {
				var albumId = mDatabase.Database.GetUid( album );
				var trackList = from DbTrack track in mDatabase.Database where track.Album == albumId select track;

				retValue.AddRange( trackList );
			}

			return( retValue );
		}

		public StorageFile GetPhysicalFile( DbTrack forTrack ) {
			var trackId = mDatabase.Database.GetUid( forTrack );

			return((from StorageFile file in mDatabase.Database where file.MetaDataPointer == trackId select file ).FirstOrDefault());
		}

		public object GetMetaData( StorageFile forFile ) {
			var parm = mDatabase.Database.CreateParameters();

			parm["id"] = forFile.MetaDataPointer;

			return( mDatabase.Database.ExecuteScalar( "SELECT data WHERE $ID = @id", parm ));
		}
	}
}
