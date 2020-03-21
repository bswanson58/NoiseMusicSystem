using System;
using ReusableBits.Platform;

namespace Noise.Infrastructure.Interfaces {
    public interface IPlaybackPublisher {
        IObservable<PlaybackEvent>      PlaybackEvents { get; }
    }
}
