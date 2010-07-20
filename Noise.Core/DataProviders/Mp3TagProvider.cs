using System;
using CuttingEdge.Conditions;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using TagLib;

namespace Noise.Core.DataProviders {
	public class Mp3TagProvider : IMetaDataProvider {
		private readonly StorageFile		mFile;
		private readonly IDatabaseManager	mDatabase;
		private readonly ILog				mLog;
		private	readonly Lazy<File>			mTags;

		public Mp3TagProvider( IDatabaseManager databaseManager, StorageFile file ) {
			mDatabase = databaseManager;
			mFile = file;

			Condition.Requires( mDatabase ).IsNotNull();
			Condition.Requires( mFile ).IsNotNull();

			mTags =new Lazy<File>(() => OpenTagFile( StorageHelpers.GetPath( mDatabase.Database, mFile )));
			mLog = new Log();
		}

		private File OpenTagFile( string path ) {
			File retValue = null;

			try {
				retValue = File.Create( path );
			}
			catch( Exception ex ) {
				mLog.LogException( String.Format( "Opening mp3 tag for file: {0}", path ), ex );
			}

			return( retValue );			
		}

		private File Tags {
			get { return( mTags.Value ); }
		}

		public string Artist {
			get {
				var retValue = "";

				if( Tags != null ) { 
					if(!String.IsNullOrEmpty( Tags.Tag.FirstAlbumArtist )) {
						retValue = Tags.Tag.FirstAlbumArtist;
					}
					else {
						if(!String.IsNullOrEmpty( Tags.Tag.FirstPerformer )) {
							retValue = Tags.Tag.FirstPerformer;
						}
					}
				}

				return( retValue );
			}
		}

		public string Album {
			get {
				var retValue = "";

				if( Tags != null ) {
					retValue = Tags.Tag.Album;
				}

				return( retValue );
			}
		}

		public string TrackName {
			get {
				var retValue = "";

				if( Tags != null ) {
					retValue = Tags.Tag.Title;
				}

				return( retValue );
			}
		}

		public string VolumeName {
			get {
				var retValue = "";

				if( Tags != null ) {
					retValue = Tags.Tag.Disc > 0 ? Tags.Tag.Disc.ToString() : "";
				}

				return( retValue );
			}
		}

		public void AddAvailableMetaData( DbArtist artist, DbAlbum album, DbTrack track ) {
			if( Tags != null ) {
				try {
					if( Tags.Tag.Year != 0 ) {
						track.PublishedYear = Tags.Tag.Year;
					}

					track.Performer = !String.IsNullOrWhiteSpace( Tags.Tag.FirstPerformer ) ? Tags.Tag.FirstPerformer : artist.Name;

					track.Bitrate = Tags.Properties.AudioBitrate;
					track.SampleRate = Tags.Properties.AudioSampleRate;
					track.DurationMilliseconds = (Int32)Tags.Properties.Duration.TotalMilliseconds;
					track.Channels = (Int16)Tags.Properties.AudioChannels;

					var pictures = Tags.Tag.Pictures;
					if(( pictures != null ) &&
					   ( pictures.GetLength( 0 ) > 0 )) {
						// Only pull the pictures from the first file in the folder.
						var parms = mDatabase.Database.CreateParameters();

						parms["folderId"] = mFile.ParentFolder;

						if( mDatabase.Database.ExecuteScalar( "SELECT DbArtwork WHERE FolderLocation = @folderId", parms ) == null ) {
	//					if(( from DbArtwork artwork in mDatabase.Database where artwork.FolderLocation == storageFile.ParentFolder select artwork ).Count() == 0 ) {
							foreach( var picture in pictures ) {
								var dbPicture = new DbArtwork( track.Album ) { ArtworkType = picture.Type == PictureType.FrontCover ? ArtworkTypes.AlbumCover : ArtworkTypes.AlbumOther,
																			   Source = InfoSource.Tag,
																			   FolderLocation = mFile.ParentFolder };
								dbPicture.Image = new byte[picture.Data.Count];
								picture.Data.CopyTo( dbPicture.Image, 0 );

								mDatabase.Database.Store( dbPicture );
							}
						}
					}

					if(( Tags.Tag.Genres != null ) &&
					   ( Tags.Tag.Genres.GetLength( 0 ) > 0 )) {
						track.Genre = Tags.Tag.Genres[0];
					}
				}
				catch( Exception ex ) {
					mLog.LogException( "Mp3TagProvider", ex );
				}
			}
		}
	}
}
