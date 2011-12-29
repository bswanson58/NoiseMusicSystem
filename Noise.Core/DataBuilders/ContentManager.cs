﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CuttingEdge.Conditions;
using Microsoft.Practices.Prism.Events;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	internal class ContentManager : IContentManager, IRequireConstruction {
		private readonly IEventAggregator			mEvents;
		private readonly IExpiringContentProvider	mExpiringContentProvider;
		private readonly List<long>					mCurrentRequests;

		private readonly IEnumerable<IContentProvider>	mContentProviders;

		public ContentManager( IEventAggregator eventAggregator, IExpiringContentProvider expContentProvider, IEnumerable<IContentProvider> contentProviders ) {
			mEvents = eventAggregator;
			mExpiringContentProvider = expContentProvider;
			mContentProviders = contentProviders;

			mCurrentRequests = new List<long>();
			mEvents.GetEvent<Events.ArtistContentRequested>().Subscribe( OnArtistContentRequested );
		}

		private void OnArtistContentRequested( DbArtist forArtist ) {
			lock( mCurrentRequests ) {
				if(!mCurrentRequests.Contains( forArtist.DbId )) {
					mCurrentRequests.Add( forArtist.DbId );

					ThreadPool.QueueUserWorkItem( f => RequestArtistContent( forArtist ));
				}
			}
		}

		private void RequestArtistContent( DbArtist forArtist ) {
			Condition.Requires( forArtist ).IsNotNull();

			var artistId = forArtist.DbId;
			var artistName = forArtist.Name;

			try {
				var selectedProviders = from IContentProvider provider in mContentProviders where provider.CanUpdateArtist select provider;
				var contentUpdated = false;

				foreach( var provider in selectedProviders ) {
					using( var contentList = mExpiringContentProvider.GetContentList( forArtist.DbId, provider.ContentType )) {

						if( contentList.List.Count() > 0 ) {
							var localProvider = provider;

							if( contentList.List.Any( content => IsContentExpired( content, localProvider ))) {
								localProvider.UpdateContent( forArtist );

								contentUpdated = true;
							}
						}
						else {
							provider.UpdateContent( forArtist );

							contentUpdated = true;
						}
					}
				}

				if( contentUpdated ) {
					mEvents.GetEvent<Events.ArtistContentUpdated>().Publish( forArtist );
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
