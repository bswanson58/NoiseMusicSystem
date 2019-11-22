using System.Threading.Tasks;
using Album4Matter.Dto;

namespace Album4Matter.Interfaces {
    public interface IAlbumBuilder {
        Task<bool>  BuildAlbum( TargetAlbumLayout layout );
    }
}
