using System;
using System.Collections.ObjectModel;
using System.Linq;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class TagEditDialogModel : PropertyChangeBase, IDialogAware {
        public  const string            cPresetParameter = "tagName";

        private readonly ITagProvider       mTagProvider;
        private Preset                      mPreset;

        public  String                      Name { get; set; }
        public  string                      Title { get; }

        public  ObservableCollection<UiTag> Tags { get; }
        public  string                      PresetName => mPreset?.Name;

        public  DelegateCommand             Ok { get; }
        public  DelegateCommand             Cancel { get; }

        public  event   Action<IDialogResult> RequestClose;

        public TagEditDialogModel( ITagProvider tagProvider ) {
            mTagProvider = tagProvider;

            Title = "Associate Tags";
            Tags = new ObservableCollection<UiTag>();

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            mPreset = parameters.GetValue<Preset>( cPresetParameter );

            Tags.Clear();
            mTagProvider.SelectTags( list => Tags.AddRange( from t in list orderby t.Name select new UiTag( t, OnTagChanged )));

            foreach( var tag in Tags ) {
                tag.SetSelectedState( mPreset.Tags.FirstOrDefault( t => t.Id.Equals( tag.Tag.Id )) != null );
            }

            RaisePropertyChanged( () => PresetName );
        }

        private void OnTagChanged( UiTag tag ) {
            mPreset = mPreset.WithTagState( tag.Tag, tag.IsSelected );
        }

        public void OnOk() {
            RaiseRequestClose( new DialogResult( ButtonResult.OK, new DialogParameters { { TagEditDialogModel.cPresetParameter, mPreset } }));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }

        public void OnDialogClosed() { }
    }
}
