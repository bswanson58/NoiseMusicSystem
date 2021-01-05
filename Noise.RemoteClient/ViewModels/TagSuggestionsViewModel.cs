﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Xamarin.Forms.Internals;

namespace Noise.RemoteClient.ViewModels {
    class TagSuggestionsViewModel : ListBase<UiTrack> {
        private readonly ITrackProvider                 mTrackProvider;
        private SuggestionState                         mSuggestionState;
        private PlayingState                            mPlayingState;
        private IDisposable                             mClientStateSubscription;
        private IDisposable                             mPlayingStateSubscription;

        public TagSuggestionsViewModel( ITrackProvider trackProvider, IQueuePlayProvider playProvider, IClientState clientState, IQueueListener queueListener,
                                        IHostInformationProvider hostInformationProvider ) :
            base( playProvider, hostInformationProvider ) {
            mTrackProvider = trackProvider;

            InitializeLibrarySubscription();
            mPlayingStateSubscription = queueListener.CurrentlyPlaying.Subscribe( OnPlaying );
            mClientStateSubscription = clientState.CurrentSuggestion.Subscribe( OnSuggestion );
        }

        private void OnSuggestion( SuggestionState state ) {
            mSuggestionState = state;

            LoadList();
        }

        private void OnPlaying( PlayingState state ) {
            mPlayingState = state;

            DisplayList.ForEach( t => t.SetIsPlaying( mPlayingState ));
        }

        protected override async Task<IEnumerable<UiTrack>> RetrieveList() {
            IEnumerable<UiTrack> retValue = new List<UiTrack>();

            if( mSuggestionState != null ) {
                var list = await mTrackProvider.GetTaggedTracks( mSuggestionState.TrackId );

                if( list?.Success == true ) {
                    retValue = from track in list.TrackList orderby track.TrackName, track.ArtistName, track.AlbumName 
                                select new UiTrack( track, OnPlay, mPlayingState );
                }
            }

            return retValue;
        }

        public override void Dispose() {
            mClientStateSubscription?.Dispose();
            mClientStateSubscription = null;

            mPlayingStateSubscription?.Dispose();
            mPlayingStateSubscription = null;

            base.Dispose();
        }
    }
}
