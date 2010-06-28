using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayHistory {
	public class PlayHistoryMgr : IPlayHistory {
		private readonly IUnityContainer	mContainter;
		private readonly IDatabaseManager	mDatabase;
		private List<DbPlayHistory>			mPlayHistory;

		public PlayHistoryMgr( IUnityContainer container ) {
			mContainter = container;
			mDatabase = mContainter.Resolve<IDatabaseManager>();

			mPlayHistory = new List<DbPlayHistory>();
		}

		public void TrackPlayCompleted( PlayQueueTrack track ) {
			mPlayHistory.Add( new DbPlayHistory( track.File ));
		}

		public IEnumerable<DbPlayHistory> PlayHistory {
			get{ return( from track in mPlayHistory select track ); }
		}

	}
}
