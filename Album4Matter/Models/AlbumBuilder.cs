using Album4Matter.Dto;
using Album4Matter.Interfaces;

namespace Album4Matter.Models {
    class AlbumBuilder : IAlbumBuilder {
        private readonly IPlatformLog   mLog;

        public AlbumBuilder( IPlatformLog log ) {
            mLog = log;
        }

        public void BuildAlbum( TargetAlbumLayout layout ) {
        }
    }
}
