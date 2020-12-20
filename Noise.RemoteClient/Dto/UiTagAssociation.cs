﻿using System;
using System.Diagnostics;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;

namespace Noise.RemoteClient.Dto {
    [DebuggerDisplay("Track = {" + nameof(TrackName) + "}")]
    class UiTagAssociation : BindableBase {
        private readonly TagAssociationInfo         mTag;
        private readonly Action<TagAssociationInfo> mPlayAction;
        private bool                                mIsPlaying;

        public  string                              ArtistName => mTag.ArtistName;
        public  string                              AlbumName => mTag.AlbumName;
        public  string                              TrackName => mTag.TrackName;
        public  TimeSpan                            TrackDuration => TimeSpan.FromMilliseconds( mTag.Duration );
        public  bool                                IsFavorite => mTag.IsFavorite;
        public  Int32                               Rating => mTag.Rating;
        public  bool                                HasRating => Rating != 0;

        public  DelegateCommand                     Play {  get; }

        public UiTagAssociation( TagAssociationInfo tag, Action<TagAssociationInfo> onPlay, PlayingState state  ) :
            this( tag, onPlay ) { 
            SetIsPlaying( state );
        }

        public UiTagAssociation( TagAssociationInfo tag, Action<TagAssociationInfo> onPlay  ) {
            mTag = tag;
            mPlayAction = onPlay;

            Play = new DelegateCommand( OnPlay );
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

        public bool IsPlaying {
            get => mIsPlaying;
            set => SetProperty( ref mIsPlaying, value );
        }

        public void SetIsPlaying( PlayingState state ) {
            IsPlaying = mTag.TrackId.Equals( state?.TrackId );
        }

        private void OnPlay() {
            mPlayAction?.Invoke( mTag );
        }
    }
}
