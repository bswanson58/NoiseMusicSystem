using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData.Binding;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Prism.Mvvm;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Noise.RemoteClient.ViewModels {
    class ArtistListViewModel : BindableBase, IDisposable {
        private readonly IArtistProvider            mArtistProvider;
        private readonly IClientState               mClientState;
        private readonly IHostInformationProvider   mHostInformationProvider;
        private readonly List<UiArtist>             mCompleteArtistList;
        private bool                                mLibraryOpen;
        private string                              mFilterText;
        private PlayingState                        mPlayingState;
        private IDisposable                         mLibraryStatusSubscription;
        private IDisposable                         mPlayingStateSubscription;
        private UiArtist                            mSelectedArtist;

        private ObservableCollectionExtended<UiArtist>  mArtistList;

        public ArtistListViewModel( IArtistProvider artistProvider, IHostInformationProvider hostInformationProvider, IClientState clientState ) {
            mArtistProvider = artistProvider;
            mClientState = clientState;
            mHostInformationProvider = hostInformationProvider;

            mCompleteArtistList = new List<UiArtist>();

            mFilterText = String.Empty;
        }

        private void Initialize() {
            mLibraryStatusSubscription = mHostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
            mPlayingStateSubscription = mClientState.CurrentlyPlaying.Subscribe( OnPlaying );
        }

        private void OnLibraryStatus( LibraryStatus status ) {
            mLibraryOpen = status?.LibraryOpen == true;

            if( mLibraryOpen ) {
                LoadArtistList();
            }
            else {
                mCompleteArtistList.Clear();

                RefreshArtistList();
            }
        }

        private void OnPlaying( PlayingState state ) {
            mPlayingState = state;

            UpdatePlayingState();
        }

        private void UpdatePlayingState() {
            mCompleteArtistList.ForEach( a => a.SetIsPlaying( mPlayingState ));
            mArtistList?.ForEach( a => a.SetIsPlaying( mPlayingState ));
        }

        public ObservableCollectionExtended<UiArtist> ArtistList {
            get {
                if( mArtistList == null ) {
                    mArtistList = new ObservableCollectionExtended<UiArtist>();

                    Initialize();
                }

                return mArtistList;
            }
        }

        public UiArtist SelectedArtist {
            get => mSelectedArtist;
            set => SetProperty( ref mSelectedArtist, value, OnArtistSelected );
        }

        private void OnArtistSelected() {
            if( mSelectedArtist != null ) {
                mClientState.SetCurrentArtist( mSelectedArtist.Artist );

                Shell.Current.GoToAsync( "albumList" );
            }
        }

        public string FilterText {
            get => mFilterText;
            set => SetProperty( ref mFilterText, value, OnFilterChanged );
        }

        private void OnFilterChanged() {
            RefreshArtistList();
        }

        private async void LoadArtistList() {
            mCompleteArtistList.Clear();

            if( mLibraryOpen ) {
                var list = await mArtistProvider.GetArtistList();

                if( list?.Success == true ) {
                    mCompleteArtistList.AddRange( from a in list.ArtistList orderby a.ArtistName select new UiArtist( a ));
                    mCompleteArtistList.ForEach( a => a.SetIsPlaying( mPlayingState ));
                }
            }

            RefreshArtistList();
        }

        private void RefreshArtistList() {
            if( mArtistList != null ) {
                mArtistList.Clear();

                mArtistList.AddRange( from artist in mCompleteArtistList where FilterArtist( artist ) select artist );
            }
        }

        private bool FilterArtist( UiArtist artist ) {
            var retValue = true;

            if(!String.IsNullOrWhiteSpace( FilterText )) {
                retValue = artist.ArtistName.ToLower().Contains( FilterText.ToLower());
            }

            return retValue;
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;

            mPlayingStateSubscription?.Dispose();
            mPlayingStateSubscription = null;
        }
    }
}
