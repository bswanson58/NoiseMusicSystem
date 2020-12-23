namespace Noise.RemoteClient.Interfaces {
    interface IQueuedItemNotifier {
        void    StartNotifications();
        void    StopNotifications();
    }
}
