using System;
using System.Collections.ObjectModel;
using System.Linq;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Mvvm;
using Xamarin.Forms;

namespace Noise.RemoteClient.ViewModels {
    class AlbumListViewModel : BindableBase, IDisposable {
        private readonly IAlbumProvider         mAlbumProvider;
        private readonly IClientState           mClientState;
        private readonly IQueuePlayProvider     mPlayProvider;
        private IDisposable                     mLibraryStatusSubscription;
        private IDisposable                     mStateSubscription;
        private ArtistInfo                      mCurrentArtist;
        private bool                            mLibraryOpen;
        private UiAlbum                         mSelectedAlbum;

        public  ObservableCollection<UiAlbum>   AlbumList { get; }

        public AlbumListViewModel( IAlbumProvider albumProvider, IHostInformationProvider hostInformationProvider, IQueuePlayProvider queuePlayProvider,
                                   IClientState clientState ) {
            mAlbumProvider = albumProvider;
            mPlayProvider = queuePlayProvider;
            mClientState = clientState;

            AlbumList = new ObservableCollection<UiAlbum>();

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
            mStateSubscription = mClientState.CurrentArtist.Subscribe( OnArtistState );
        }

        private void OnArtistState( ArtistInfo artist ) {
            mCurrentArtist = artist;

            LoadAlbumList();
        }

        public UiAlbum SelectedAlbum {
            get => mSelectedAlbum;
            set => SetProperty( ref mSelectedAlbum, value, OnAlbumSelected );
        }

        private void OnAlbumSelected() {
            if( mSelectedAlbum?.Album != null ) {
                mClientState.SetCurrentAlbum( mSelectedAlbum.Album );

                Shell.Current.GoToAsync( "trackList" );
            }
        }

        private void OnLibraryStatus( LibraryStatus status ) {
            mLibraryOpen = status.LibraryOpen;

            LoadAlbumList();
        }

        private async void LoadAlbumList() {
            AlbumList.Clear();

            if(( mLibraryOpen ) &&
               ( mCurrentArtist != null )) {
                var list = await mAlbumProvider.GetAlbumList( mCurrentArtist.DbId );

                if( list?.Success == true ) {
                    foreach( var album in list.AlbumList.OrderBy( a => a.AlbumName )) {
                        AlbumList.Add( new UiAlbum( album, OnAlbumPlay ));
                    }
                }
            }
        }

        private void OnAlbumPlay( UiAlbum album ) {
            mPlayProvider.QueueAlbum( album.Album );
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;

            mStateSubscription?.Dispose();
            mStateSubscription = null;
        }
    }
}
