using System;
using DynamicData;
using Noise.RemoteClient.Dto;

namespace Noise.RemoteClient.Interfaces {
    interface IQueueListener {
        IObservableList<UiQueuedTrack>      QueueList { get; }
        IObservable<UiQueuedTrack>          NextPlayingTrack { get; }
        IObservable<PlayingState>           CurrentlyPlaying { get; }

        TimeSpan    TotalPlayingTime { get; }
        TimeSpan    RemainingPlayTime { get; }
    }
}
