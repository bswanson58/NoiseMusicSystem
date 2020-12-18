using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;

namespace Noise.RemoteClient.ViewModels {
    class FavoritesListViewModel : ListBase<UiTrack> {
        private readonly ITrackProvider     mTrackProvider;

        public FavoritesListViewModel( ITrackProvider trackProvider, IQueuePlayProvider queuePlayProvider, IHostInformationProvider hostInformationProvider ) :
            base( queuePlayProvider, hostInformationProvider ) {
            mTrackProvider = trackProvider;

            InitializeLibrarySubscription();
        }

        protected override void OnLibraryStatusChanged( LibraryStatus status ) {
            if( status?.LibraryOpen == true ) {
                LoadList();
            }
        }

        protected override async Task<IEnumerable<UiTrack>> RetrieveList() {
            IEnumerable<UiTrack> retValue = new List<UiTrack>();

            var list = await mTrackProvider.GetFavoriteTracks();

            if( list?.Success == true ) {
                retValue = from track in list.TrackList orderby track.TrackName, track.ArtistName, track.AlbumName select new UiTrack( track, OnPlay );
            }

            return retValue;
        }
    }
}
