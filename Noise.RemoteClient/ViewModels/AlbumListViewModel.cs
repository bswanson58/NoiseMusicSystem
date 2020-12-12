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
        private readonly IAlbumProvider mAlbumProvider;
        private readonly IClientState   mClientState;
        private IDisposable             mLibraryStatusSubscription;
        private IDisposable             mStateSubscription;
        private ArtistInfo              mCurrentArtist;
        private bool                    mLibraryOpen;
        private AlbumInfo               mSelectedAlbum;

        public  ObservableCollection<AlbumInfo> AlbumList { get; }

        public AlbumListViewModel( IAlbumProvider albumProvider, IHostInformationProvider hostInformationProvider, IClientState clientState ) {
            mAlbumProvider = albumProvider;
            mClientState = clientState;

            AlbumList = new ObservableCollection<AlbumInfo>();

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
            mStateSubscription = mClientState.CurrentArtist.Subscribe( OnArtistState );
        }

        private void OnArtistState( ArtistInfo artist ) {
            mCurrentArtist = artist;

            LoadAlbumList();
        }

        public AlbumInfo SelectedAlbum {
            get => mSelectedAlbum;
            set => SetProperty( ref mSelectedAlbum, value, OnAlbumSelected );
        }

        private void OnAlbumSelected() {
            if( mSelectedAlbum != null ) {
                mClientState.SetCurrentAlbum( mSelectedAlbum );

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
                    foreach( var artist in list.AlbumList.OrderBy( a => a.AlbumName )) {
                        AlbumList.Add( artist );
                    }
                }
            }
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;

            mStateSubscription?.Dispose();
            mStateSubscription = null;
        }
    }
}
