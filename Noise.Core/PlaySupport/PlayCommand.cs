using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits;

namespace Noise.Core.PlaySupport {
	public class PlayCommand : IPlayCommand {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IPlayQueue			mPlayQueue;
		private readonly INoiseLog			mLog;
		private TaskHandler					mPlayTask;

		public PlayCommand( IEventAggregator eventAggregator, IPlayQueue playQueue, INoiseLog log ) {
			mEventAggregator = eventAggregator;
			mPlayQueue = playQueue;
			mLog = log;
		}

		internal TaskHandler PlayTask {
			get {
				if( mPlayTask == null ) {
					Execute.OnUIThread( () => mPlayTask = new TaskHandler());
				}

				return( mPlayTask );
			}

			set { mPlayTask = value; }
		}

		public Task Play( DbArtist artist ) {
			return( PlayTask.StartTask( () => mPlayQueue.Add( artist ),
										() => {},
										exception => mLog.LogException( string.Format( "Adding artist '{0}' to playback queue", artist.Name ), exception )));
		}

		public Task Play( DbAlbum album ) {
			return( PlayTask.StartTask( () => mPlayQueue.Add( album ),
										() => mEventAggregator.Publish( new Events.AlbumQueued( album )),
										exception => mLog.LogException( string.Format( "Adding album '{0}' to playback queue", album.Name ), exception )));
		}

		public Task Play( DbAlbum album, string volumeName ) {
			return( PlayTask.StartTask( () => mPlayQueue.Add( album, volumeName ),
										() => {},
										exception => mLog.LogException( string.Format( "Adding volume '{0}' of album '{1}' to playback queue", volumeName, album.Name ), exception )));
		}

		public Task Play( DbTrack track ) {
			return( PlayTask.StartTask( () => mPlayQueue.Add( track ),
										() => mEventAggregator.Publish( new Events.TrackQueued( track )),
										exception => mLog.LogException( string.Format( "Adding track '{0}' to playback queue", track.Name ), exception )));
		}

		public Task Play( IEnumerable<DbTrack> trackList ) {
			return( PlayTask.StartTask( () => mPlayQueue.Add( trackList ),
										() => {},
										exception => mLog.LogException( "Adding track list to playback queue", exception )));
		}

		public Task Play( DbInternetStream stream ) {
			return( PlayTask.StartTask( () => mPlayQueue.Add( stream ),
										() => {},
										exception => mLog.LogException( string.Format( "Adding stream '{0}' to playback queue", stream.Name ), exception )));
		}
	}
}
