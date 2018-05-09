using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayHistory {
	public class PlayHistoryMgr : IPlayHistory,
								  IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private const int						cMaximumHistory = 100;

		private readonly IEventAggregator		mEventAggregator;
		private readonly INoiseLog				mLog;
		private readonly IPlayHistoryProvider	mPlayHistoryProvider;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private DatabaseCache<DbPlayHistory>	mPlayHistory;

		public PlayHistoryMgr( IEventAggregator eventAggregator, INoiseLog log, IPlayHistoryProvider playHistoryProvider,
							   IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mPlayHistoryProvider = playHistoryProvider;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mPlayHistory = new DatabaseCache<DbPlayHistory>( null );

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.DatabaseOpened args ) {
			try {
				using( var historyList = mPlayHistoryProvider.GetPlayHistoryList()) {
					mPlayHistory = new DatabaseCache<DbPlayHistory>( historyList.List );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Building play history cache", ex );
			}
		}

		public void Handle( Events.DatabaseClosing args ) {
			mPlayHistory.Clear();
		}

		public void Shutdown() { }

		public Task TrackPlayCompleted( PlayQueueTrack track ) {
			var retValue = new Task( () => UpdatePlayHistory( track ));

			retValue.Start();

			return( retValue );
		}

		private void UpdatePlayHistory( PlayQueueTrack track ) {
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
						trackUpdate.Item.UpdateLastPlayed();
						trackUpdate.Update();
					}

					if( track.Artist != null ) {
						using( var artistUpdate = mArtistProvider.GetArtistForUpdate( track.Artist.DbId )) {
							artistUpdate.Item.UpdateLastPlayed();
							artistUpdate.Update();
						}
					}

					if( track.Album != null ) {
						using( var albumUpdate = mAlbumProvider.GetAlbumForUpdate( track.Album.DbId )) {
							albumUpdate.Item.UpdateLastPlayed();
							albumUpdate.Update();
						}
					}

					GlobalCommands.UpdatePlayCount.Execute( new UpdatePlayCountCommandArgs( track.Track.DbId ));

					TrimHistoryList();

					mEventAggregator.PublishOnUIThread( new Events.PlayHistoryChanged( this ));

					if( track.Artist != null ) {
						using( var updater = mArtistProvider.GetArtistForUpdate( track.Artist.DbId )) {
							if( updater.Item != null ) {
								updater.Item.UpdateLastPlayed();

								updater.Update();
							}
						}

						mEventAggregator.PublishOnUIThread( new Events.ArtistPlayed( track.Artist.DbId ));
					}
				}
				catch( Exception ex) {
					mLog.LogException( "Recording play history", ex );
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
