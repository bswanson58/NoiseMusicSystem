using System;
using CuttingEdge.Conditions;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using TagLib;

namespace Noise.Core.DataProviders {
	public class Mp3TagProvider : IMetaDataProvider {
		private readonly IDatabaseManager	mDatabaseManager;
		private readonly ITagManager		mGenreManager;
		private readonly StorageFile		mFile;
		private readonly ILog				mLog;
		private	readonly Lazy<File>			mTags;

		public Mp3TagProvider( IUnityContainer container, StorageFile file ) {
			mDatabaseManager = container.Resolve<IDatabaseManager>();
			mFile = file;

			var manager = container.Resolve<INoiseManager>();
			mGenreManager = manager.TagManager;

			Condition.Requires( mDatabaseManager ).IsNotNull();
			Condition.Requires( mFile ).IsNotNull();

			mTags =new Lazy<File>(() => {
				File	retValue = null;
				var		database = mDatabaseManager.ReserveDatabase();

				try {
					retValue = OpenTagFile( StorageHelpers.GetPath( database.Database, mFile ));
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - Mp3TagProvider:OpenTagFile:", ex );
				}
				finally {
					mDatabaseManager.FreeDatabase( database );
				}

				return( retValue );	});

			mLog = container.Resolve<ILog>();
		}

		private File OpenTagFile( string path ) {
			File retValue = null;

			try {
				retValue = File.Create( path );
			}
			catch( Exception ex ) {
				mLog.LogException( String.Format( "Exception - Mp3TagProvider opening file: {0}", path ), ex );
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
				var database = mDatabaseManager.ReserveDatabase();
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
						var parms = database.Database.CreateParameters();

						parms["folderId"] = mFile.ParentFolder;

						if( database.Database.ExecuteScalar( "SELECT DbArtwork WHERE FolderLocation = @folderId", parms ) == null ) {
	//					if(( from DbArtwork artwork in mDatabase.Database where artwork.FolderLocation == storageFile.ParentFolder select artwork ).Count() == 0 ) {
							foreach( var picture in pictures ) {
								var dbPicture = new DbArtwork( track.Album, picture.Type == PictureType.FrontCover ? ContentType.AlbumCover : ContentType.AlbumArtwork )
										{ Source = InfoSource.Tag,
										  FolderLocation = mFile.ParentFolder };
								dbPicture.Image = new byte[picture.Data.Count];
								picture.Data.CopyTo( dbPicture.Image, 0 );

								database.Insert( dbPicture );
							}
						}
					}

					if(( track.ExternalGenre == Constants.cDatabaseNullOid ) &&
					   ( Tags.Tag.Genres != null ) &&
					   ( Tags.Tag.Genres.GetLength( 0 ) > 0 )) {
						track.ExternalGenre = mGenreManager.ResolveGenre( Tags.Tag.Genres[0]);
					}
				}
				catch( Exception ex ) {
					mLog.LogException( "Mp3TagProvider", ex );
				}
				finally {
					mDatabaseManager.FreeDatabase( database );
				}
			}
		}
	}
}
