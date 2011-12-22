using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	public class PlayListMgr : IPlayListMgr {
		private readonly IEventAggregator	mEvents;
		private readonly IDatabaseManager	mDatabaseManager;
		private readonly IDataProvider		mDataProvider;
		private readonly ITrackProvider		mTrackProvider;

		public PlayListMgr( IDatabaseManager databaseManager, IDataProvider dataProvider, ITrackProvider trackProvider, IEventAggregator eventAggregator ) {
			mDatabaseManager = databaseManager;
			mTrackProvider = trackProvider;
			mDataProvider = dataProvider;
			mEvents = eventAggregator;
		}

		public List<DbPlayList> PlayLists {
			get {
				var retValue = new List<DbPlayList>();

				using( var list = mDataProvider.GetPlayLists()) {
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
				NoiseLogger.Current.LogException( "Exception - PlayListMgr:GetPlayList ", ex );
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
				NoiseLogger.Current.LogException( "Exception - PlayListMgr:Create(PlayQueueTrack) ", ex );
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
				NoiseLogger.Current.LogException( "Exception - PlayListMgr:Delete ", ex );
			}
			finally{
				mDatabaseManager.FreeDatabase( database );
			}
		}

		public IEnumerable<DbTrack> GetTracks( DbPlayList forList ) {
			return( forList.TrackIds.Select( trackId => mTrackProvider.GetTrack( trackId )).ToList());
		}

		private void FireListChanged() {
			mEvents.GetEvent<Events.PlayListChanged>().Publish( this );
		}
	}
}
