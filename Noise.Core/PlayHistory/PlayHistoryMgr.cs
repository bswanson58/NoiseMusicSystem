using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Core.Database;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayHistory {
	public class PlayHistoryMgr : IPlayHistory, IRequireInitialization {
		private const int						cMaximumHistory = 100;

		private readonly IEventAggregator		mEventAggregator;
		private readonly IPlayHistoryProvider	mPlayHistoryProvider;
		private readonly ITrackProvider			mTrackProvider;
		private DatabaseCache<DbPlayHistory>	mPlayHistory;

		public PlayHistoryMgr( ILifecycleManager lifecycleManager, IEventAggregator eventAggregator,
							   IPlayHistoryProvider playHistoryProvider, ITrackProvider trackProvider ) {
			mEventAggregator = eventAggregator;
			mPlayHistoryProvider = playHistoryProvider;
			mTrackProvider = trackProvider;
			mPlayHistory = new DatabaseCache<DbPlayHistory>( null );

			lifecycleManager.RegisterForInitialize( this );

			NoiseLogger.Current.LogInfo( "PlayHistory created" );
		}

		public void Initialize() {
			try {
				using( var historyList = mPlayHistoryProvider.GetPlayHistoryList()) {
					mPlayHistory = new DatabaseCache<DbPlayHistory>( historyList.List );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - PlayHistoryMgr:ctor ", ex );
			}
		}

		public void Shutdown() { }

		public void TrackPlayCompleted( PlayQueueTrack track ) {
			if( track.PercentPlayed > 0.8 ) {
				try {
					var lastPlayed = mPlayHistory.FindList( history => history.StorageFileId == track.File.DbId ).FirstOrDefault();

					if( lastPlayed != null ) {
						lastPlayed.PlayedOnTicks = DateTime.Now.Ticks;

						using( var historyUpdate = mPlayHistoryProvider.GetPlayHistoryForUpdate( lastPlayed.DbId )) {
							if( historyUpdate.Item != null ) {
								historyUpdate.Item.PlayedOnTicks = DateTime.Now.Ticks;
								historyUpdate.Update();
							}
						}
					}
					else {
						var	newHistory = new DbPlayHistory( track.File );

						mPlayHistory.Add( newHistory );
						mPlayHistoryProvider.AddPlayHistory( newHistory );
					}

					using( var trackUpdate = mTrackProvider.GetTrackForUpdate( track.Track.DbId )) {
						trackUpdate.Item.PlayCount++;
						trackUpdate.Update();
					}

					GlobalCommands.UpdatePlayCount.Execute( new UpdatePlayCountCommandArgs( track.Track.DbId ));

					TrimHistoryList();

					mEventAggregator.Publish( new Events.PlayHistoryChanged( this ));
				}
				catch( Exception ex) {
					NoiseLogger.Current.LogException( "Exception - TrackPlayCompleted:", ex );
				}
			}
		}

		private void TrimHistoryList() {
			var historyList = from DbPlayHistory history in PlayHistory orderby history.PlayedOn ascending select history;
			var historyCount = historyList.Count();
			var historyEnum = historyList.GetEnumerator();
			var deleteList = new List<DbPlayHistory>();

			while(( historyCount > cMaximumHistory ) &&
				  ( historyEnum.MoveNext())) {
				deleteList.Add( historyEnum.Current );
				mPlayHistoryProvider.DeletePlayHistory( historyEnum.Current );
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
