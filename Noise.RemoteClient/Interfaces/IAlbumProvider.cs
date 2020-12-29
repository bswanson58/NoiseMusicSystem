using System.Threading.Tasks;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface IAlbumProvider {
        Task<AlbumListResponse>     GetAlbumList( long artistId );
        Task<AlbumListResponse>     GetFavoriteAlbums();
        Task<AlbumUpdateResponse>   UpdateAlbumRatings( AlbumInfo album );
    }
}
