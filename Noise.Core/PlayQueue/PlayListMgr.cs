using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	public class PlayListMgr : IPlayListMgr {
		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private readonly IDatabaseManager	mDatabaseManager;
		private readonly INoiseManager		mNoiseManager;
		private readonly ILog				mLog;

		public PlayListMgr( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mDatabaseManager = mContainer.Resolve<IDatabaseManager>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mLog = mContainer.Resolve<ILog>();
		}

		public List<DbPlayList> PlayLists {
			get {
				var retValue = new List<DbPlayList>();

				using( var list = mNoiseManager.DataProvider.GetPlayLists()) {
					retValue.AddRange( list.List );
				}

				return( retValue );
			}
		}

		public DbPlayList GetPlayList( long playListId ) {
			DbPlayList	retValue = null;
			var			database = mDatabaseManager.ReserveDatabase();

			try {
				retValue = ( from DbPlayList playList in database.Database where playList.DbId == playListId select playList ).FirstOrDefault();
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - PlayListMgr:GetPlayList ", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DbPlayList Create( IEnumerable<PlayQueueTrack> fromList, string name, string description ) {
			var retValue = new DbPlayList { Name = name, Description = description,
											TrackIds = fromList.Select( track => track.Track.DbId ).ToArray() };
			var database = mDatabaseManager.ReserveDatabase();
			try {
				database.Insert( retValue );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - PlayListMgr:Create(PlayQueueTrack) ", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			FireListChanged();

			return( retValue );
		}

		public void Delete( DbPlayList playList ) {
			var database = mDatabaseManager.ReserveDatabase();

			try {
				database.Delete( playList );

				FireListChanged();
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - PlayListMgr:Delete ", ex );
			}
			finally{
				mDatabaseManager.FreeDatabase( database );
			}
		}

		public IEnumerable<DbTrack> GetTracks( DbPlayList forList ) {
			return( forList.TrackIds.Select( trackId => mNoiseManager.DataProvider.GetTrack( trackId )).ToList());
		}

		private void FireListChanged() {
			mEvents.GetEvent<Events.PlayListChanged>().Publish( this );
		}
	}
}
