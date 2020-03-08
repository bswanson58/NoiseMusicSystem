using System.Collections.ObjectModel;
using System.Linq;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using Prism.Commands;

namespace MilkBottle.ViewModels {
    class TagEditViewModel {
        private readonly ITagProvider   mTagProvider;

        public  ObservableCollection<PresetTag> Tags { get; }
        public  ObservableCollection<Preset>    TaggedPresets {  get; }

        public  DelegateCommand                 NewTag { get; }

        public TagEditViewModel( ITagProvider tagProvider ) {
            mTagProvider = tagProvider;

            Tags = new ObservableCollection<PresetTag>();
            TaggedPresets = new ObservableCollection<Preset>();

            NewTag = new DelegateCommand( OnNewTag );

            LoadTags();
        }

        private void LoadTags() {
            Tags.Clear();

            mTagProvider.SelectTags( list => Tags.AddRange( from t in list orderby t.Name select t ));
        }

        private void OnNewTag() {

        }
    }
}
