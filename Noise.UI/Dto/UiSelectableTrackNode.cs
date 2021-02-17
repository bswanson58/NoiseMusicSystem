using System;
using Noise.Infrastructure.Dto;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
    class UiSelectableTrackNode : PropertyChangeBase {
        private readonly Action<UiSelectableTrackNode>   mOnSelection;
        private bool                mSelected;
        private bool                mWillSelect;

        public	DbAlbum				Album { get; }
        public	UiTrack				Track { get; }
        public	string				TrackName => Track.Name;
        public	string				AlbumName => Album.Name;

        public UiSelectableTrackNode( UiTrack track, DbAlbum album, Action<UiSelectableTrackNode> onSelection ) {
            Track = track;
            Album = album;

            mOnSelection = onSelection;
        }

        public bool Selected {
            get => mSelected;
            set {
                mSelected = value;

                mOnSelection?.Invoke( this );
                RaisePropertyChanged( () => Selected );
            }
        }

        public bool WillSelect {
            get => mWillSelect;
            set {
                mWillSelect = value;

                RaisePropertyChanged( () => WillSelect );
            }
        }
    }
}
