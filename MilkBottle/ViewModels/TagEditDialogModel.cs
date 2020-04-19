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
        public  const string                cPresetParameter = "tagName";

        private readonly ITagProvider       mTagProvider;
        private readonly IPlatformLog       mLog;
        private readonly IDialogService     mDialogService;
        private Preset                      mPreset;

        public  String                      Name { get; set; }
        public  string                      Title { get; }

        public  ObservableCollection<UiTag> Tags { get; }
        public  string                      PresetName => mPreset?.Name;
        public  bool                        IsFavorite { get; set; }

        public  DelegateCommand             Ok { get; }
        public  DelegateCommand             Cancel { get; }
        public  DelegateCommand             NewTag { get; }

        public  event Action<IDialogResult> RequestClose;

        public TagEditDialogModel( ITagProvider tagProvider, IDialogService dialogService, IPlatformLog log ) {
            mTagProvider = tagProvider;
            mDialogService = dialogService;
            mLog = log;

            Title = "Associate Tags";
            Tags = new ObservableCollection<UiTag>();

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
            NewTag = new DelegateCommand( OnNewTag );
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            mPreset = parameters.GetValue<Preset>( cPresetParameter );

            UpdatePresetState();
        }

        private void UpdatePresetState() {
            Tags.Clear();
            mTagProvider.SelectTags( list => Tags.AddRange( from t in list orderby t.Name select new UiTag( t, OnTagChanged, null, null )))
                .IfLeft( ex => LogException( "UpdatePresetState.SelectTags", ex ));

            if( mPreset != null ) {
                foreach( var tag in Tags ) {
                    tag.SetSelectedState( mPreset.Tags.FirstOrDefault( t => t.Id.Equals( tag.Tag.Id )) != null );
                }

                IsFavorite = mPreset.IsFavorite;
            }

            RaisePropertyChanged( () => IsFavorite );
            RaisePropertyChanged( () => PresetName );
        }

        private void OnTagChanged( UiTag tag ) {
            mPreset = mPreset.WithTagState( tag.Tag, tag.IsSelected );
        }

        private void OnNewTag() {
            mDialogService.ShowDialog( "NewTagDialog", null, OnNewTagResult );
        }

        private void OnNewTagResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var newTag = new PresetTag( result.Parameters.GetValue<string>( NewTagDialogModel.cTagNameParameter ));

                mTagProvider.Insert( newTag );

                UpdatePresetState();
            }
        }


        public void OnOk() {
            mPreset = mPreset.WithFavorite( IsFavorite );

            RaiseRequestClose( new DialogResult( ButtonResult.OK, new DialogParameters { { TagEditDialogModel.cPresetParameter, mPreset } }));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }

        public void OnDialogClosed() { }

        private void LogException( string message, Exception ex ) {
            mLog.LogException( message, ex );
        }
    }
}
