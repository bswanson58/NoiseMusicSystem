using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.DataBuilders {
	public class MetaDataCleaner : IMetaDataCleaner {
		private readonly IUnityContainer	mContainer;
		private readonly IDatabaseManager	mDatabaseManager;
		private readonly ILog				mLog;
		private bool						mStopCleaning;
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

		public void CleanDatabase() {
			mStopCleaning = false;
			mAlbumList.Clear();

			mLog.LogInfo( "Starting MetaDataCleaning." );

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
				CleanFolderFiles( database, folder );

				mLog.LogInfo( string.Format( "Deleting Folder: {0}", StorageHelpers.GetPath( database.Database, folder )));
				database.Database.Delete( folder );
			}
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
			var parms = database.Database.CreateParameters();
			parms["id"] = file.MetaDataPointer;

			var associatedItem = database.Database.ExecuteScalar( "SELECT DbBase WHERE DbId = @id", parms ) as DbBase;
			if( associatedItem != null ) {
				TypeSwitch.Do( associatedItem, TypeSwitch.Case<DbTrack>( CleanTrack),
											   TypeSwitch.Case<ExpiringContent>( CleanContent ));
				database.Database.Delete( associatedItem );
			}

			database.Database.Delete( file );
		}

		private void CleanTrack( DbTrack track ) {
			if(!mAlbumList.Contains( track.Album )) {
				mAlbumList.Add( track.Album );
			}
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

				var albums = database.Database.ExecuteQuery( "SELECT DbAlbum WHERE Artist = @artistId", parms ).OfType<DbAlbum>();

				if( albums.Count() == 0 ) {
					var artist = database.Database.ExecuteScalar( "SELECT DbArtist WHERE DbId = @artistId", parms ) as DbArtist;

					if( artist != null ) {
						mLog.LogInfo( string.Format( "Deleting Artist: {0}", artist.Name ));

						database.Database.Delete( artist );
					}
				}
			}
		}

		private IEnumerable<long> CleanAlbums( IDatabase database, IEnumerable<long> albums ) {
			var retValue = new List<long>();
			var parms = database.Database.CreateParameters();

			foreach( var albumId in albums ) {
				parms["albumId"] = albumId;

				var tracks = database.Database.ExecuteQuery( "SELECT DbTrack WHERE Album = @albumId", parms ).OfType<DbTrack>();

				if( tracks.Count() == 0 ) {
					var content = database.Database.ExecuteQuery( "SELECT ExpiringContent WHERE Album = @albumId", parms ).OfType<ExpiringContent>();

					if( content.Count() == 0 ) {
						var album = database.Database.ExecuteScalar( "SELECT DbAlbum WHERE DbId = @albumId", parms ) as DbAlbum;

						if( album != null ) {
							if(!retValue.Contains( album.Artist )) {
								retValue.Add( album.Artist );
							}

							mLog.LogInfo( string.Format( "Deleting Album: {0}", album.Name ));

							database.Database.Delete( album );
						}
					}
				}
			}

			return( retValue );
		}
	}
}
