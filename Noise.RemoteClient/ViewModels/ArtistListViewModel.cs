using System;
using System.Collections.ObjectModel;
using System.Linq;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Mvvm;
using Xamarin.Forms;

namespace Noise.RemoteClient.ViewModels {
    class ArtistListViewModel : BindableBase, IDisposable {
        private readonly IArtistProvider    mArtistProvider;
        private readonly IClientState       mClientState;
        private IDisposable                 mLibraryStatusSubscription;
        private ArtistInfo                  mSelectedArtist;

        public  ObservableCollection<ArtistInfo>    ArtistList { get; }

        public ArtistListViewModel( IArtistProvider artistProvider, IHostInformationProvider hostInformationProvider, IClientState clientState ) {
            mArtistProvider = artistProvider;
            mClientState = clientState;

            ArtistList = new ObservableCollection<ArtistInfo>();

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

        private void OnLibraryStatus( LibraryStatus status ) {
            if( status?.LibraryOpen == true ) {
                LoadArtistList();
            }
            else {
                ArtistList.Clear();
            }
        }

        private async void LoadArtistList() {
            ArtistList.Clear();

            var list = await mArtistProvider.GetArtistList();

            if( list?.Success == true ) {
                foreach( var artist in list.ArtistList.OrderBy( a => a.ArtistName )) {
                    ArtistList.Add( artist );
                }
            }
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;
        }
    }
}
