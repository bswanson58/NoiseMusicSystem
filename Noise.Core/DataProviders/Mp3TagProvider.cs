using System;
using CuttingEdge.Conditions;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Core.MetaData;
using TagLib;

namespace Noise.Core.DataProviders {
	public class Mp3TagProvider {
		private readonly IDatabaseManager	mDatabase;

		public Mp3TagProvider( IDatabaseManager databaseManager ) {
			mDatabase = databaseManager;
		}

		public void BuildMetaData( StorageFile storageFile, DbTrack track ) {
			try {
				var	tags = File.Create( StorageHelpers.GetPath( mDatabase.Database, storageFile ));
				var	parm = mDatabase.Database.CreateParameters();

				parm["artistName"] = tags.Tag.FirstAlbumArtist;
				parm["albumName"] = tags.Tag.Album;

				var	artist = mDatabase.Database.ExecuteScalar( "SELECT DbArtist WHERE Name = @artistName", parm ) as DbArtist;
				if( artist == null ) {
					artist = new DbArtist();

					if(!String.IsNullOrEmpty( tags.Tag.FirstAlbumArtist )) {
						artist.Name = tags.Tag.FirstAlbumArtist;
					}
					else {
						if(!String.IsNullOrEmpty( tags.Tag.FirstPerformer )) {
							artist.Name = tags.Tag.FirstPerformer;
						}
					}

					Condition.Requires( artist.Name ).IsNotNullOrEmpty( "Artist name must not be empty." );

					mDatabase.Database.Store( artist );
				}

				var	album = mDatabase.Database.ExecuteScalar( "SELECT DbAlbum WHERE Name = @albumName", parm ) as DbAlbum;
				if( album == null ) {
					album = new DbAlbum { Name = tags.Tag.Album, Artist = mDatabase.Database.GetUid( artist ) };

					Condition.Requires( album.Name ).IsNotNullOrEmpty( "Album name must not be empty." );
					mDatabase.Database.Store( album );
				}
				track.Album = mDatabase.Database.GetUid( album );

				if( tags.Tag.Year != 0 ) {
					track.PublishedYear = tags.Tag.Year;
				}

				track.Bitrate = tags.Properties.AudioBitrate;
				track.SampleRate = tags.Properties.AudioSampleRate;
				track.Duration = tags.Properties.Duration;
				track.Channels = (Int16)tags.Properties.AudioChannels;

				if(( tags.Tag.Genres != null ) &&
				   ( tags.Tag.Genres.GetLength( 0 ) > 0 )) {
	//				track.Genre = tags.Tag.Genres[0];
				}
			}
			catch( Exception ex ) {
				
			}
		}
	}
}
