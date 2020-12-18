using System.Threading.Tasks;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface ITagInformationProvider {
        Task<TagListResponse>           GetUserTags();
        Task<TagAssociationsResponse>   GetAssociations( TagInfo forTag );
    }
}
