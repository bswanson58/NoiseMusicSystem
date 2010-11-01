using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayHistory {
	public class PlayHistoryMgr : IPlayHistory {
		private const int						cMaximumHistory = 100;

		private readonly IUnityContainer				mContainer;
		private readonly IDatabaseManager				mDatabaseManager;
		private readonly IEventAggregator				mEvents;
		private readonly DatabaseCache<DbPlayHistory>	mPlayHistory;
		private readonly ILog							mLog;

		public PlayHistoryMgr( IUnityContainer container ) {
			mContainer = container;
			mDatabaseManager = mContainer.Resolve<IDatabaseManager>();
			mEvents = mContainer.Resolve<IEventAggregator>();
			mLog = mContainer.Resolve<ILog>();

			var database = mDatabaseManager.ReserveDatabase();
			try {
				mPlayHistory = new DatabaseCache<DbPlayHistory>( from DbPlayHistory history in database.Database select history );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - PlayHistoryMgr:ctor ", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		public void TrackPlayCompleted( PlayQueueTrack track ) {
			if( track.PercentPlayed > 0.8 ) {
				var database = mDatabaseManager.ReserveDatabase();

				try {
					var lastPlayed = mPlayHistory.FindList( history => history.Track.MetaDataPointer == track.File.MetaDataPointer ).FirstOrDefault();

					if( lastPlayed != null ) {
						lastPlayed.PlayedOn = DateTime.Now;

						database.Store( database.ValidateOnThread( lastPlayed ));
					}
					else {
						var	newHistory = new DbPlayHistory( track.File );

						database.Insert( newHistory );
						mPlayHistory.Add( newHistory );
					}

					track.Track.PlayCount++;
					database.Store( database.ValidateOnThread( track.Track ));

					TrimHistoryList( database );

					mEvents.GetEvent<Events.PlayHistoryChanged>().Publish( this );
				}
				catch( Exception ex) {
					mLog.LogException( "Exception - TrackPlayCompleted:", ex );
				}
				finally {
					mDatabaseManager.FreeDatabase( database );
				}
			}
		}

		private void TrimHistoryList( IDatabase database ) {
			var historyList = PlayHistory;
			var historyCount = historyList.Count();
			var historyEnum = historyList.GetEnumerator();
			var deleteList = new List<DbPlayHistory>();

			while(( historyCount > cMaximumHistory ) &&
				  ( historyEnum.MoveNext())) {
				deleteList.Add( historyEnum.Current );
				database.Delete( historyEnum.Current );
				historyCount--;
			}

			foreach( var history in deleteList ) {
				mPlayHistory.List.Remove( history );
			}
		}

		public IEnumerable<DbPlayHistory> PlayHistory {
			get{ return( from history in mPlayHistory.List orderby history.PlayedOn select history ); }
		}
	}
}
