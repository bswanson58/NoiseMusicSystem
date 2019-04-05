using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Interfaces;

namespace Noise.UI.Models {
    class PlayingItemHandler : IPlayingItemHandler {
        private readonly ISelectionState    mSelectionState;
        private IEnumerable<IPlayingItem>   mList;
        private PlayingItem                 mPlayingItem;
        private IDisposable                 mPlayingTrackSubscription;

        public PlayingItemHandler( ISelectionState selectionState ) {
            mSelectionState = selectionState;

            mPlayingItem = new PlayingItem();
        }

        public void StartHandler( IEnumerable<IPlayingItem> list ) {
            mList = list;

            mPlayingTrackSubscription = mSelectionState.PlayingTrackChanged.Subscribe( OnPlayingTrackChanged );
        }

        public void StopHandler() {
            mPlayingTrackSubscription?.Dispose();
            mPlayingTrackSubscription = null;
        }

        private void OnPlayingTrackChanged( PlayingItem item ) {
            mPlayingItem = item ?? new PlayingItem();

            UpdateList();
        }

        public void UpdateList() {
            mList.ForEach( i => i.SetPlayingStatus( mPlayingItem ));
        }
    }
}
