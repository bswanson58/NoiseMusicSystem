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
    class ArtistListViewModel : BindableBase, IDisposable {
        private readonly IArtistProvider    mArtistProvider;
        private readonly IClientState       mClientState;
        private readonly List<ArtistInfo>   mArtistList;
        private bool                        mLibraryOpen;
        private string                      mFilterText;
        private IDisposable                 mLibraryStatusSubscription;
        private ArtistInfo                  mSelectedArtist;

        public  ObservableCollection<ArtistInfo>    ArtistList { get; }

        public ArtistListViewModel( IArtistProvider artistProvider, IHostInformationProvider hostInformationProvider, IClientState clientState ) {
            mArtistProvider = artistProvider;
            mClientState = clientState;

            mArtistList = new List<ArtistInfo>();
            ArtistList = new ObservableCollection<ArtistInfo>();

            FilterText = String.Empty;

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
        }

        public ArtistInfo SelectedArtist {
            get => mSelectedArtist;
            set => SetProperty( ref mSelectedArtist, value, OnArtistSelected );
        }

        private void OnArtistSelected() {
            if( mSelectedArtist != null ) {
                mClientState.SetCurrentArtist( mSelectedArtist );

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
            mArtistList.Clear();

            if( mLibraryOpen ) {
                var list = await mArtistProvider.GetArtistList();

                if( list?.Success == true ) {
                    mArtistList.AddRange( from a in list.ArtistList orderby a.ArtistName select a );
                }
            }

            RefreshArtistList();
        }

        private void RefreshArtistList() {
            ArtistList.Clear();

            mArtistList.Where( FilterArtist ).ForEach( a => ArtistList.Add( a ));
        }

        private bool FilterArtist( ArtistInfo artist ) {
            var retValue = true;

            if(!String.IsNullOrWhiteSpace( FilterText )) {
                retValue = artist.ArtistName.ToLower().Contains( FilterText.ToLower());
            }

            return retValue;
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;
        }
    }
}
