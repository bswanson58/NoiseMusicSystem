using System.Threading.Tasks;
using Grpc.Core;
using Noise.Infrastructure.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteServer.Services {
    class TagInformationService : TagInformation.TagInformationBase {
        private readonly IUserTagManager    mTagManager;

        public TagInformationService( IUserTagManager tagManager ) {
            mTagManager = tagManager;
        }

        public override Task<TagListResponse> GetUserTags( TagInformationEmpty request, ServerCallContext context ) {
            return base.GetUserTags( request, context );
        }
    }
}
