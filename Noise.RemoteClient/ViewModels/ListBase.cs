using System;
using System.Collections.Generic;
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
        private T                                   mCurrentItem;

        public  ObservableCollectionExtended<T>     DisplayList { get; }

        protected ListBase( IQueuePlayProvider queuePlayProvider, IHostInformationProvider hostInformationProvider ) {
            mQueuePlay = queuePlayProvider;
            mHostInformationProvider = hostInformationProvider;

            DisplayList = new ObservableCollectionExtended<T>();
        }

        protected void InitializeLibrarySubscription() {
            mLibraryStatusSubscription = mHostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
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

            if( mLibraryOpen ) {
                DisplayList.AddRange( await RetrieveList());
            }
        }

        protected void OnPlay( UiTrack track ) {
            mQueuePlay.Queue( track.Track );
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
