using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Interfaces;
using ReusableBits.Platform;

namespace Noise.Desktop.Models {
    class PlaybackPublisher : IDisposable, IPlaybackPublisher {
        private readonly IArtistProvider        mArtistProvider;
        private readonly IAlbumProvider         mAlbumProvider;
        private readonly ITrackProvider         mTrackProvider;
        private readonly ITagManager	        mTagManager;
        private readonly IUserTagManager        mUserTagManager;
        private readonly Subject<PlaybackEvent> mPlaybackEventSubject;
        private IDisposable                     mPlayingTrackSubscription;

        public  IObservable<PlaybackEvent>      PlaybackEvents => mPlaybackEventSubject.AsObservable();

        public PlaybackPublisher( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, 
                                  ITagManager tagManager, IUserTagManager userTagManager, ISelectionState selectionState ) {
            mArtistProvider = artistProvider;
            mAlbumProvider = albumProvider;
            mTrackProvider = trackProvider;
            mTagManager = tagManager;
            mUserTagManager = userTagManager;

            mPlaybackEventSubject = new Subject<PlaybackEvent>();
            mPlayingTrackSubscription = selectionState.PlayingTrackChanged.Subscribe( OnPlayingTrackChanged );
        }

        private void OnPlayingTrackChanged( PlayingItem item ) {
            if( item != null ) {
                var artist = mArtistProvider.GetArtist( item.Artist );
                var album = mAlbumProvider.GetAlbum( item.Album );
                var track = mTrackProvider.GetTrack( item.Track );

                if(( artist != null ) &&
                   ( album != null ) &&
                   ( track != null )) {
                    var genre = mTagManager.GetGenre( artist.Genre );
                    var tags = mUserTagManager.GetAssociatedTags( track.DbId ).Select( t => t.Name );

                    var playbackEvent = new PlaybackEvent{ ArtistName = artist.Name, AlbumName = album.Name, TrackName = track.Name,
                                                           ArtistGenre = genre?.Name, TrackTags = tags.ToArray(), TrackRating = track.Rating, 
                                                           TrackLength = (uint)track.Duration.TotalSeconds, IsFavorite = track.IsFavorite, PublishedYear = track.PublishedYear };

                    mPlaybackEventSubject.OnNext( playbackEvent );
                }
            }
        }

        public void Dispose() {
            mPlayingTrackSubscription?.Dispose();
            mPlayingTrackSubscription = null;
        }
    }
}
