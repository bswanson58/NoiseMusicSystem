using System;
using System.Threading.Tasks;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Views;
using Rg.Plugins.Popup.Contracts;

namespace Noise.RemoteClient.Models {
    class QueuedItemNotifier : IQueuedItemNotifier, IDisposable {
        private readonly IQueuePlayProvider mPlayProvider;
        private readonly IPopupNavigation   mNavigation;
        private readonly IPlatformLog       mLog;
        private IDisposable                 mNotificationSubscription;

        public QueuedItemNotifier( IQueuePlayProvider playProvider, IPopupNavigation navigation, IPlatformLog log ) {
            mPlayProvider = playProvider;
            mNavigation = navigation;
            mLog = log;
        }

        public void StartNotifications() {
            mNotificationSubscription = mPlayProvider.ItemQueued.Subscribe( OnItemPlayed );
        }

        public void StopNotifications() {
            mNotificationSubscription?.Dispose();
            mNotificationSubscription = null;
        }

        private async void OnItemPlayed( QueuedItem item ) {
            try {
                await mNavigation.PushAsync( new QueuedPopup( item ));
                await Task.Delay( 2000 );
                await mNavigation.PopAsync();
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( OnItemPlayed ), ex );
            }
        }

        public void Dispose() {
            StopNotifications();
        }
    }
}
