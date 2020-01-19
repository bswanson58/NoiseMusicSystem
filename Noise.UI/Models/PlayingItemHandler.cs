using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Interfaces;

namespace Noise.UI.Models {
    class PlayingItemHandler : IPlayingItemHandler {
        private readonly ISelectionState    mSelectionState;
        private IEnumerable<IPlayingItem>   mList;
        private Func<IPlayingItem>          mItemFunc;
        private Action<IPlayingItem>        mOnPlayAction;
        private PlayingItem                 mPlayingItem;
        private IDisposable                 mPlayingTrackSubscription;

        public PlayingItemHandler( ISelectionState selectionState ) {
            mSelectionState = selectionState;

            mPlayingItem = new PlayingItem();
        }

        public void StartHandler() {
            mPlayingTrackSubscription = mSelectionState.PlayingTrackChanged.Subscribe( OnPlayingChanged );
        }

        public void StartHandler( IEnumerable<IPlayingItem> list, Action<IPlayingItem> onPlayAction ) {
            StartHandler( list );

            mOnPlayAction = onPlayAction;
        }

        public void StartHandler( IEnumerable<IPlayingItem> list ) {
            mList = list;

            mPlayingTrackSubscription = mSelectionState.PlayingTrackChanged.Subscribe( OnPlayingListChanged );
        }

        public void StartHandler( Func<IPlayingItem> forItem ) {
            mItemFunc = forItem;

            mPlayingTrackSubscription = mSelectionState.PlayingTrackChanged.Subscribe( OnPlayingItemChanged );
        }

        public void StopHandler() {
            mPlayingTrackSubscription?.Dispose();
            mPlayingTrackSubscription = null;
        }

        private void OnPlayingChanged( PlayingItem item ) {
            mPlayingItem = item ?? new PlayingItem();
        }

        private void OnPlayingItemChanged( PlayingItem item ) {
            OnPlayingChanged( item );

            UpdateItem();
        }

        public void UpdateItem() {
            var target = mItemFunc?.Invoke();

            target?.SetPlayingStatus( mPlayingItem );
        }

        public void UpdateItem( IPlayingItem item ) {
            item.SetPlayingStatus( mPlayingItem );
        }

        private void OnPlayingListChanged( PlayingItem item ) {
            OnPlayingChanged( item );

            UpdateList();
        }

        public void UpdateList() {
            UpdateList( mList );
        }

        public void UpdateList( IEnumerable<IPlayingItem> list ) {
            var playList = list.ToList();

            playList.ForEach( i => i.SetPlayingStatus( mPlayingItem ));

            if( mOnPlayAction != null ) {
                var item = playList.FirstOrDefault( i => i.IsPlaying );

                if( item != null ) {
                    mOnPlayAction.Invoke( item );
                }
            }
        }
    }
}
