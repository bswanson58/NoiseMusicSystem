namespace Noise.RemoteClient.Interfaces {
    interface IClientManager {
        void    OnApplicationStarting();
        void    OnApplicationStopping();

        void    StartClientManager();
        void    StopClientManager();
    }
}
