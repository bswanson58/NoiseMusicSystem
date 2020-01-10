using System;
using System.Collections.Generic;
using Microsoft.Practices.ObjectBuilder2;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Interfaces;

namespace Noise.UI.Models {
    class PlayingItemHandler : IPlayingItemHandler {
        private readonly ISelectionState    mSelectionState;
        private IEnumerable<IPlayingItem>   mList;
        private Func<IPlayingItem>          mItemFunc;
        private PlayingItem                 mPlayingItem;
        private IDisposable                 mPlayingTrackSubscription;

        public PlayingItemHandler( ISelectionState selectionState ) {
            mSelectionState = selectionState;

            mPlayingItem = new PlayingItem();
        }

        public void StartHandler() {
            mPlayingTrackSubscription = mSelectionState.PlayingTrackChanged.Subscribe( OnPlayingChanged );
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
            list.ForEach( i => i.SetPlayingStatus( mPlayingItem ));
        }
    }
}
