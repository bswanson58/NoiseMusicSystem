using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using CuttingEdge.Conditions;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	internal class ContentManager : IContentManager {
		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private readonly IDatabaseManager	mDatabase;
		private readonly ILog				mLog;

		[ImportMany( typeof( IContentProvider ))]
		public IEnumerable<IContentProvider>	ContentProviders;

		public ContentManager( IUnityContainer unityContainer ) {
			mContainer = unityContainer;
			mDatabase = mContainer.Resolve<IDatabaseManager>();
			mEvents =mContainer.Resolve<IEventAggregator>();
			mLog = mContainer.Resolve<ILog>();

			var catalog = new DirectoryCatalog(  @".\" );
			var container = new CompositionContainer( catalog );

			container.ComposeExportedValue( mContainer );
			container.ComposeParts( this );
		}

		public void RequestContent( DbArtist forArtist ) {
			Condition.Requires( forArtist ).IsNotNull();

			try {
				var	artistId = mDatabase.Database.GetUid( forArtist );
				var selectedProviders = from IContentProvider provider in ContentProviders where provider.CanUpdateArtist select provider;
				var contentUpdated = false;

				foreach( var provider in selectedProviders ) {
					var parms = mDatabase.Database.CreateParameters();

					parms["contentType"] = provider.ContentType;
					parms["artistId"] = artistId;

					var providerContent = mDatabase.Database.ExecuteQuery( "SELECT ExpiringContent WHERE AssociatedItem = @artistId AND ContentType = @contentType", parms ).Cast<ExpiringContent>();

					if( providerContent.Count() > 0 ) {
						foreach( var content in providerContent ) {
							if( IsContentExpired( content, provider )) {
								provider.UpdateContent( forArtist );

								contentUpdated = true;
							}
						}
					}
					else {
						provider.UpdateContent( forArtist );

						contentUpdated = true;
					}
				}

				if( contentUpdated ) {
					mEvents.GetEvent<Events.ArtistContentUpdated>().Publish( forArtist );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( String.Format( "ContentManager updating Artist: {0}", forArtist.Name ), ex );
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
