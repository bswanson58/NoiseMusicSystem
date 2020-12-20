using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData.Binding;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Mvvm;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Noise.RemoteClient.ViewModels {
    class AlbumListViewModel : BindableBase {
        private readonly IClientState               mClientState;
        private readonly IAlbumProvider             mAlbumProvider;
        private readonly IQueuePlayProvider         mPlayProvider;
        private readonly List<UiAlbum>              mCompleteAlbumList;
        private readonly ArtistInfo                 mCurrentArtist;
        private string                              mFilterText;
        private UiAlbum                             mSelectedAlbum;
        private PlayingState                        mPlayingState;
        private IDisposable                         mPlayingTrackSubscription;
        private  ObservableCollectionExtended<UiAlbum>  mAlbumList;

        public  string                                  ArtistName { get; private set; }
        public  Int32                                   AlbumCount { get; private set; }

        public AlbumListViewModel( IAlbumProvider albumProvider, IQueuePlayProvider queuePlayProvider, IClientState clientState ) {
            mAlbumProvider = albumProvider;
            mPlayProvider = queuePlayProvider;
            mClientState = clientState;

            mCurrentArtist = clientState.CurrentArtist;
            mCompleteAlbumList = new List<UiAlbum>();
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
            ArtistName = mCurrentArtist?.ArtistName;
            RaisePropertyChanged( nameof( ArtistName ));

            LoadAlbumList();

            mPlayingTrackSubscription = mClientState.CurrentlyPlaying.Subscribe( OnPlaying );
        }

        private void OnPlaying( PlayingState state ) {
            mPlayingState = state;

            UpdatePlayStates();
        }

        private void UpdatePlayStates() {
            mCompleteAlbumList.ForEach( a => a.SetIsPlaying( mPlayingState ));
            mAlbumList?.ForEach( a => a.SetIsPlaying( mPlayingState ));
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

        private async void LoadAlbumList() {
            mCompleteAlbumList.Clear();

            if( mCurrentArtist != null ) {
                var list = await mAlbumProvider.GetAlbumList( mCurrentArtist.DbId );

                if( list?.Success == true ) {
                    mCompleteAlbumList.AddRange( from a in list.AlbumList orderby a.AlbumName select new UiAlbum( a, OnAlbumPlay ));
                    mCompleteAlbumList.ForEach( a => a.SetIsPlaying( mPlayingState ));
                }
            }

            RefreshAlbumList();
        }

        private void RefreshAlbumList() {
            if( mAlbumList != null ) {
                mAlbumList.Clear();

                mAlbumList.AddRange( from a in mCompleteAlbumList where FilterAlbum( a ) select a );

                AlbumCount = AlbumList.Count;
                RaisePropertyChanged( nameof( AlbumCount ));
            }
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
    }
}
