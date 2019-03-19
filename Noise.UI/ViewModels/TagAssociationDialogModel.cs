using Caliburn.Micro;
using Noise.UI.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
    class TagAssociationDialogModel : DialogModelBase {
        public BindableCollection<UiTag>    TagList { get; }
        public string                       TrackName { get; }

        public TagAssociationDialogModel( string trackName ) {
            TrackName = trackName;

            TagList = new BindableCollection<UiTag>();
        }
    }
}
