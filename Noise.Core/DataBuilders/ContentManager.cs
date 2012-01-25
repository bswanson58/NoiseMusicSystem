using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	internal class ContentManager : IContentManager, IRequireConstruction, IHandle<Events.ArtistContentRequest> {
		private readonly ICaliburnEventAggregator	mEventAggregator;
		private readonly IArtistProvider			mArtistProvider;
		private readonly IExpiringContentProvider	mExpiringContentProvider;
		private readonly List<long>					mCurrentRequests;

		private readonly IEnumerable<IContentProvider>	mContentProviders;

		public ContentManager( ICaliburnEventAggregator caliburnEventAggregator,
							   IArtistProvider artistProvider, IExpiringContentProvider expContentProvider, IEnumerable<IContentProvider> contentProviders ) {
			mEventAggregator = caliburnEventAggregator;
			mArtistProvider = artistProvider;
			mExpiringContentProvider = expContentProvider;
			mContentProviders = contentProviders;

			mCurrentRequests = new List<long>();

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.ArtistContentRequest request ) {
			lock( mCurrentRequests ) {
				if(!mCurrentRequests.Contains( request.ArtistId )) {
					mCurrentRequests.Add( request.ArtistId );

					ThreadPool.QueueUserWorkItem( f => RequestArtistContent( request.ArtistId ));
				}
			}
		}

		private void RequestArtistContent( long artistId ) {
			var artistName = "";

			try {
				var artist = mArtistProvider.GetArtist( artistId );

				if( artist != null ) {
					var selectedProviders = from IContentProvider provider in mContentProviders where provider.CanUpdateArtist select provider;
					var contentUpdated = false;

					artistName = artist.Name;

					foreach( var provider in selectedProviders ) {
						using( var contentList = mExpiringContentProvider.GetContentList( artistId, provider.ContentType )) {

							if( contentList.List.Any()) {
								var localProvider = provider;

								if( contentList.List.Any( content => IsContentExpired( content, localProvider ))) {
									localProvider.UpdateContent( artist );

									contentUpdated = true;
								}
							}
							else {
								provider.UpdateContent( artist );

								contentUpdated = true;
							}
						}
					}

					if( contentUpdated ) {
						mEventAggregator.Publish( new Events.ArtistContentUpdated( artist.DbId ));
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( String.Format( "Exception - ContentManager updating Artist: {0}", artistName ), ex );
			}

			lock( mCurrentRequests ) {
				if( mCurrentRequests.Contains( artistId )) {
					mCurrentRequests.Remove( artistId );
				}
			}
		}

		public void RequestContent( DbAlbum forAlbum ) {
			throw new NotImplementedException();
		}

		public void RequestContent( DbTrack forTrack ) {
			throw new NotImplementedException();
		}

		private static bool IsContentExpired( ExpiringContent content, IContentProvider provider ) {
			var retValue = false;
			var expireDate = content.HarvestDate + provider.ExpirationPeriod;

			if( expireDate < DateTime.Now ) {
				retValue = true;
			}

			return( retValue );
		}
	}
}
