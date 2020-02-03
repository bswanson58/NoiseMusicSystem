using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.RavenDatabase.Support {
    class RavenTrackUpdateShell : RavenDataUpdateShell<DbTrack>, ITrackUpdateShell {
        public RavenTrackUpdateShell( Action<DbTrack> update, DbTrack item )
            : base( update, item ) { }

        public void UpdateTrackAndAlbum() {
            throw new NotImplementedException();
        }
    }
}
