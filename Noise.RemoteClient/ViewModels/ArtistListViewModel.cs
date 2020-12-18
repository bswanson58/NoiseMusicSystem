using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData;
using DynamicData.Binding;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Prism.Mvvm;
using Xamarin.Forms;

namespace Noise.RemoteClient.ViewModels {
    class ArtistListViewModel : BindableBase, IDisposable {
        private readonly IArtistProvider            mArtistProvider;
        private readonly IClientState               mClientState;
        private readonly List<UiArtist>             mCompleteArtistList;
        private readonly SourceList<UiArtist>       mArtistList;
        private bool                                mLibraryOpen;
        private string                              mFilterText;
        private IDisposable                         mLibraryStatusSubscription;
        private IDisposable                         mSourceSubscription;
        private UiArtist                            mSelectedArtist;

        public  ObservableCollectionExtended<UiArtist>  ArtistList { get; }

        public ArtistListViewModel( IArtistProvider artistProvider, IHostInformationProvider hostInformationProvider, IClientState clientState ) {
            mArtistProvider = artistProvider;
            mClientState = clientState;

            ArtistList = new ObservableCollectionExtended<UiArtist>();
            mCompleteArtistList = new List<UiArtist>();
            mArtistList = new SourceList<UiArtist>();
            mSourceSubscription = mArtistList.Connect().Bind( ArtistList ).Subscribe();

            FilterText = String.Empty;

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
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

        private void OnLibraryStatus( LibraryStatus status ) {
            mLibraryOpen = status?.LibraryOpen == true;

            if( mLibraryOpen ) {
                LoadArtistList();
            }
            else {
                mArtistList.Clear();

                RefreshArtistList();
            }
        }

        private async void LoadArtistList() {
            mCompleteArtistList.Clear();

            if( mLibraryOpen ) {
                var list = await mArtistProvider.GetArtistList();

                if( list?.Success == true ) {
                    mCompleteArtistList.AddRange( from a in list.ArtistList orderby a.ArtistName select new UiArtist( a ));
                }
            }

            RefreshArtistList();
        }

        private void RefreshArtistList() {
            mArtistList.Clear();

            mArtistList.Edit( list => {
                list.AddRange( from artist in mCompleteArtistList where FilterArtist( artist ) select artist );
            });
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

            mSourceSubscription?.Dispose();
            mSourceSubscription = null;
        }
    }
}
