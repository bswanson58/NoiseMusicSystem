using System;
using System.Linq;
using DynamicData;
using DynamicData.Binding;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;

namespace Noise.RemoteClient.ViewModels {
    class SearchViewModel : BindableBase, IDisposable {
        private readonly ISearchProvider            mSearchProvider;
        private readonly IQueuePlayProvider         mPlayProvider;
        private readonly SourceList<UiSearchItem>   mSearchItems;
        private IDisposable                         mLibraryStatusSubscription;
        private IDisposable                         mListSubscription;
        private bool                                mLibraryOpen;
        private bool                                mIsBusy;
        private string                              mSearchTerm;

        public  DelegateCommand                     Search { get; }

        public  ObservableCollectionExtended<UiSearchItem>  SearchItems { get; }

        public SearchViewModel( ISearchProvider searchProvider, IQueuePlayProvider playProvider, IHostInformationProvider hostInformationProvider ) {
            mSearchProvider = searchProvider;
            mPlayProvider = playProvider;

            Search = new DelegateCommand( OnSearch );
            SearchItems = new ObservableCollectionExtended<UiSearchItem>();
            mSearchItems = new SourceList<UiSearchItem>();
            mListSubscription = mSearchItems.Connect().Bind( SearchItems ).Subscribe();

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
        }

        public string SearchTerm {
            get => mSearchTerm;
            set => SetProperty( ref mSearchTerm, value );
        }

        public bool IsBusy {
            get => mIsBusy;
            set => SetProperty( ref mIsBusy, value );
        }

        private void OnSearch() {
            LoadSearchItems( SearchTerm );
        }

        private void OnLibraryStatus( LibraryStatus status ) {
            mLibraryOpen = status?.LibraryOpen == true;
        }

        private async void LoadSearchItems( string searchTerm ) {
            mSearchItems.Clear();
            IsBusy = true;

            if(( mLibraryOpen ) &&
               (!String.IsNullOrWhiteSpace( searchTerm ))) {
                var searchResults = await mSearchProvider.Search( searchTerm );

                if( searchResults.Success ) {
                    mSearchItems.AddRange( from item in searchResults.SearchResults orderby item.TrackName, item.ArtistName select new UiSearchItem( item, OnPlay ));
                }
            }

            IsBusy = false;
        }

        private TrackInfo CreateTrack( SearchItemInfo fromSearchItem ) {
            return new TrackInfo {
                TrackId = fromSearchItem.TrackId, ArtistId = fromSearchItem.ArtistId, AlbumId = fromSearchItem.AlbumId,
                TrackName = fromSearchItem.TrackName, ArtistName = fromSearchItem.ArtistName, AlbumName = fromSearchItem.AlbumName, VolumeName = fromSearchItem.VolumeName,
                TrackNumber = fromSearchItem.TrackNumber, Duration = fromSearchItem.Duration, IsFavorite = fromSearchItem.IsFavorite, Rating = fromSearchItem.Rating
            };
        }

        private void OnPlay( SearchItemInfo searchItem ) {
            mPlayProvider.Queue( CreateTrack( searchItem ));
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;

            mListSubscription?.Dispose();
            mListSubscription = null;
        }
    }
}
