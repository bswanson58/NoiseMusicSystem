using System.ComponentModel.Composition;
using System.Linq;
using GDataDB.Linq;
using Noise.Core.DataExchange.Dto;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using IDatabase = GDataDB.IDatabase;

namespace Noise.Core.DataExchange {
	[Export( typeof( ICloudSyncProvider ))]
	internal class CloudSyncFavorites : ICloudSyncProvider {
		private IDataProvider		mDataProvider;
		private IDatabase			mCloudDatabase;

		public bool Initialize( IDataProvider dataProvider, IDatabase cloudDatabase ) {
			mDataProvider = dataProvider;
			mCloudDatabase = cloudDatabase;

			return( true );
		}

		public ObjectTypes SyncTypes {
			get{ return( ObjectTypes.Favorites ); }
		}

		public void UpdateFromCloud( long startSeqn, long toSeqn ) {
			var favoritesTable = mCloudDatabase.GetTable<ExportFavorite>( Constants.CloudSyncFavoritesTable ) ??
								 mCloudDatabase.CreateTable<ExportFavorite>( Constants.CloudSyncFavoritesTable );
			var cloudFavorites = from ExportFavorite e in favoritesTable.AsQueryable()
								 where e.SequenceId > startSeqn && e.SequenceId <= toSeqn select e;
			var updateCount = 0;
			foreach( var favorite in cloudFavorites ) {
				var dbEntry = mDataProvider.Find( favorite.Artist, favorite.Album, favorite.Track );

				if(( dbEntry != null ) &&
				   ( dbEntry.WasSuccessful )) {
					if( dbEntry.Track != null ) {
						if( dbEntry.Track.IsFavorite != favorite.IsFavorite ) {
							GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( dbEntry.Track.DbId, favorite.IsFavorite ));

							updateCount++;
						}
					}
					else if( dbEntry.Album != null ) {
						if( dbEntry.Album.IsFavorite != favorite.IsFavorite ) {
							GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( dbEntry.Album.DbId, favorite.IsFavorite ));

							updateCount++;
						}
					}
					else if( dbEntry.Artist != null ) {
						if( dbEntry.Artist.IsFavorite != favorite.IsFavorite ) {
							GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( dbEntry.Artist.DbId, favorite.IsFavorite ));

							updateCount++;
						}
					}
				}
			}

			if( updateCount > 0 ) {
				NoiseLogger.Current.LogInfo( string.Format( "Favorites - UpdateFromCloud: {0} item(s).", updateCount ));
			}
		}

		public void UpdateToCloud( ExportBase item ) {
			if( item is ExportFavorite ) {
				var favoriteExport = item as ExportFavorite;
				var favoriteDb = mCloudDatabase.GetTable<ExportFavorite>( Constants.CloudSyncFavoritesTable ) ??
								 mCloudDatabase.CreateTable<ExportFavorite>( Constants.CloudSyncFavoritesTable );
				if( favoriteDb != null ) {
					var row = favoriteDb.FindAll().Where( e => e.Element.Compare( item )).FirstOrDefault();

					if( row != null ) {
						row.Element.IsFavorite = favoriteExport.IsFavorite;
						row.Element.SequenceId = favoriteExport.SequenceId;
						row.Element.OriginDb = favoriteExport.OriginDb;

						row.Update();
					}
					else {
						favoriteDb.Add( favoriteExport );
					}
				}

				NoiseLogger.Current.LogInfo( string.Format( "Updated favorite to cloud: {0}({1}/{2})", item.Track, item.Artist, item.Album ));
			}
		}
	}
}
