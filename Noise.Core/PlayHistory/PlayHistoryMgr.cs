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
		private readonly IEventAggregator		mEvents;
		private readonly IDatabaseManager		mDatabase;
		private readonly List<DbPlayHistory>	mPlayHistory;

		public PlayHistoryMgr( IUnityContainer container ) {
			mContainer = container;
			mDatabase = mContainer.Resolve<IDatabaseManager>();
			mEvents = mContainer.Resolve<IEventAggregator>();

			mPlayHistory = new List<DbPlayHistory>();
			UpdateHistoryList();
		}

		public void TrackPlayCompleted( PlayQueueTrack track ) {
			if( track.PercentPlayed > 0.8 ) {
				var lastPlayed = ( from DbPlayHistory history in mDatabase.Database where history.Track.MetaDataPointer == track.File.MetaDataPointer select history ).FirstOrDefault();

				if( lastPlayed != null ) {
					lastPlayed.PlayedOn = DateTime.Now;

					mDatabase.Database.Store( lastPlayed );
				}
				else {
					mDatabase.Database.Store( new DbPlayHistory( track.File ));
				}

				TrimHistoryList();
				UpdateHistoryList();

				mEvents.GetEvent<Events.PlayHistoryChanged>().Publish( this );
			}
		}

		private void TrimHistoryList() {
			var historyList = from DbPlayHistory history in mDatabase.Database orderby history.PlayedOn select history;
			var historyCount = historyList.Count();
			var historyEnum = historyList.GetEnumerator();

			while(( historyCount > cMaximumHistory ) &&
				  ( historyEnum.MoveNext())) {
				mDatabase.Database.Delete( historyEnum.Current );
				historyCount--;
			}
		}

		private void UpdateHistoryList() {
			mPlayHistory.Clear();
			mPlayHistory.AddRange( from DbPlayHistory history in mDatabase.Database orderby history.PlayedOn select history );
		}

		public IEnumerable<DbPlayHistory> PlayHistory {
			get{ return( from track in mPlayHistory select track ); }
		}

	}
}
