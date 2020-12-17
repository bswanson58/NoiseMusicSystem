using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Mvvm;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Noise.RemoteClient.ViewModels {
    class AlbumListViewModel : BindableBase, IDisposable {
        private readonly IAlbumProvider         mAlbumProvider;
        private readonly IClientState           mClientState;
        private readonly IQueuePlayProvider     mPlayProvider;
        private readonly List<UiAlbum>          mAlbumList;
        private IDisposable                     mLibraryStatusSubscription;
        private IDisposable                     mStateSubscription;
        private ArtistInfo                      mCurrentArtist;
        private string                          mFilterText;
        private bool                            mLibraryOpen;
        private UiAlbum                         mSelectedAlbum;

        public  string                          ArtistName { get; private set; }
        public  Int32                           AlbumCount { get; private set; }
        public  ObservableCollection<UiAlbum>   AlbumList { get; }

        public AlbumListViewModel( IAlbumProvider albumProvider, IHostInformationProvider hostInformationProvider, IQueuePlayProvider queuePlayProvider,
                                   IClientState clientState ) {
            mAlbumProvider = albumProvider;
            mPlayProvider = queuePlayProvider;
            mClientState = clientState;

            mAlbumList = new List<UiAlbum>();
            AlbumList = new ObservableCollection<UiAlbum>();

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
            mStateSubscription = mClientState.CurrentArtist.Subscribe( OnArtistState );
        }

        private void OnArtistState( ArtistInfo artist ) {
            mCurrentArtist = artist;

            if( mCurrentArtist != null ) {
                ArtistName = mCurrentArtist.ArtistName;

                RaisePropertyChanged( nameof( ArtistName ));
            }

            LoadAlbumList();
        }

        public string FilterText {
            get => mFilterText;
            set => SetProperty( ref mFilterText, value, OnFilterChanged );
        }

        private void OnFilterChanged() {
            RefreshAlbumList();
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
            mLibraryOpen = status?.LibraryOpen == true;

            if( mLibraryOpen ) {
                LoadAlbumList();
            }
            else {
                mAlbumList.Clear();

                RefreshAlbumList();
            }
        }

        private async void LoadAlbumList() {
            mAlbumList.Clear();

            if(( mLibraryOpen ) &&
               ( mCurrentArtist != null )) {
                var list = await mAlbumProvider.GetAlbumList( mCurrentArtist.DbId );

                if( list?.Success == true ) {
                    mAlbumList.AddRange( from a in list.AlbumList orderby a.AlbumName select new UiAlbum( a, OnAlbumPlay ));
                }
            }

            RefreshAlbumList();
        }

        private void RefreshAlbumList() {
            AlbumList.Clear();

            mAlbumList.Where( FilterAlbum ).ForEach( a => AlbumList.Add( a ));

            AlbumCount = AlbumList.Count;
            RaisePropertyChanged( nameof( AlbumCount ));
        }

        private bool FilterAlbum( UiAlbum album ) {
            var retValue = true;

            if(!String.IsNullOrWhiteSpace( FilterText )) {
                retValue = album.AlbumName.ToLower().Contains( FilterText.ToLower());
            }

            return retValue;
        }

        private void OnAlbumPlay( UiAlbum album ) {
            mPlayProvider.Queue( album.Album );
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;

            mStateSubscription?.Dispose();
            mStateSubscription = null;
        }
    }
}
