using Noise.Infrastructure.Dto;

namespace Noise.UI.Dto {
    public class UiTag {
        private readonly DbTag  mTag;

        public  string          Name => mTag.Name;

        public UiTag( DbTag tag ) {
            mTag = tag;
        }
    }
}
