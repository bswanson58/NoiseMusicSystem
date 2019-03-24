using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure.Dto;
using Noise.UI.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
    class TagAssociationDialogModel : DialogModelBase {
        public BindableCollection<UiTag>    TagList { get; }
        public DbTrack                      Track { get; }
        public string                       TrackName => Track.Name;

        public TagAssociationDialogModel( DbTrack track, IEnumerable<DbTag> allTags, IEnumerable<DbTag> currentTags ) {
            Track = track;

            TagList = new BindableCollection<UiTag>();
            TagList.AddRange( from tag in allTags orderby tag.Name select new UiTag( tag ));

            foreach( var tag in currentTags ) {
                var uiTag = TagList.FirstOrDefault( t => t.Tag.DbId.Equals( tag.DbId ));

                if( uiTag!= null ) {
                    uiTag.IsChecked = true;
                }
            }
        }

        public IEnumerable<DbTag> GetSelectedTags() {
            return from tag in TagList where tag.IsChecked select tag.Tag;
        }
    }
}
