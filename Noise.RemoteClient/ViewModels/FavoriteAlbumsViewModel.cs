using System;
using System.Linq;
using DynamicData.Binding;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Support;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Noise.RemoteClient.ViewModels {
    class FavoriteAlbumsViewModel : BindableBase, IDisposable {
        private readonly IClientState               mClientState;
        private readonly IQueueListener             mQueueListener;
        private readonly IAlbumProvider             mAlbumProvider;
        private readonly IQueuePlayProvider         mPlayProvider;
        private readonly IPrefixedNameHandler       mPrefixedNameHandler;
        private bool                                mIsBusy;
        private UiAlbum                             mSelectedAlbum;
        private PlayingState                        mPlayingState;
        private IDisposable                         mPlayingTrackSubscription;
        private ObservableCollectionExtended<UiAlbum>   mAlbumList;

        public  DelegateCommand<UiAlbum>                SelectAlbum { get; }

        public FavoriteAlbumsViewModel( IAlbumProvider albumProvider, IQueuePlayProvider queuePlayProvider, IClientState clientState,
                                        IQueueListener queueListener, IPrefixedNameHandler prefixedNameHandler ) {
            mAlbumProvider = albumProvider;
            mPlayProvider = queuePlayProvider;
            mQueueListener = queueListener;
            mClientState = clientState;
            mPrefixedNameHandler = prefixedNameHandler;

            SelectAlbum = new DelegateCommand<UiAlbum>( OnSelectAlbum );
        }

        public ObservableCollectionExtended<UiAlbum> AlbumList {
            get {
                if( mAlbumList == null ) {
                    mAlbumList = new ObservableCollectionExtended<UiAlbum>();

                    Initialize();
                }

                return mAlbumList;
            }
        }

        private void Initialize() {
            LoadAlbumList();

            mPlayingTrackSubscription = mQueueListener.CurrentlyPlaying.Subscribe( OnPlaying );
        }

        private void OnPlaying( PlayingState state ) {
            mPlayingState = state;

            UpdatePlayStates();
        }

        private void UpdatePlayStates() {
            AlbumList?.ForEach( a => a.SetIsPlaying( mPlayingState ));
        }

        public UiAlbum SelectedAlbum {
            get => mSelectedAlbum;
            set => SetProperty( ref mSelectedAlbum, value, OnAlbumSelected );
        }

        private void OnSelectAlbum( UiAlbum album ) {
            mSelectedAlbum = album;

            OnAlbumSelected();
        }

        private void OnAlbumSelected() {
            if( mSelectedAlbum?.Album != null ) {
                mClientState.SetCurrentAlbum( mSelectedAlbum.Album );

                Shell.Current.GoToAsync( RouteNames.TrackList );

                SelectedAlbum = null;
            }
        }

        public bool IsBusy {
            get => mIsBusy;
            set => SetProperty( ref mIsBusy, value );
        }

        private async void LoadAlbumList() {
            IsBusy = true;

            if( mAlbumList != null ) {
                mAlbumList.Clear();

                var list = await mAlbumProvider.GetFavoriteAlbums();

                if( list?.Success == true ) {
                    mAlbumList.AddRange( from a in list.AlbumList orderby a.AlbumName select CreateAlbum( a ));
                }
            }

            IsBusy = false;
        }

        private UiAlbum CreateAlbum( AlbumInfo fromAlbum ) {
            var retValue = new UiAlbum( fromAlbum, OnAlbumPlay );

            retValue.SetDisplayName( mPrefixedNameHandler.FormatPrefixedName( retValue.AlbumName ));
            retValue.SetSortName( mPrefixedNameHandler.FormatSortingName( retValue.AlbumName ));
            retValue.SetIsPlaying( mPlayingState );

            return retValue;
        }

        private void OnAlbumPlay( UiAlbum album ) {
            mPlayProvider.Queue( album.Album );
        }

        public void Dispose() {
            mPlayingTrackSubscription?.Dispose();
            mPlayingTrackSubscription = null;
        }
    }
}
