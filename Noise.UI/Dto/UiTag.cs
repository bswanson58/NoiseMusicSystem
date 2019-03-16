using Noise.Infrastructure.Dto;

namespace Noise.UI.Dto {
    public class UiTag {
        public  DbTag   Tag { get; }
        public  string  Name => Tag.Name;

        public UiTag( DbTag tag ) {
            Tag = tag;
        }
    }
}
