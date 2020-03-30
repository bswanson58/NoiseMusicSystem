using System;
using System.Collections.Generic;
using MilkBottle.Entities;
using ReusableBits.Platform;

namespace MilkBottle.Interfaces {
    interface IIpcManager : IDisposable {
        IObservable<IEnumerable<ActiveCompanionApp>>    CompanionAppsUpdate { get; }
        IObservable<PlaybackEvent>                      OnPlaybackEvent { get; }
        IObservable<bool>                               OnActivationRequest { get; }

        void    ActivateApplication( string applicationName );
    }
}
