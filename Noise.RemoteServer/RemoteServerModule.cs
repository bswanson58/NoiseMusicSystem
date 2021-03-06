﻿using Noise.Infrastructure.RemoteHost;
using Noise.RemoteServer.Discovery;
using Noise.RemoteServer.Interfaces;
using Noise.RemoteServer.Server;
using Prism.Ioc;
using Prism.Modularity;

namespace Noise.RemoteServer {
    public class RemoteServerModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IDiscoveryService, DiscoveryService>();
            containerRegistry.RegisterSingleton<IRemoteServer, Server.RemoteServer>();
            containerRegistry.RegisterSingleton<IRemoteServiceFactory, RemoteServiceFactory>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
