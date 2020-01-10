using System;
using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
    public interface IPlayingItem {
        bool    IsPlaying { get; }
        void    SetPlayingStatus( PlayingItem item );
    }

    public interface IPlayingItemHandler {
        void    StartHandler();
        void    StartHandler( IEnumerable<IPlayingItem> list );
        void    StartHandler( Func<IPlayingItem> forItem );

        void    StopHandler();

        void    UpdateList();
        void    UpdateList( IEnumerable<IPlayingItem> list );
        void    UpdateItem();
        void    UpdateItem( IPlayingItem item );
    }
}
