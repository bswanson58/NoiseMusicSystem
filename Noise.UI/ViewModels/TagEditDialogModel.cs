using Noise.UI.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
    public class TagEditDialogModel : DialogModelBase {
        public  UiTag   Tag { get; }

        public TagEditDialogModel( UiTag tag ) {
            Tag = tag;
        }

        public bool IsValid => ( Tag?.Tag != null ) && (!string.IsNullOrWhiteSpace( Tag.Name ));
    }
}
