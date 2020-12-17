using System;
using System.Collections.ObjectModel;
using System.Linq;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;
using Xamarin.Forms.Internals;

namespace Noise.RemoteClient.ViewModels {
    class SearchViewModel : BindableBase, IDisposable {
        private readonly ISearchProvider            mSearchProvider;
        private readonly IQueuePlayProvider         mPlayProvider;
        private IDisposable                         mLibraryStatusSubscription;
        private bool                                mLibraryOpen;
        private string                              mSearchTerm;

        public  DelegateCommand                     Search { get; }

        public  ObservableCollection<UiSearchItem>  SearchItems { get; }

        public SearchViewModel( ISearchProvider searchProvider, IQueuePlayProvider playProvider, IHostInformationProvider hostInformationProvider ) {
            mSearchProvider = searchProvider;
            mPlayProvider = playProvider;

            Search = new DelegateCommand( OnSearch );
            SearchItems = new ObservableCollection<UiSearchItem>();

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
        }

        public string SearchTerm {
            get => mSearchTerm;
            set => SetProperty( ref mSearchTerm, value );
        }

        private void OnSearch() {
            LoadSearchItems( SearchTerm );
        }

        private void OnLibraryStatus( LibraryStatus status ) {
            mLibraryOpen = status?.LibraryOpen == true;
        }

        private async void LoadSearchItems( string searchTerm ) {
            SearchItems.Clear();

            if(( mLibraryOpen ) &&
               (!String.IsNullOrWhiteSpace( searchTerm ))) {
                var searchResults = await mSearchProvider.Search( searchTerm );

                if( searchResults.Success ) {
                    searchResults.SearchResults
                        .OrderBy( s => s.TrackName )
                        .ThenBy( s => s.ArtistName )
                        .ForEach( r => SearchItems.Add( new UiSearchItem( r, OnPlay )));
                }
            }
        }

        private void OnPlay( SearchItemInfo searchItem ) {
            mPlayProvider.QueueTrack( searchItem.TrackId );
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;
        }
    }
}
