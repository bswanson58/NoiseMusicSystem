using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData.Binding;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Prism.Mvvm;

namespace Noise.RemoteClient.ViewModels {
    abstract class ListBase<T> : BindableBase, IDisposable {
        private readonly IQueuePlayProvider         mQueuePlay;
        private readonly IHostInformationProvider   mHostInformationProvider;
        private IDisposable                         mLibraryStatusSubscription;
        private bool                                mLibraryOpen;
        private bool                                mIsBusy;
        private T                                   mCurrentItem;

        public  ObservableCollectionExtended<T>     DisplayList { get; }

        protected ListBase( IQueuePlayProvider queuePlayProvider, IHostInformationProvider hostInformationProvider ) {
            mQueuePlay = queuePlayProvider;
            mHostInformationProvider = hostInformationProvider;

            DisplayList = new ObservableCollectionExtended<T>();
        }

        protected void InitializeLibrarySubscription() {
            mLibraryStatusSubscription = mHostInformationProvider.LibraryStatus.ObserveOn( SynchronizationContext.Current ).Subscribe( OnLibraryStatus );
        }

        public bool IsBusy {
            get => mIsBusy;
            set => SetProperty( ref mIsBusy, value );
        }

        public T CurrentItem {
            get => mCurrentItem;
            set => SetProperty( ref mCurrentItem, value, OnCurrentItemChanged );
        }

        protected virtual void OnCurrentItemChanged() { }

        protected virtual void OnLibraryStatusChanged( LibraryStatus status ) { }

        private void OnLibraryStatus( LibraryStatus status ) {
            mLibraryOpen = status?.LibraryOpen == true;

            OnLibraryStatusChanged( status );
        }

        protected abstract Task<IEnumerable<T>> RetrieveList();

        protected async void LoadList() {
            DisplayList.Clear();
            IsBusy = true;

            if( mLibraryOpen ) {
                DisplayList.AddRange( await RetrieveList());
            }

            IsBusy = false;
        }

        protected void OnPlay( UiTrack track, bool playNext ) {
            mQueuePlay.Queue( track.Track, playNext );
        }

        protected void OnPlay( UiAlbum album ) {
            mQueuePlay.Queue( album.Album );
        }

        public virtual void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;
        }
    }
}
