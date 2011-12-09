using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	internal class LinkTopAlbums : IBackgroundTask {
		private IUnityContainer		mContainer;
		private IDatabaseManager	mDatabaseMgr;

		private List<long>				mArtistList;
		private IEnumerator<long>		mArtistEnum;

		public string TaskId {
			get { return( "Task_LinkTopAlbums" ); }
		}

		public bool Initialize( IUnityContainer container ) {
			mContainer = container;
			mDatabaseMgr = mContainer.Resolve<IDatabaseManager>();

			InitializeLists();

			return( true );
		}

		private void InitializeLists() {
			var database = mDatabaseMgr.ReserveDatabase();

			try {
				mArtistList = new List<long>( from DbArtist artist in database.Database.ExecuteQuery( "SELECT DbArtist" ).OfType<DbArtist>() select artist.DbId );
				mArtistEnum = mArtistList.GetEnumerator();
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "", ex );
			}
			finally {
				mDatabaseMgr.FreeDatabase( database );
			}
		}

		private long NextArtist() {
			if(!mArtistEnum.MoveNext()) {
				InitializeLists();

				mArtistEnum.MoveNext();
			}

			return( mArtistEnum.Current );
		}

		public void ExecuteTask() {
			var artistId = NextArtist();
			var database = mDatabaseMgr.ReserveDatabase();

			if( artistId != 0 ) {
				try {
					var parms = database.Database.CreateParameters();
					parms["artistId"] = artistId;
					parms["topAlbums"] = ContentType.TopAlbums;

					var topAlbums = database.Database.ExecuteScalar( "SELECT DbAssociatedItemList Where Artist = @artistId AND ContentType = @topAlbums", parms ) as DbAssociatedItemList;

					if( topAlbums != null ) {
						var	albums = database.Database.ExecuteQuery( "SELECT DbAlbum WHERE Artist = @artistId", parms ).OfType<DbAlbum>();
						var needUpdate = false;

						foreach( var topAlbum in topAlbums.Items ) {
							var topAlbumName = topAlbum.Item;
							var	dbAlbum = albums.FirstOrDefault( album => album.Name.Equals( topAlbumName, StringComparison.CurrentCultureIgnoreCase ));

							if( dbAlbum != null ) {
								if(( topAlbum.AssociatedId != dbAlbum.DbId ) ||
								   ( topAlbum.IsLinked == false )) {
									topAlbum.SetAssociatedId( dbAlbum.DbId );

									needUpdate = true;
								}
							}

							if(( dbAlbum == null ) &&
							   ( topAlbum.IsLinked )) {
								topAlbum.SetAssociatedId( Constants.cDatabaseNullOid );

								needUpdate = true;
							}
						}

						if( needUpdate ) {
							database.Store( topAlbums );

							NoiseLogger.Current.LogMessage( "Updated links to top albums" );
						}
					}
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - LinkTopAlbums:Task ", ex );
				}
				finally {
					mDatabaseMgr.FreeDatabase( database );
				}
			}
		}

		public void Shutdown() {
		}
	}
}
