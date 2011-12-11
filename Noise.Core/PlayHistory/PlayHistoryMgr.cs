using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayHistory {
	public class PlayHistoryMgr : IPlayHistory {
		private const int						cMaximumHistory = 100;

		private readonly IDatabaseManager				mDatabaseManager;
		private readonly IEventAggregator				mEvents;
		private readonly DatabaseCache<DbPlayHistory>	mPlayHistory;

		public PlayHistoryMgr( IEventAggregator eventAggregator, IDatabaseManager databaseManager ) {
			mDatabaseManager = databaseManager;
			mEvents = eventAggregator;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				mPlayHistory = new DatabaseCache<DbPlayHistory>( from DbPlayHistory history in database.Database select history );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - PlayHistoryMgr:ctor ", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		public void TrackPlayCompleted( PlayQueueTrack track ) {
			if( track.PercentPlayed > 0.8 ) {
				var database = mDatabaseManager.ReserveDatabase();

				try {
					var lastPlayed = mPlayHistory.FindList( history => history.StorageFileId == track.File.DbId ).FirstOrDefault();

					if( lastPlayed != null ) {
						lastPlayed.PlayedOnTicks = DateTime.Now.Ticks;

						database.Store( database.ValidateOnThread( lastPlayed ));
					}
					else {
						var	newHistory = new DbPlayHistory( track.File );

						database.Insert( newHistory );
						mPlayHistory.Add( newHistory );
					}

					var dbTrack = database.ValidateOnThread( track.Track ) as DbTrack;
					if( dbTrack != null ) {
						dbTrack.PlayCount++;

						database.Store( dbTrack );
						GlobalCommands.UpdatePlayCount.Execute( new UpdatePlayCountCommandArgs( track.Track.DbId ));
					}

					TrimHistoryList( database );

					mEvents.GetEvent<Events.PlayHistoryChanged>().Publish( this );
				}
				catch( Exception ex) {
					NoiseLogger.Current.LogException( "Exception - TrackPlayCompleted:", ex );
				}
				finally {
					mDatabaseManager.FreeDatabase( database );
				}
			}
		}

		private void TrimHistoryList( IDatabase database ) {
			var historyList = from DbPlayHistory history in PlayHistory orderby history.PlayedOn ascending select history;
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
			get{ return( mPlayHistory.List ); }
		}
	}
}
