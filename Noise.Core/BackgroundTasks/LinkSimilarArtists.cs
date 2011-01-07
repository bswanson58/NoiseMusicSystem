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

		private DatabaseCache<DbArtist>				mArtistCache;
		private List<DbAssociatedItemList>			mSimilarArtistLists;
		private IEnumerator<DbAssociatedItemList>	mListEnum;

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
				mSimilarArtistLists = new List<DbAssociatedItemList>( from DbAssociatedItemList list in database.Database where list.ContentType == ContentType.SimilarArtists select list );
				mListEnum = mSimilarArtistLists.GetEnumerator();
			}
			catch( Exception ex ) {
				mLog.LogException( "", ex );
			}
			finally {
				mDatabaseMgr.FreeDatabase( database );
			}
		}

		public void ExecuteTask() {
			var similarArtistList = NextList();

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
					var database = mDatabaseMgr.ReserveDatabase();

					try {
						database.Store( similarArtistList );
					}
					catch( Exception ex ) {
						mLog.LogException( "", ex );
					}
					finally {
						mDatabaseMgr.FreeDatabase( database );
					}
				}
			}
		}

		private DbAssociatedItemList NextList() {
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
