﻿using System;
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

		public void BuildMetaData( StorageFile storageFile, MusicTrack track ) {
			try {
				var	tags = File.Create( StorageHelpers.GetPath( mDatabase.Database, storageFile ));
				var	parm = mDatabase.Database.CreateParameters();

				parm["artistName"] = tags.Tag.FirstAlbumArtist;
				parm["albumName"] = tags.Tag.Album;

				var	artist = mDatabase.Database.ExecuteScalar( "SELECT Artist WHERE Name = @artistName", parm ) as Artist;
				if( artist == null ) {
					artist = new Artist { Name = tags.Tag.FirstAlbumArtist };

					mDatabase.Database.Store( artist );
				}

				var	album = mDatabase.Database.ExecuteScalar( "SELECT Album WHERE Name = @albumName", parm ) as Album;
				if( album == null ) {
					album = new Album { Name = tags.Tag.Album, Artist = mDatabase.Database.GetUid( artist ) };

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
