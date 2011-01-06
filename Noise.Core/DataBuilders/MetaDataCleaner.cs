using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.DataBuilders {
	public class MetaDataCleaner : IMetaDataCleaner {
		private readonly IUnityContainer	mContainer;
		private readonly IDatabaseManager	mDatabaseManager;
		private readonly ILog				mLog;
		private bool						mStopCleaning;
		private DatabaseChangeSummary		mSummary;
		private readonly List<long>			mAlbumList;

		public MetaDataCleaner( IUnityContainer container ) {
			mContainer =  container;
			mDatabaseManager = mContainer.Resolve<IDatabaseManager>();
			mLog = mContainer.Resolve<ILog>();

			mAlbumList = new List<long>();
		}
		public void Stop() {
			mStopCleaning = true;
		}

		public void CleanDatabase( DatabaseChangeSummary summary ) {
			Condition.Requires( summary ).IsNotNull();

			mSummary = summary;
			mStopCleaning = false;
			mAlbumList.Clear();

			mLog.LogMessage( "Starting MetaDataCleaning." );

			var database = mDatabaseManager.ReserveDatabase();

			try {
				CleanFolders( database );
				CleanFiles( database );

				CleanArtists( database, CleanAlbums( database, mAlbumList ));
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - MetaDataCleaner:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		private void CleanFolders( IDatabase database ) {
			var deletedFolders = database.Database.ExecuteQuery( "SELECT StorageFolder WHERE IsDeleted" ).OfType<StorageFolder>();

			foreach( var folder in deletedFolders ) {
				CleanFolder( database, folder );

				if( mStopCleaning ) {
					break;
				}
			}
		}

		private void CleanFolder( IDatabase database, StorageFolder folder ) {
			var parms = database.Database.CreateParameters();
			parms["parentId"] = folder.DbId;

			var childFolders = database.Database.ExecuteQuery( "SELECT StorageFolder WHERE ParentFolder = @parentId", parms ).OfType<StorageFolder>();
			foreach( var childFolder in childFolders ) {
				CleanFolder( database, childFolder );
			}

			CleanFolderFiles( database, folder );

			mLog.LogMessage( string.Format( "Deleting Folder: {0}", folder.Name ));
			database.Delete( folder );
		}

		private void CleanFolderFiles( IDatabase database, StorageFolder folder ) {
			var	parms = database.Database.CreateParameters();
			parms["parentId"] = folder.DbId;

			var fileList = database.Database.ExecuteQuery( "SELECT StorageFile WHERE ParentFolder = @parentId", parms ).OfType<StorageFile>();
			foreach( var file in fileList ) {
				CleanFile( database, file );
			}
		}

		private void CleanFiles( IDatabase database ) {
			var fileList = database.Database.ExecuteQuery( "SELECT StorageFile WHERE IsDeleted" ).OfType<StorageFile>();

			foreach( var file in fileList ) {
				CleanFile( database, file );

				if( mStopCleaning ) {
					break;
				}
			}
		}

		private void CleanFile( IDatabase database, StorageFile file ) {
			if( file.MetaDataPointer != Constants.cDatabaseNullOid ) {
				var parms = database.Database.CreateParameters();
				parms["id"] = file.MetaDataPointer;

				var associatedItem = database.Database.ExecuteScalar( "SELECT DbBase WHERE DbId = @id", parms ) as DbBase;
				if( associatedItem != null ) {
					TypeSwitch.Do( associatedItem, TypeSwitch.Case<DbTrack>( CleanTrack),
												   TypeSwitch.Case<DbArtwork>( CleanContent ),
												   TypeSwitch.Case<DbTextInfo>( CleanContent ));
					database.Delete( associatedItem );
				}
			}

			database.Delete( file );
		}

		private void CleanTrack( DbTrack track ) {
			if(!mAlbumList.Contains( track.Album )) {
				mAlbumList.Add( track.Album );
			}

			mSummary.TracksRemoved++;
			mLog.LogMessage( "Deleting Track: {0}", track.Name );
		}

		private void CleanContent( ExpiringContent content ) {
			if(!mAlbumList.Contains( content.Album )) {
				mAlbumList.Add( content.Album );
			}
		}

		private void CleanArtists( IDatabase database, IEnumerable<long> artists ) {
			var parms = database.Database.CreateParameters();

			foreach( var artistId in artists ) {
				parms["artistId"] = artistId;

				var artist = database.Database.ExecuteScalar( "SELECT DbArtist WHERE DbId = @artistId", parms ) as DbArtist;

				if( artist != null ) {
					var albums = database.Database.ExecuteQuery( "SELECT DbAlbum WHERE Artist = @artistId", parms ).OfType<DbAlbum>();

					if( albums.Count() == 0 ) {
						mLog.LogMessage( string.Format( "Deleting Artist: {0}", artist.Name ));
						mSummary.ArtistsRemoved++;

						database.Delete( artist );
					}
				}
			}
		}

		private IEnumerable<long> CleanAlbums( IDatabase database, IEnumerable<long> albums ) {
			var retValue = new List<long>();
			var parms = database.Database.CreateParameters();

			foreach( var albumId in albums ) {
				parms["albumId"] = albumId;

				var album = database.Database.ExecuteScalar( "SELECT DbAlbum WHERE DbId = @albumId", parms ) as DbAlbum;
				if( album != null ) {
					parms["artistId"] = album.Artist;

					var tracks = database.Database.ExecuteQuery( "SELECT DbTrack WHERE Album = @albumId", parms ).OfType<DbTrack>();

					if( tracks.Count() == 0 ) {
						var content = database.Database.ExecuteQuery( "SELECT ExpiringContent WHERE Album = @albumId", parms ).OfType<ExpiringContent>();

						if( content.Count() == 0 ) {
							if(!retValue.Contains( album.Artist )) {
								retValue.Add( album.Artist );
							}

							mLog.LogMessage( string.Format( "Deleting Album: {0}", album.Name ));
							mSummary.AlbumsRemoved++;

							database.Delete( album );
						}
					}

					var artist = database.Database.ExecuteScalar( "SELECT DbArtist WHERE DbId = @artistId", parms ) as DbArtist;
					if( artist != null ) {
						mSummary.AddChangedArtist( artist );
					}
				}
			}

			return( retValue );
		}
	}
}
