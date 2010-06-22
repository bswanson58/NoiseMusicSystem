using System;
using System.IO;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.DataProviders;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataBuilders {
	public class MetaDataExplorer : IMetaDataExplorer {
		private readonly IUnityContainer	mContainer;
		private readonly IDatabaseManager	mDatabase;

		public  MetaDataExplorer( IUnityContainer container ) {
			mContainer = container;
			mDatabase = mContainer.Resolve<IDatabaseManager>();
		}

		public void BuildMetaData() {
			var		fileNameProvider = new FileNameProvider( mDatabase );
			var		tagProvider = new FileTagProvider( mDatabase );
			var		fileEnum = from StorageFile file in mDatabase.Database where file.FileType == eFileType.Undetermined orderby file.ParentFolder select file;

			foreach( var file in fileEnum ) {
				file.FileType = DetermineFileType( file );

				switch( file.FileType ) {
					case eFileType.Music:
						var		track = new DbTrack();

						fileNameProvider.BuildMetaData( file, track  );
						tagProvider.BuildMetaData( file, track );

						file.MetaDataPointer = mDatabase.Database.Store( track );
						mDatabase.Database.Store( file );
						break;
				}
			}

			UpdateCounts();

			var	lastFmProvider = new LastFmProvider( mDatabase );
			lastFmProvider.BuildMetaData();
		}

		private static eFileType DetermineFileType( StorageFile file ) {
			var retValue = eFileType.Unknown;
			var ext = Path.GetExtension( file.Name ).ToLower();

			switch( ext ) {
				case ".mp3":
					retValue = eFileType.Music;
					break;
			}

			return( retValue );
		}

		private void UpdateCounts() {
			var artists = from DbArtist artist in mDatabase.Database select artist;

			foreach( var artist in artists ) {
				var artistId = mDatabase.Database.GetUid( artist );
				var albums = from DbAlbum album in mDatabase.Database where album.Artist == artistId select album;

				foreach( var album in albums ) {
					var albumId = mDatabase.Database.GetUid( album );
					var tracks = from DbTrack track in mDatabase.Database where track.Album == albumId select track;

					album.TrackCount = (Int16)tracks.Count();
					mDatabase.Database.Store( album );
				}

				artist.AlbumCount = (Int16)albums.Count();
				mDatabase.Database.Store( artist );
			}
		}
	}
}
