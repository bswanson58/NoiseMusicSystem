using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Noise.Core.Database;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	public class LinkSimilarArtists : IBackgroundTask, IRequireInitialization {
		private readonly IDatabaseManager	mDatabaseMgr;
		private DatabaseCache<DbArtist>		mArtistCache;
		private List<long>					mSimilarArtistLists;
		private IEnumerator<long>			mListEnum;

		public LinkSimilarArtists( ILifecycleManager lifecycleManager, IDatabaseManager databaseManager ) {
			mDatabaseMgr = databaseManager;

			lifecycleManager.RegisterForInitialize( this );
		}

		public string TaskId {
			get { return( "Task_LinkSimilarArtists" ); }
		}

		public void Initialize() {
			InitializeLists();
		}

		public void Shutdown() { }

		private void InitializeLists() {
			var database = mDatabaseMgr.ReserveDatabase();

			try {
				mArtistCache = new DatabaseCache<DbArtist>( from DbArtist artist in database.Database select artist );
				mSimilarArtistLists = new List<long>( from DbAssociatedItemList list in database.Database where list.ContentType == ContentType.SimilarArtists select list.DbId );
				mListEnum = mSimilarArtistLists.GetEnumerator();
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - LinkSimilarArtists:InitializeLists ", ex );
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
								if( similarArtist.AssociatedId != dbArtist.DbId ) {
									similarArtist.SetAssociatedId( dbArtist.DbId );

									needUpdate = true;
								}
							}
							else {
								if( similarArtist.IsLinked ) {
									similarArtist.SetAssociatedId( Constants.cDatabaseNullOid );

									needUpdate = true;
								}
							}
						}

						if( needUpdate ) {
							database.Store( similarArtistList );

							NoiseLogger.Current.LogMessage( "Updated links to similar artists." );
						}
					}
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - LinkSimilarArtist Task:", ex );
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
	}
}
