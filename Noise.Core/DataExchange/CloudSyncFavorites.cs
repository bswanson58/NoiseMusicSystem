using System.ComponentModel.Composition;
using System.Linq;
using GDataDB.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.DataExchange.Dto;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataExchange {
	[Export( typeof( ICloudSyncProvider ))]
	internal class CloudSyncFavorites : ICloudSyncProvider {
		private IUnityContainer		mContainer;
		private GDataDB.IDatabase	mCloudDatabase;

		public bool Initialize( IUnityContainer container, GDataDB.IDatabase cloudDatabase ) {
			mContainer = container;
			mCloudDatabase = cloudDatabase;

			return( true );
		}

		public ObjectTypes SyncTypes {
			get{ return( ObjectTypes.Favorites ); }
		}

		public void UpdateFromCloud( long startSeqn, long toSeqn ) {
			var noiseManager = mContainer.Resolve<INoiseManager>();
			var favoritesTable = mCloudDatabase.GetTable<ExportFavorite>( Constants.CloudSyncFavoritesTable ) ??
								 mCloudDatabase.CreateTable<ExportFavorite>( Constants.CloudSyncFavoritesTable );
			var cloudFavorites = from ExportFavorite e in favoritesTable.AsQueryable()
									  where e.SequenceId > startSeqn && e.SequenceId <= toSeqn select e;
			foreach( var favorite in cloudFavorites ) {
				var dbEntry = noiseManager.DataProvider.Find( favorite.Artist, favorite.Album, favorite.Track );

				if(( dbEntry != null ) &&
				   ( dbEntry.WasSuccessful )) {
					if( dbEntry.Track != null ) {
						if( dbEntry.Track.IsFavorite != favorite.IsFavorite ) {
							GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( dbEntry.Track.DbId, favorite.IsFavorite ));
						}
					}
					else if( dbEntry.Album != null ) {
						if( dbEntry.Album.IsFavorite != favorite.IsFavorite ) {
							GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( dbEntry.Album.DbId, favorite.IsFavorite ));
						}
					}
					else if( dbEntry.Artist != null ) {
						if( dbEntry.Artist.IsFavorite != favorite.IsFavorite ) {
							GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( dbEntry.Artist.DbId, favorite.IsFavorite ));
						}
					}
				}
			}
		}
	}
}
