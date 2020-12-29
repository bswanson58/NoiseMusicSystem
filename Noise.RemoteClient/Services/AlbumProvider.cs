using System;
using System.Threading.Tasks;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class AlbumProvider : BaseProvider<AlbumInformation.AlbumInformationClient>, IAlbumProvider {
        private readonly IPlatformLog   mLog;

        public AlbumProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider, IPlatformLog log ) :
            base( serviceLocator, hostProvider ) {
            mLog = log;
        }

        public async Task<AlbumListResponse> GetAlbumList( long artistId ) {
            var client = Client;

            if( client != null ) {
                try {
                    return await client.GetAlbumListAsync( new AlbumListRequest { ArtistId = artistId });
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( GetAlbumList ), ex );
                }
            }

            return default;
        }

        public async Task<AlbumListResponse> GetFavoriteAlbums() {
            var client = Client;

            if( client != null ) {
                try {
                    return await client.GetFavoriteAlbumsAsync( new AlbumInfoEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( GetFavoriteAlbums ), ex );
                }
            }

            return default;
        }

        public async Task<AlbumUpdateResponse> UpdateAlbumRatings( AlbumInfo album ) {
            var client = Client;

            if( client != null ) {
                try {
                    return await client.UpdateAlbumRatingsAsync( new AlbumUpdateRequest { Album = album });
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( UpdateAlbumRatings ), ex );
                }
            }

            return default;
        }
    }
}
