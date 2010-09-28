using System;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	public class SearchBuilder : ISearchBuilder {
		private readonly IUnityContainer	mContainer;
		private readonly INoiseManager		mNoiseManager;
		private readonly ILog				mLog;
		private bool						mStopBuilding;

		public SearchBuilder( IUnityContainer container ) {
			mContainer = container;
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mLog = mContainer.Resolve<ILog>();
		}

		public void BuildSearchIndex() {
			mStopBuilding = false;

			mLog.LogMessage( "Building Search Index." );

			var databaseMgr = mContainer.Resolve<IDatabaseManager>();
			var database = databaseMgr.ReserveDatabase();

			if( database != null ) {
				try {
					if( mNoiseManager.SearchProvider.StartIndexUpdate( true )) {
						var artistList = from DbArtist artist in database.Database select artist;

						foreach( var artist in artistList ) {
							using( var searchItem = mNoiseManager.SearchProvider.AddSearchItem()) {
								searchItem.AddIndex( "artistId", artist.DbId.ToString());
								searchItem.AddSearchText( "artistName", artist.Name );
							}
							if( mStopBuilding ) {
								break;
							}
						}
					}
					else {
						mLog.LogMessage( "SearchProvider Could not start index update." );
					}
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - Building search data: ", ex );
				}
				finally {
					databaseMgr.FreeDatabase( database );
					mNoiseManager.SearchProvider.EndIndexUpdate();
				}
			}
		}

		public void Stop() {
			mStopBuilding = true;
		}
	}
}
