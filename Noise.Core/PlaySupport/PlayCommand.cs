using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlaySupport {
	public class PlayCommand : IPlayCommand {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IMetadataManager	mMetadataManager;
		private readonly ITrackProvider		mTrackProvider;
		private readonly IPlayQueue			mPlayQueue;
		private readonly INoiseLog			mLog;
		private readonly Random				mRandom;

		public PlayCommand( IEventAggregator eventAggregator, IPlayQueue playQueue, ITrackProvider trackProvider, IMetadataManager metadataManager, INoiseLog log ) {
			mEventAggregator = eventAggregator;
			mPlayQueue = playQueue;
			mTrackProvider = trackProvider;
			mMetadataManager = metadataManager;
			mLog = log;

			mRandom = new Random( DateTime.Now.Millisecond );
		}

		public Task Play( DbArtist artist ) {
			return( Task.Run( () => {
					try {
						mPlayQueue.Add( artist );
					}
					catch( Exception exception ) {
						mLog.LogException( string.Format( "Adding artist '{0}' to playback queue", artist.Name ), exception );
					}
				}));
		}

		public Task PlayRandomArtistTracks( DbArtist artist ) {
			return( Task.Run( () => mEventAggregator.Publish( new Events.PlayArtistTracksRandom( artist.DbId ))));
		}

		public Task PlayTopArtistTracks( DbArtist artist ) {
			return( Task.Run( () => {
				try {
					var tracks = RetrieveTopTracks( artist );

					if( tracks.Any()) {
						mPlayQueue.Add( tracks );
					}
				}
				catch( Exception exception ) {
					mLog.LogException( string.Format( "Adding top tracks for artist '{0}' to playback queue", artist.Name ), exception );
				}
			}));
		}

		private List<DbTrack> RetrieveTopTracks( DbArtist artist ) {
			var retValue = new List<DbTrack>();
			var info = mMetadataManager.GetArtistMetadata( artist.Name );
			var topTracks = info.GetMetadataArray( eMetadataType.TopTracks ).ToArray();

			if( topTracks.Any()) {
				var allTracks = mTrackProvider.GetTrackList( artist );

				foreach( var trackName in topTracks ) {
					string	name = trackName;
					var		trackList = allTracks.List.Where( t => t.Name.Equals( name, StringComparison.CurrentCultureIgnoreCase )).ToList();

					if( trackList.Any()) {
						var selectedTrack = trackList.Skip( NextRandom( trackList.Count - 1 )).Take( 1 ).FirstOrDefault();

						if( selectedTrack != null ) {
							retValue.Add( selectedTrack );
						}
					}
				}
			}

			return( retValue );
		} 

		private int NextRandom( int maxValue ) {
			return( mRandom.Next( maxValue ));
		}

		public Task Play( DbAlbum album ) {
			return( Task.Run( () => {
				try {
					mPlayQueue.Add( album );

					mEventAggregator.Publish( new Events.AlbumQueued( album ));
				}
				catch( Exception exception ) {
					mLog.LogException( string.Format( "Adding album '{0}' to playback queue", album.Name ), exception );
				}
			}));
		}

		public Task Play( DbAlbum album, string volumeName ) {
			return( Task.Run( () => {
				try {
					mPlayQueue.Add( album, volumeName );

					mEventAggregator.Publish( new Events.AlbumQueued( album ));
				}
				catch( Exception exception ) {
					mLog.LogException( string.Format( "Adding volume '{0}' of album '{1}' to playback queue", volumeName, album.Name ), exception );
				}
			}));
		}

		public Task Play( DbTrack track ) {
			return( Task.Run( () => {
				try {
					mPlayQueue.Add( track );

					mEventAggregator.Publish( new Events.TrackQueued( track ));
				}
				catch( Exception exception ) {
					mLog.LogException( string.Format( "Adding track '{0}' to playback queue", track.Name ), exception );
				}
			}));
		}

		public Task Play( IEnumerable<DbTrack> trackList ) {
			return( Task.Run( () => {
				try {
					mPlayQueue.Add( trackList );
				}
				catch( Exception exception ) {
					mLog.LogException( "Adding track list to playback queue", exception );
				}
			}));
		}

		public Task Play( DbInternetStream stream ) {
			return( Task.Run( () => {
				try {
					mPlayQueue.Add( stream );
				}
				catch( Exception exception ) {
					mLog.LogException( string.Format( "Adding stream '{0}' to playback queue", stream.Name ), exception );
				}
			}));
		}
	}
}
