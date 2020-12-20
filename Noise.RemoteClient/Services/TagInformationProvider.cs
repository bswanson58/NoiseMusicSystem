using System;
using System.Threading.Tasks;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class TagInformationProvider : BaseProvider<TagInformation.TagInformationClient>, ITagInformationProvider {
        private readonly IPlatformLog   mLog;

        public TagInformationProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider, IPlatformLog log ) :
            base( serviceLocator, hostProvider ) {
            mLog = log;
        }

        public async Task<TagListResponse> GetUserTags() {
            var client = Client;

            if( client != null ) {
                try {
                    return await client.GetUserTagsAsync( new TagInformationEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( "GetUserTags", ex );
                }
            }

            return default;
        }

        public async Task<TagAssociationsResponse> GetAssociations( TagInfo forTag ) {
            var client = Client;

            if( client != null ) {
                try {
                    return await client.GetTagAssociationsAsync( new TagAssociationRequest { TagId = forTag.TagId });
                }
                catch( Exception ex ) {
                    mLog.LogException( "GetAssociations", ex );
                }
            }

            return default;
        }
    }
}
