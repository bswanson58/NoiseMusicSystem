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
        private readonly IUserTagManager    mTagManager;
		private readonly IPlayQueue			mPlayQueue;
		private readonly INoiseLog			mLog;
		private readonly Random				mRandom;

		public PlayCommand( IEventAggregator eventAggregator, IPlayQueue playQueue, ITrackProvider trackProvider,
                            IMetadataManager metadataManager, IUserTagManager tagManager, INoiseLog log ) {
			mEventAggregator = eventAggregator;
			mPlayQueue = playQueue;
			mTrackProvider = trackProvider;
			mMetadataManager = metadataManager;
            mTagManager = tagManager;
			mLog = log;

			mRandom = new Random( DateTime.Now.Millisecond );
		}

		public Task Play( DbArtist artist ) {
			return( Task.Run( () => {
					try {
						mPlayQueue.Add( artist );
					}
					catch( Exception exception ) {
						mLog.LogException( $"Adding artist '{artist.Name}' to playback queue", exception );
					}
				}));
		}

		public Task PlayRandomArtistTracks( DbArtist artist ) {
			return( Task.Run( () => mEventAggregator.PublishOnUIThread( new Events.PlayArtistTracksRandom( artist.DbId ))));
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
					mLog.LogException( $"Adding top tracks for artist '{artist.Name}' to playback queue", exception );
				}
			}));
		}

        public Task PlayRandomTaggedTracks( DbTag tag ) {
            return Task.Run( () => {
                try {
                    var associations = mTagManager.GetAssociations( tag.DbId ).ToList();
                    var tracksToQueue = Math.Min( associations.Count, 10 );

                    while( associations.Any() && tracksToQueue > 0 ) {
                        var association = associations.Skip( NextRandom( associations.Count )).Take( 1 ).FirstOrDefault();

                        if( association != null ) {
                            var track = mTrackProvider.GetTrack( association.ArtistId );

                            if(!mPlayQueue.IsTrackQueued( track )) {
                                mPlayQueue.Add( track );

                                tracksToQueue--;
                            }

                            associations.Remove( association );
                        }
                    }
                }
                catch( Exception exception ) {
                    mLog.LogException( $"Adding tagged tracks for tag '{tag.Name}' to playback queue", exception );
                }
            });
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
						var selectedTrack = trackList.Skip( NextRandom( trackList.Count )).Take( 1 ).FirstOrDefault();

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

					mEventAggregator.PublishOnUIThread( new Events.AlbumQueued( album ));
				}
				catch( Exception exception ) {
					mLog.LogException( $"Adding album '{album.Name}' to playback queue", exception );
				}
			}));
		}

		public Task Play( DbAlbum album, string volumeName ) {
			return( Task.Run( () => {
				try {
					mPlayQueue.Add( album, volumeName );

					mEventAggregator.PublishOnUIThread( new Events.AlbumQueued( album ));
				}
				catch( Exception exception ) {
					mLog.LogException( $"Adding volume '{volumeName}' of album '{album.Name}' to playback queue", exception );
				}
			}));
		}

		public Task Play( DbTrack track ) {
			return( Task.Run( () => {
				try {
					mPlayQueue.Add( track );

					mEventAggregator.PublishOnUIThread( new Events.TrackQueued( track ));
				}
				catch( Exception exception ) {
					mLog.LogException( $"Adding track '{track.Name}' to playback queue", exception );
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
					mLog.LogException( $"Adding stream '{stream.Name}' to playback queue", exception );
				}
			}));
		}
	}
}
