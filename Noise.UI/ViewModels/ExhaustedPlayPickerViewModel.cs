using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits;

namespace Noise.UI.ViewModels {
    class ExhaustedPlayPickerViewModel : IDialogAware {
        private readonly IEventAggregator                       mEventAggregator;
        private readonly IArtistProvider                        mArtistProvider;
        private readonly IAlbumProvider                         mAlbumProvider;
        private readonly ITrackProvider                         mTrackProvider;
        private readonly IUserTagManager                        mTagManager;
        private readonly IPlayQueue                             mPlayQueue;
        private readonly IExhaustedStrategyPlayManager          mStrategyManager;
        private UiArtistAlbumTrack                              mSelectedTrack;
        private TaskHandler<IEnumerable<UiArtistAlbumTrack>>    mTrackLoader;

        public  ObservableCollection<UiArtistAlbumTrack>        SuggestedTracks { get; }

        public  string                                          Title { get; }
        public  DelegateCommand                                 Close { get; }

        public  event Action<IDialogResult>                     RequestClose;

        public ExhaustedPlayPickerViewModel( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, IUserTagManager tagManager,
                                             IPlayQueue playQueue, IExhaustedStrategyPlayManager strategyManager, IEventAggregator eventAggregator ) {
            mArtistProvider = artistProvider;
            mAlbumProvider = albumProvider;
            mTrackProvider = trackProvider;
            mPlayQueue = playQueue;
            mTagManager = tagManager;
            mStrategyManager = strategyManager;
            mEventAggregator = eventAggregator;

            SuggestedTracks = new ObservableCollection<UiArtistAlbumTrack>();

            Title = "Suggested Tracks Selector";

            Close = new DelegateCommand( OnClose );
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            mStrategyManager.StrategySpecification = mPlayQueue.ExhaustedPlayStrategy;

            LoadTracks();
        }

        public UiArtistAlbumTrack SelectedTrack {
            get => mSelectedTrack;
            set {
                mSelectedTrack = value;

                if( mSelectedTrack != null ) {
                    mEventAggregator.PublishOnUIThread( new Events.AlbumFocusRequested( mSelectedTrack.Artist.DbId, mSelectedTrack.Album.DbId, true ));
                }
            }
        }

        internal TaskHandler<IEnumerable<UiArtistAlbumTrack>>  TracksRetrievalTaskHandler {
            get {
                if( mTrackLoader == null ) {
                    Execute.OnUIThread( () => mTrackLoader = new TaskHandler<IEnumerable<UiArtistAlbumTrack>> ());
                }

                return( mTrackLoader );
            }
            set => mTrackLoader = value;
        }

        private void LoadTracks() {
            SuggestedTracks.Clear();
            SelectedTrack = null;

            TracksRetrievalTaskHandler.StartTask( 
                () => {
                    var retValue = new List<UiArtistAlbumTrack>();

                    foreach( var track in mStrategyManager.SelectTracks( mPlayQueue, 50 )) {
                        var artist = mArtistProvider.GetArtist( track.Artist );
                        var album = mAlbumProvider.GetAlbum( track.Album );

                        if(( artist != null ) &&
                           ( album != null )) {
                            retValue.Add( new UiArtistAlbumTrack( artist, album, track, OnTrackPlay ));
                        }
                    }

                    retValue.ForEach( SetTrackTags );

                    return retValue;
                }, tracks => {
                    SuggestedTracks.AddRange( tracks );
                }, ex => { } );
        }

        private void SetTrackTags( UiArtistAlbumTrack track ) {
            track?.SetTags( from tag in mTagManager.GetAssociatedTags( track.Track.DbId ) orderby tag.Name select tag.Name );
        }

        private void OnTrackPlay( DbTrack targetTrack ) {
            var previousTrack = targetTrack;
            var previousTracks = new List<DbTrack>();
            var playList = new List<DbTrack>();
            var albumTracks = new List<DbTrack>();

            using( var tracks = mTrackProvider.GetTrackList( targetTrack.Album )) {
                albumTracks.AddRange( from DbTrack track in tracks.List orderby track.VolumeName, track.TrackNumber select track );
            }

            while(( previousTrack != null ) && 
                 (( previousTrack.PlayAdjacentStrategy == ePlayAdjacentStrategy.PlayPrevious ) ||
                  ( previousTrack.PlayAdjacentStrategy == ePlayAdjacentStrategy.PlayNextPrevious ))) {
                previousTrack = albumTracks.TakeWhile( t => !t.DbId.Equals( previousTrack.DbId )).LastOrDefault();

                if( previousTrack != null ) {
                    previousTracks.Insert( 0, previousTrack );
                }
            }
            previousTracks.ForEach( t => playList.Add( t ));
            
            playList.Add( targetTrack );

            while(( targetTrack != null ) &&
                 (( targetTrack.PlayAdjacentStrategy == ePlayAdjacentStrategy.PlayNextPrevious ) ||
                  ( targetTrack.PlayAdjacentStrategy == ePlayAdjacentStrategy.PlayNext ))) {
                targetTrack = albumTracks.SkipWhile( t => !t.DbId.Equals( targetTrack.DbId )).Skip( 1 ).FirstOrDefault();

                if( targetTrack != null ) {
                    playList.Add( targetTrack );
                }
            }

            mPlayQueue.Add( playList );
        }

        private void OnClose() {
            RaiseRequestClose( new DialogResult( ButtonResult.OK ));
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
