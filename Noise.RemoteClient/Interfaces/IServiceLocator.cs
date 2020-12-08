using System;
using Grpc.Core;

namespace Noise.RemoteClient.Interfaces {
    public interface IServiceLocator {
        void    StartServiceLocator();
        void    StopServiceLocator();

        IObservable<Channel>    ChannelAcquired { get; }
    }
}
