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

		private readonly IUnityContainer		mContainer;
		private readonly IDatabaseManager		mDatabaseManager;
		private readonly IEventAggregator		mEvents;
		private readonly List<DbPlayHistory>	mPlayHistory;
		private readonly ILog					mLog;

		public PlayHistoryMgr( IUnityContainer container ) {
			mContainer = container;
			mDatabaseManager = mContainer.Resolve<IDatabaseManager>();
			mEvents = mContainer.Resolve<IEventAggregator>();
			mLog = mContainer.Resolve<ILog>();

			mPlayHistory = new List<DbPlayHistory>();

			UpdateHistoryList();
		}

		public void TrackPlayCompleted( PlayQueueTrack track ) {
			if( track.PercentPlayed > 0.8 ) {
				var database = mDatabaseManager.ReserveDatabase( "TrackPlayCompleted" );

				try {
					var lastPlayed = ( from DbPlayHistory history in database.Database where history.Track.MetaDataPointer == track.File.MetaDataPointer select history ).FirstOrDefault();

					if( lastPlayed != null ) {
						lastPlayed.PlayedOn = DateTime.Now;

						database.Database.Store( lastPlayed );
					}
					else {
						database.Database.Store( new DbPlayHistory( track.File ));
					}

					TrimHistoryList( database );
					UpdateHistoryList();

					mEvents.GetEvent<Events.PlayHistoryChanged>().Publish( this );
				}
				catch( Exception ex) {
					mLog.LogException( "Exception - TrackPlayCompleted:", ex );
				}
				finally {
					mDatabaseManager.FreeDatabase( database.DatabaseId );
				}
			}
		}

		private static void TrimHistoryList( IDatabase database ) {
			var historyList = from DbPlayHistory history in database.Database orderby history.PlayedOn select history;
			var historyCount = historyList.Count();
			var historyEnum = historyList.GetEnumerator();

			while(( historyCount > cMaximumHistory ) &&
				  ( historyEnum.MoveNext())) {
				database.Database.Delete( historyEnum.Current );
				historyCount--;
			}
		}

		private void UpdateHistoryList() {
			var database = mDatabaseManager.ReserveDatabase( "UpdateHistoryList" );

			try {
				mPlayHistory.Clear();
				mPlayHistory.AddRange( from DbPlayHistory history in database.Database orderby history.PlayedOn select history );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - PlayHistoryMgr: Could not update play history.", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database.DatabaseId );
			}
		}

		public IEnumerable<DbPlayHistory> PlayHistory {
			get{ return( from track in mPlayHistory select track ); }
		}

	}
}
