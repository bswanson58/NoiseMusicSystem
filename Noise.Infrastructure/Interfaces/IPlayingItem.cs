using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
    public interface IPlayingItem {
        bool    IsPlaying { get; }
        void    SetPlayingStatus( PlayingItem item );
    }

    public interface IPlayingItemHandler {
        void    StartHandler( IEnumerable<IPlayingItem> list );
        void    StopHandler();

        void    UpdateList();
    }
}
