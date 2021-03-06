﻿using System;
using Noise.RemoteServer.Protocol;
using Prism.Commands;

namespace Noise.RemoteClient.Dto {
    class UiSearchItem {
        private readonly SearchItemInfo                 mSearchItem;
        private readonly Action<SearchItemInfo, bool>   mPlayAction;

        public  string                          ArtistName => mSearchItem.ArtistName;
        public  string                          AlbumName => mSearchItem.AlbumName;
        public  string                          TrackName => mSearchItem.TrackName;
        public  TimeSpan                        TrackDuration => TimeSpan.FromMilliseconds( mSearchItem.Duration );
        public  bool                            IsFavorite => mSearchItem.IsFavorite;
        public  Int32                           Rating => mSearchItem.Rating;
        public  bool                            HasRating => Rating != 0;

        public  DelegateCommand                 Play { get; }
        public  DelegateCommand                 PlayNext { get; }

        public UiSearchItem( SearchItemInfo searchItem, Action<SearchItemInfo, bool> onPlay ) {
            mSearchItem = searchItem;
            mPlayAction = onPlay;

            Play = new DelegateCommand( OnPlay );
            PlayNext = new DelegateCommand( OnPlayNext );
        }

        public string RatingSource {
            get {
                var retValue = "0_Star";

                if(( Rating > 0 ) &&
                   ( Rating < 6 )) {
                    retValue = $"{Rating:D1}_Star";
                }

                return retValue;
            }
        }

        private void OnPlay() {
            mPlayAction?.Invoke( mSearchItem, false );
        }

        private void OnPlayNext() {
            mPlayAction?.Invoke( mSearchItem, true );
        }
    }
}
