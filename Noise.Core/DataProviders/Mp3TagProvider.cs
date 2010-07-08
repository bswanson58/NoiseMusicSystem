using System;
using CuttingEdge.Conditions;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using TagLib;

namespace Noise.Core.DataProviders {
	public class Mp3TagProvider {
		private readonly IDatabaseManager	mDatabase;
		private readonly ILog				mLog;

		public Mp3TagProvider( IDatabaseManager databaseManager ) {
			mDatabase = databaseManager;

			mLog = new Log();
		}

		public void BuildMetaData( StorageFile storageFile, DbTrack track ) {
			try {
				var	tags = File.Create( StorageHelpers.GetPath( mDatabase.Database, storageFile ));
				var	parm = mDatabase.Database.CreateParameters();
				var artistName = "";

				if(!String.IsNullOrEmpty( tags.Tag.FirstAlbumArtist )) {
					artistName = tags.Tag.FirstAlbumArtist;
				}
				else {
					if(!String.IsNullOrEmpty( tags.Tag.FirstPerformer )) {
						artistName = tags.Tag.FirstPerformer;
					}
				}
				Condition.Requires( artistName ).IsNotNullOrEmpty( "Artist name must not be empty." );

				parm["artistName"] = artistName;
				parm["albumName"] = tags.Tag.Album;

				var	artist = mDatabase.Database.ExecuteScalar( "SELECT DbArtist WHERE Name = @artistName", parm ) as DbArtist;
				if( artist == null ) {
					artist = new DbArtist { Name = artistName };

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

				track.Performer = !String.IsNullOrWhiteSpace( tags.Tag.FirstPerformer ) ? tags.Tag.FirstPerformer : artistName;

				track.Bitrate = tags.Properties.AudioBitrate;
				track.SampleRate = tags.Properties.AudioSampleRate;
				track.DurationMilliseconds = (Int32)tags.Properties.Duration.TotalMilliseconds;
				track.Channels = (Int16)tags.Properties.AudioChannels;

				var pictures = tags.Tag.Pictures;
				if(( pictures != null ) &&
				   ( pictures.GetLength( 0 ) > 0 )) {
					// Only pull the pictures from the first file in the folder.
					var parms = mDatabase.Database.CreateParameters();

					parms["folderId"] = storageFile.ParentFolder;

					if( mDatabase.Database.ExecuteScalar( "SELECT DbArtwork WHERE FolderLocation = @folderId", parms ) == null ) {
//					if(( from DbArtwork artwork in mDatabase.Database where artwork.FolderLocation == storageFile.ParentFolder select artwork ).Count() == 0 ) {
						foreach( var picture in pictures ) {
							var dbPicture = new DbArtwork( track.Album ) { ArtworkType = picture.Type == PictureType.FrontCover ? ArtworkTypes.AlbumCover : ArtworkTypes.AlbumOther,
																		   Source = InfoSource.Tag,
																		   FolderLocation = storageFile.ParentFolder };
							dbPicture.Image = new byte[picture.Data.Count];
							picture.Data.CopyTo( dbPicture.Image, 0 );

							mDatabase.Database.Store( dbPicture );
						}
					}
				}

				if(( tags.Tag.Genres != null ) &&
				   ( tags.Tag.Genres.GetLength( 0 ) > 0 )) {
					track.Genre = tags.Tag.Genres[0];
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Mp3TagProvider", ex );
			}
		}
	}
}
