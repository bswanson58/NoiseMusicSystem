﻿using System;
using System.Reactive.Subjects;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Models {
    class ClientState : IClientState {
        private readonly BehaviorSubject<ArtistInfo>        mArtistSubject;
        private readonly BehaviorSubject<AlbumInfo>         mAlbumSubject;
        private readonly BehaviorSubject<SuggestionState>   mSuggestionSubject;
        private ArtistInfo                                  mCurrentArtist;
        private AlbumInfo                                   mCurrentAlbum;

        public  IObservable<ArtistInfo>                     CurrentArtist => mArtistSubject;
        public  IObservable<AlbumInfo>                      CurrentAlbum => mAlbumSubject;
        public  IObservable<SuggestionState>                CurrentSuggestion => mSuggestionSubject;

        public ClientState() {
            mArtistSubject = new BehaviorSubject<ArtistInfo>( null );
            mAlbumSubject = new BehaviorSubject<AlbumInfo>( null );
            mSuggestionSubject = new BehaviorSubject<SuggestionState>( null );
        }

        public void SetCurrentArtist( ArtistInfo artist ) {
            mCurrentArtist = artist;

            mArtistSubject.OnNext( mCurrentArtist );
        }

        public void SetCurrentAlbum( AlbumInfo album ) {
            mCurrentAlbum = album;

            mAlbumSubject.OnNext( mCurrentAlbum );
        }

        public void SetSuggestionState( UiQueuedTrack forTrack ) {
            if( forTrack != null ) {
                mSuggestionSubject.OnNext( new SuggestionState( forTrack ));
            }
        }
    }
}