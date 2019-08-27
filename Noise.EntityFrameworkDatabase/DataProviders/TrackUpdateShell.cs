using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
    class TrackUpdateShell : EfUpdateShell<DbTrack>, ITrackUpdateShell {
        private readonly IAlbumProvider mAlbumProvider;

        public TrackUpdateShell( IAlbumProvider albumProvider, IDbContext context, DbTrack item )
            : base( context, item ) {
            mAlbumProvider = albumProvider;
        }

        public void UpdateTrackAndAlbum() {
            Update();

            using( var albumUpdater = mAlbumProvider.GetAlbumForUpdate( Item.Album )) {
                albumUpdater.Update();
            }
        }
    }
}
