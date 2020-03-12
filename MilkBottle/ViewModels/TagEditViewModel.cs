using System;
using System.Collections.ObjectModel;
using System.Linq;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using Prism;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class TagEditViewModel : PropertyChangeBase, IActiveAware {
        private readonly IDialogService mDialogService;
        private readonly ITagProvider   mTagProvider;
        private PresetTag               mCurrentTag;
        private bool                    mIsActive;

        public  ObservableCollection<PresetTag> Tags { get; }
        public  ObservableCollection<Preset>    TaggedPresets {  get; }

        public  DelegateCommand                 NewTag { get; }

        public  string                          Title => "Tags";
        public  event EventHandler              IsActiveChanged = delegate { };

        public TagEditViewModel( ITagProvider tagProvider, IDialogService dialogService ) {
            mTagProvider = tagProvider;
            mDialogService = dialogService;

            Tags = new ObservableCollection<PresetTag>();
            TaggedPresets = new ObservableCollection<Preset>();

            NewTag = new DelegateCommand( OnNewTag );

            LoadTags();
        }

        public bool IsActive {
            get => mIsActive;
            set {
                mIsActive = value;

                if( mIsActive ) {
                    LoadTags();
                }
            }
        }

        public PresetTag CurrentTag {
            get => mCurrentTag;
            set {
                mCurrentTag = value;

                OnTagChanged();
                RaisePropertyChanged( () => CurrentTag );
            }
        }

        private void LoadTags() {
            Tags.Clear();

            mTagProvider.SelectTags( list => Tags.AddRange( from t in list orderby t.Name select t ));
        }

        private void OnTagChanged() {

        }

        private void OnNewTag() {
            mDialogService.ShowDialog( "NewTagDialog", null, OnNewTagResult );
        }

        private void OnNewTagResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var newTag = new PresetTag( result.Parameters.GetValue<string>( NewTagDialogModel.cTagNameParameter ));

                mTagProvider.Insert( newTag );

                LoadTags();
            }
        }
    }
}
