using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	public class LinkSimilarArtists : IBackgroundTask {
		private IUnityContainer		mContainer;
		private IDatabaseManager	mDatabaseMgr;
		private ILog				mLog;

		private DatabaseCache<DbArtist>	mArtistCache;
		private List<long>				mSimilarArtistLists;
		private IEnumerator<long>		mListEnum;

		public string TaskId {
			get { return( "Task_LinkSimilarArtists" ); }
		}

		public bool Initialize( IUnityContainer container ) {
			mContainer = container;
			mDatabaseMgr = mContainer.Resolve<IDatabaseManager>();
			mLog = mContainer.Resolve<ILog>();

			InitializeLists();

			return( true );
		}

		private void InitializeLists() {
			var database = mDatabaseMgr.ReserveDatabase();

			try {
				mArtistCache = new DatabaseCache<DbArtist>( from DbArtist artist in database.Database select artist );
				mSimilarArtistLists = new List<long>( from DbAssociatedItemList list in database.Database where list.ContentType == ContentType.SimilarArtists select list.DbId );
				mListEnum = mSimilarArtistLists.GetEnumerator();
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - LinkSimilarArtists:InitializeLists ", ex );
			}
			finally {
				mDatabaseMgr.FreeDatabase( database );
			}
		}

		public void ExecuteTask() {
			var listId = NextList();

			if( listId != 0 ) {
				var	database = mDatabaseMgr.ReserveDatabase();

				try {
					var parms = database.Database.CreateParameters();
					parms["listId"] = listId;

					var similarArtistList = database.Database.ExecuteScalar( "SELECT DbAssociatedItemList WHERE DbId = @listId", parms ) as DbAssociatedItemList;

					if( similarArtistList != null ) {
						var	needUpdate = false;

						foreach( var similarArtist in similarArtistList.Items ) {
							var artistName = similarArtist.Item;
							var dbArtist = mArtistCache.Find( artist => String.Compare( artist.Name, artistName, true ) == 0 );

							if( dbArtist != null ) {
								similarArtist.SetAssociatedId( dbArtist.DbId );

								needUpdate = true;
							}
						}

						if( needUpdate ) {
							database.Store( similarArtistList );

							mLog.LogMessage( "Updated links to similar artists." );
						}
					}
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - LinkSimilarArtist Task:", ex );
				}
				finally {
					mDatabaseMgr.FreeDatabase( database );
				}
			}
		}

		private long NextList() {
			if(!mListEnum.MoveNext()) {
				InitializeLists();

				mListEnum.MoveNext();
			}

			return( mListEnum.Current );
		}

		public void Shutdown() {
		}
	}
}
