using System;
using System.Linq;
using DynamicData.Binding;
using Noise.RemoteClient.Dialogs;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Xamarin.Forms.Internals;

namespace Noise.RemoteClient.ViewModels {
    class TrackListViewModel : BindableBase, IDisposable {
        private readonly ITrackProvider         mTrackProvider;
        private readonly IClientState           mClientState;
        private readonly IQueueListener         mQueueListener;
        private readonly IQueuePlayProvider     mPlayProvider;
        private readonly IDialogService         mDialogService;
        private PlayingState                    mPlayingState;
        private IDisposable                     mPlayingStateSubscription;
        private AlbumInfo                       mCurrentAlbum;
        private ObservableCollectionExtended<UiTrack>   mTrackList;

        public  DelegateCommand<UiTrack>        EditTrackRatings { get; }
        public  DelegateCommand<UiTrack>        EditTrackTags { get; }

        public  string                          ArtistName { get; private set; }
        public  string                          AlbumName { get; private set; }

        public TrackListViewModel( ITrackProvider trackProvider, IQueuePlayProvider queuePlayProvider, IClientState clientState, IQueueListener queueListener,
                                   IDialogService dialogService ) {
            mTrackProvider = trackProvider;
            mPlayProvider = queuePlayProvider;
            mQueueListener = queueListener;
            mClientState = clientState;
            mDialogService = dialogService;

            EditTrackRatings = new DelegateCommand<UiTrack>( OnEditTrackRatings );
            EditTrackTags = new DelegateCommand<UiTrack>( OnEditTrackTags );
        }

        public ObservableCollectionExtended<UiTrack> DisplayList {
            get {
                if( mTrackList == null ) {
                    mTrackList = new ObservableCollectionExtended<UiTrack>();

                    Initialize();
                }

                return mTrackList;
            }
        }

        private void Initialize() {
            mPlayingStateSubscription = mQueueListener.CurrentlyPlaying.Subscribe( OnPlayingState );

            mCurrentAlbum = mClientState.CurrentAlbum;

            if( mCurrentAlbum != null ) {
                ArtistName = mCurrentAlbum.ArtistName;
                AlbumName = mCurrentAlbum.AlbumName;

                RaisePropertyChanged( nameof( ArtistName ));
                RaisePropertyChanged( nameof( AlbumName ));
            }

            LoadAlbums();
        }

        private void OnPlayingState( PlayingState state ) {
            mPlayingState = state;

            DisplayList.ForEach( t => t.SetIsPlaying( mPlayingState ));
        }

        private async void LoadAlbums() {
            if( mCurrentAlbum != null ) {
                var list = await mTrackProvider.GetTrackList( mCurrentAlbum.ArtistId, mCurrentAlbum.AlbumId );

                if( list?.Success == true ) {
                    mTrackList.AddRange( from track in list.TrackList orderby track.VolumeName, track.TrackNumber select new UiTrack( track, OnPlay ));

                    mTrackList.ForEach( t => t.SetIsPlaying( mPlayingState ));
                }
            }
        }

        private void OnPlay( UiTrack track, bool playNext ) {
            mPlayProvider.Queue( track.Track, playNext );
        }

        private void OnEditTrackRatings( UiTrack forTrack ) {
            var parameters = new DialogParameters {{ EditTrackRatingsViewModel.cTrackParameter, forTrack?.Track }};

            mDialogService.ShowDialog( nameof( EditTrackRatingsView ), parameters, async result => {
                var accepted = result.Parameters.GetValue<bool>( EditTrackRatingsViewModel.cDialogAccepted );

                if( accepted ) {
                    var track = parameters.GetValue<TrackInfo>( EditTrackRatingsViewModel.cTrackParameter );

                    if( track != null ) {
                        var response = await mTrackProvider.UpdateTrackRatings( track );

                        if( response.Success ) {
                            var displayTrack = mTrackList.FirstOrDefault( t => t.Track.TrackId.Equals( track.TrackId ));
                            displayTrack?.UpdateTrack( track );
                        }
                    }
                }
            });
        }

        private void OnEditTrackTags( UiTrack forTrack ) {
            var parameters = new DialogParameters {{ EditTrackTagsViewModel.cTrackParameter, forTrack?.Track }};

            mDialogService.ShowDialog( nameof( EditTrackTagsView ), parameters, async result => {
                var accepted = result.Parameters.GetValue<bool>( EditTrackTagsViewModel.cDialogAccepted );

                if( accepted ) {
                    var track = parameters.GetValue<TrackInfo>( EditTrackTagsViewModel.cTrackParameter );

                    if( track != null ) {
                        var response = await mTrackProvider.UpdateTrackTags( track );

                        if( response.Success ) {
                            var displayTrack = mTrackList.FirstOrDefault( t => t.Track.TrackId.Equals( track.TrackId ));
                            displayTrack?.UpdateTrack( track );
                        }
                    }
                }
            });
        }

        public void Dispose() {
            mPlayingStateSubscription?.Dispose();
            mPlayingStateSubscription = null;
        }
    }
}
