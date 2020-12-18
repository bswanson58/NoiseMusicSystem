using System.Threading.Tasks;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class TagInformationProvider : BaseProvider<TagInformation.TagInformationClient>, ITagInformationProvider {
        public TagInformationProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider )
            : base( serviceLocator, hostProvider ) {
        }

        public async Task<TagListResponse> GetUserTags() {
            var client = Client;

            if( client != null ) {
                return await client.GetUserTagsAsync( new TagInformationEmpty());
            }

            return default;
        }

        public async Task<TagAssociationsResponse> GetAssociations( TagInfo forTag ) {
            var client = Client;

            if( client != null ) {
                return await client.GetTagAssociationsAsync( new TagAssociationRequest { TagId = forTag.TagId });
            }

            return default;
        }
    }
}
