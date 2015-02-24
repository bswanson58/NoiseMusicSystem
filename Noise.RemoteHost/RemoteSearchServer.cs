using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using AutoMapper;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteDto;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class RemoteSearchServer : INoiseRemoteSearch {
		private const int						cMaxSearchResults = 100;
		private const int						cMaxPlayItems = 10;

		private	readonly ISearchProvider		mSearchProvider;
		private readonly IRandomTrackSelector	mTrackSelector;
		private readonly IPlayQueue				mPlayQueue;
		private readonly INoiseLog				mLog;
		private readonly List<DbTrack>			mApprovalList; 

		public RemoteSearchServer( ISearchProvider searchProvider, IRandomTrackSelector trackSelector, IPlayQueue playQueue, INoiseLog log ) {
			mSearchProvider = searchProvider;
			mTrackSelector = trackSelector;
			mPlayQueue = playQueue;
			mLog = log;

			mApprovalList = new List<DbTrack>();
		}

		private static RoSearchResultItem TransformSearchItem( SearchResultItem searchItem ) {
			var retValue = new RoSearchResultItem();

			Mapper.DynamicMap( searchItem, retValue );

			return( retValue );
		}

		public SearchResult Search( string searchText ) {
			var retValue = new SearchResult();

			try {
				var searchList = mSearchProvider.Search( eSearchItemType.Everything, searchText, cMaxSearchResults ).ToArray();

				retValue.Items = searchList.Select( TransformSearchItem ).ToArray();
				foreach( var searchItem in retValue.Items ) {
					searchItem.CanPlay = ( searchItem.AlbumId != Constants.cDatabaseNullOid ) || ( searchItem.TrackId != Constants.cDatabaseNullOid );
				}

				if( retValue.Items.Any()) {
					mApprovalList.Clear();

					retValue.RandomTracks = mTrackSelector.SelectTracks( searchList, ApproveTrack, cMaxPlayItems ).Select( track => track.DbId ).ToArray();
				}

				retValue.Success = true;
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Search for \"{0}\"", searchText ), ex );

				retValue.ErrorMessage = ex.Message;
			}
			return( retValue );
		}

		private bool ApproveTrack( DbTrack track ) {
			bool	retValue = false;

			if((!mPlayQueue.IsTrackQueued( track )) &&
			   ( mApprovalList.FirstOrDefault( t => t.Name.Equals( track.Name, StringComparison.CurrentCultureIgnoreCase )) == null )) {
				mApprovalList.Add( track );

				retValue = true;
			}

			return( retValue );
		}
	}
}
