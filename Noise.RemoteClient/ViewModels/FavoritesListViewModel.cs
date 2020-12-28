using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Support;
using Prism.Commands;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms.Internals;

namespace Noise.RemoteClient.ViewModels {
    class FavoritesListViewModel : ListBase<UiTrack> {
        private readonly ITrackProvider     mTrackProvider;
        private readonly IPreferences       mPreferences;
        private PlayingState                mPlayingState;
        private IDisposable                 mPlayingStateSubscription;
        private SortTypes                   mSortOrder;

        public  DelegateCommand             SortByName { get; }
        public  DelegateCommand             SortByArtist { get; }

        public FavoritesListViewModel( ITrackProvider trackProvider, IQueuePlayProvider queuePlayProvider, IHostInformationProvider hostInformationProvider,
                                       IClientState clientState, IPreferences preferences ) :
            base( queuePlayProvider, hostInformationProvider ) {
            mTrackProvider = trackProvider;
            mPreferences = preferences;

            SortByName = new DelegateCommand( OnSortByName );
            SortByArtist = new DelegateCommand( OnSortByArtist );

            InitializeLibrarySubscription();
            mPlayingStateSubscription = clientState.CurrentlyPlaying.Subscribe( OnPlayingState );
        }

        protected override void OnLibraryStatusChanged( LibraryStatus status ) {
            if( status?.LibraryOpen == true ) {
                LoadList();
            }
        }

        private void OnPlayingState( PlayingState state ) {
            mPlayingState = state;

            DisplayList.ForEach( f => f.SetIsPlaying( mPlayingState ));
        }

        private void OnSortByName() {
            SetSortTo( SortTypes.Name );
        }

        private void OnSortByArtist() {
            SetSortTo( SortTypes.Artist );
        }

        private void SetSortTo( SortTypes sort ) {
            mSortOrder = sort;
            mPreferences.Set( PreferenceNames.FavoritesListSorting, mSortOrder.ToString());

            LoadList();
        }

        protected override async Task<IEnumerable<UiTrack>> RetrieveList() {
            IEnumerable<UiTrack> retValue = new List<UiTrack>();

            var list = await mTrackProvider.GetFavoriteTracks();

            if( list?.Success == true ) {
                if( mSortOrder == SortTypes.Artist ) {
                    retValue = from track in list.TrackList orderby track.ArtistName, track.TrackName, track.AlbumName 
                               select new UiTrack( track, OnPlay, mPlayingState );
                }
                else {
                    retValue = from track in list.TrackList orderby track.TrackName, track.ArtistName, track.AlbumName 
                               select new UiTrack( track, OnPlay, mPlayingState );
                }
            }

            return retValue;
        }

        public override void Dispose() {
            mPlayingStateSubscription?.Dispose();
            mPlayingStateSubscription = null;

            base.Dispose();
        }
    }
}
