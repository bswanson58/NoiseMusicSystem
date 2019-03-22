using Noise.UI.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
    public class TagEditDialogModel : DialogModelBase {
        public  UiTag   Tag { get; }
        public  bool    DeleteRequested {get; private set; }

        public TagEditDialogModel( UiTag tag ) {
            Tag = tag;

            DeleteRequested = false;
        }

        public bool IsValid => ( Tag?.Tag != null ) && (!string.IsNullOrWhiteSpace( Tag.Name ));

        public void Execute_DeleteTag() {
            DeleteRequested = true;

            DialogWindow?.Close( true );
        }
    }
}
