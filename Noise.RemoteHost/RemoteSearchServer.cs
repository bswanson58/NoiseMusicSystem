using System;
using System.Linq;
using System.ServiceModel;
using AutoMapper;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteDto;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class RemoteSearchServer : INoiseRemoteSearch {
		private const int cMaxSearchResults = 100;

		private readonly IUnityContainer	mContainer;
		private	readonly INoiseManager		mNoiseManager;

		public RemoteSearchServer( IUnityContainer container ) {
			mContainer = container;
			mNoiseManager = mContainer.Resolve<INoiseManager>();
		}

		private static RoSearchResultItem TransformSearchItem( SearchResultItem searchItem ) {
			var retValue = new RoSearchResultItem();

			Mapper.DynamicMap( searchItem, retValue );

			return( retValue );
		}

		public SearchResult Search( string searchText ) {
			var retValue = new SearchResult();

			try {
				var searchList = mNoiseManager.SearchProvider.Search( eSearchItemType.Everything, searchText, cMaxSearchResults );

				retValue.Items = searchList.Select( TransformSearchItem ).ToArray();
				foreach( var searchItem in retValue.Items ) {
					searchItem.CanPlay = ( searchItem.AlbumId != Constants.cDatabaseNullOid ) || ( searchItem.TrackId != Constants.cDatabaseNullOid );
				}
				retValue.Success = true;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteSearchServer:Search", ex );

				retValue.ErrorMessage = ex.Message;
			}
			return( retValue );
		}
	}
}
