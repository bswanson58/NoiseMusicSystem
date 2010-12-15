using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Threading;
using CuttingEdge.Conditions;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	internal class ContentManager : IContentManager {
		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private readonly IDatabaseManager	mDatabaseManager;
		private readonly ILog				mLog;
		private readonly List<long>			mCurrentRequests;

		[ImportMany( typeof( IContentProvider ))]
		public IEnumerable<IContentProvider>	ContentProviders;

		public ContentManager( IUnityContainer unityContainer ) {
			mContainer = unityContainer;
			mDatabaseManager = mContainer.Resolve<IDatabaseManager>();
			mEvents =mContainer.Resolve<IEventAggregator>();
			mLog = mContainer.Resolve<ILog>();
			mCurrentRequests = new List<long>();

			var catalog = new DirectoryCatalog(  @".\" );
			var container = new CompositionContainer( catalog );

			container.ComposeExportedValue( mContainer );
			container.ComposeExportedValue( mDatabaseManager );
			container.ComposeParts( this );
		}

		public void RequestContent( DbArtist forArtist ) {
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
			var database = mDatabaseManager.ReserveDatabase();
			try {
				forArtist = database.ValidateOnThread( forArtist ) as DbArtist;
				if( forArtist != null ) {
					var selectedProviders = from IContentProvider provider in ContentProviders where provider.CanUpdateArtist select provider;
					var contentUpdated = false;

					foreach( var provider in selectedProviders ) {
						var parms = database.Database.CreateParameters();

						parms["contentType"] = provider.ContentType;
						parms["artistId"] = artistId;

						var providerContent = database.Database.ExecuteQuery( "SELECT ExpiringContent WHERE AssociatedItem = @artistId AND ContentType = @contentType", parms ).OfType<ExpiringContent>().ToList();

						if( providerContent.Count() > 0 ) {
							foreach( var content in providerContent ) {
								if( IsContentExpired( content, provider )) {
									provider.UpdateContent( database, forArtist );

									contentUpdated = true;
								}
							}
						}
						else {
							provider.UpdateContent( database, forArtist );

							contentUpdated = true;
						}
					}

					if( contentUpdated ) {
						mEvents.GetEvent<Events.ArtistContentUpdated>().Publish( forArtist );
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( String.Format( "Exception - ContentManager updating Artist: {0}", artistName ), ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
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
