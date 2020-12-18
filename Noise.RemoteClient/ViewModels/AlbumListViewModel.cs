using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData;
using DynamicData.Binding;
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
        private readonly List<UiAlbum>          mCompleteAlbumList;
        private readonly SourceList<UiAlbum>    mAlbumList;
        private IDisposable                     mLibraryStatusSubscription;
        private IDisposable                     mStateSubscription;
        private IDisposable                     mListSubscription;
        private ArtistInfo                      mCurrentArtist;
        private string                          mFilterText;
        private bool                            mLibraryOpen;
        private UiAlbum                         mSelectedAlbum;

        public  string                                  ArtistName { get; private set; }
        public  Int32                                   AlbumCount { get; private set; }
        public  ObservableCollectionExtended<UiAlbum>   AlbumList { get; }

        public AlbumListViewModel( IAlbumProvider albumProvider, IHostInformationProvider hostInformationProvider, IQueuePlayProvider queuePlayProvider,
                                   IClientState clientState ) {
            mAlbumProvider = albumProvider;
            mPlayProvider = queuePlayProvider;
            mClientState = clientState;

            mCompleteAlbumList = new List<UiAlbum>();
            mAlbumList = new SourceList<UiAlbum>();
            AlbumList = new ObservableCollectionExtended<UiAlbum>();
            mListSubscription = mAlbumList.Connect().Bind( AlbumList ).Subscribe();

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
            mCompleteAlbumList.Clear();

            if(( mLibraryOpen ) &&
               ( mCurrentArtist != null )) {
                var list = await mAlbumProvider.GetAlbumList( mCurrentArtist.DbId );

                if( list?.Success == true ) {
                    mCompleteAlbumList.AddRange( from a in list.AlbumList orderby a.AlbumName select new UiAlbum( a, OnAlbumPlay ));
                }
            }

            RefreshAlbumList();
        }

        private void RefreshAlbumList() {
            mAlbumList.Clear();

            mAlbumList.Edit( list => {
                list.AddRange( from a in mCompleteAlbumList where FilterAlbum( a ) select a );
            });

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

            mListSubscription?.Dispose();
            mListSubscription = null;
        }
    }
}
