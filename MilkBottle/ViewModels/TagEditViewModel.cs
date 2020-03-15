using System;
using System.Collections.ObjectModel;
using System.Linq;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Views;
using Prism;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class TagEditViewModel : PropertyChangeBase, IActiveAware {
        private readonly IDialogService         mDialogService;
        private readonly ITagProvider           mTagProvider;
        private readonly IPresetProvider        mPresetProvider;
        private readonly IPresetController      mPresetController;
        private readonly IStateManager          mStateManager;
        private readonly IPlatformLog           mLog;
        private UiTag                           mCurrentTag;
        private UiPreset                        mCurrentPreset;
        private bool                            mIsActive;

        public  ObservableCollection<UiTag>     Tags { get; }
        public  ObservableCollection<UiPreset>  TaggedPresets {  get; }
        public  string                          PresetListTitle => TaggedPresets.Any() ? $"({TaggedPresets.Count}) Tagged Presets" : " Tagged Presets ";

        public  DelegateCommand                 NewTag { get; }
        public  DelegateCommand                 DeleteTag {  get; }

        public  string                          Title => "Tags";
        public  event EventHandler              IsActiveChanged = delegate { };

        public TagEditViewModel( ITagProvider tagProvider, IPresetProvider presetProvider, IPresetController presetController, 
                                 IStateManager stateManager, IDialogService dialogService, IPlatformLog log ) {
            mTagProvider = tagProvider;
            mPresetProvider = presetProvider;
            mDialogService = dialogService;
            mPresetController = presetController;
            mStateManager = stateManager;
            mLog = log;

            Tags = new ObservableCollection<UiTag>();
            TaggedPresets = new ObservableCollection<UiPreset>();

            NewTag = new DelegateCommand( OnNewTag );
            DeleteTag = new DelegateCommand( OnDeleteTag, CanDeleteTag );

            mPresetController.BlendPresetTransition = false;
            mPresetController.ConfigurePresetSequencer( PresetSequence.Sequential );
            mPresetController.ConfigurePresetTimer( PresetTimer.Infinite );
            mStateManager.EnterState( eStateTriggers.Stop );

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

        public UiTag CurrentTag {
            get => mCurrentTag;
            set {
                mCurrentTag = value;

                OnTagChanged();
                RaisePropertyChanged( () => CurrentTag );
            }
        }

        public UiPreset CurrentPreset {
            get => mCurrentPreset;
            set {
                mCurrentPreset = value;

                OnPresetChanged();
                RaisePropertyChanged( () => CurrentPreset );
            }
        }

        private void OnPresetChanged() {
            if( mCurrentPreset != null ) {
                mPresetController.PlayPreset( mCurrentPreset.Preset );

                mStateManager.EnterState( eStateTriggers.Run );
            }
            else {
                mStateManager.EnterState( eStateTriggers.Stop );
            }
        }

        private void LoadTags() {
            var previousTag = mCurrentTag;

            Tags.Clear();

            mTagProvider.SelectTags( list => Tags.AddRange( from t in list orderby t.Name select new UiTag(  t, null, OnEditTag, OnDeleteTag )))
                .IfLeft( ex => LogException( "LoadTags", ex ));

            CurrentTag = previousTag != null ? 
                Tags.FirstOrDefault( t => t.Tag.Id.Equals( previousTag.Tag.Id )) : 
                Tags.FirstOrDefault();
        }

        private void OnTagChanged() {
            DeleteTag.RaiseCanExecuteChanged();

            LoadPresets();
        }

        private void LoadPresets() {
            TaggedPresets.Clear();

            if( mCurrentTag != null ) {
                mPresetProvider.SelectPresets( mCurrentTag.Tag, list => TaggedPresets.AddRange( from t in list orderby t.Name select new UiPreset( t, OnPresetEdit, OnPresetDelete )))
                    .IfLeft( ex => LogException( "LoadPresets", ex ));
            }

            mPresetController.LoadPresets( from p in TaggedPresets select p.Preset );
            CurrentPreset = TaggedPresets.FirstOrDefault();

            RaisePropertyChanged( () => PresetListTitle );
        }

        private void OnPresetEdit( Preset preset ) {
            mDialogService.ShowDialog( nameof( TagEditDialog ), new DialogParameters { { TagEditDialogModel.cPresetParameter, mCurrentPreset.Preset } }, OnPresetEditResult );
        }

        private void OnPresetEditResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var preset = result.Parameters.GetValue<Preset>( TagEditDialogModel.cPresetParameter );

                if( preset != null ) {
                    mPresetProvider.Update( preset ).IfLeft( ex => LogException( "OnTagsEdited", ex ));

                    LoadPresets();
                }
            }
        }

        private void OnPresetDelete( Preset preset ) {
            if(( preset != null ) &&
               ( mCurrentTag != null )) {
                preset = preset.WithoutTag( mCurrentTag.Tag );

                if( preset != null ) {
                    mPresetProvider.Update( preset ).IfLeft( ex => LogException( "OnPresetDelete (Tag)", ex ));

                    LoadPresets();
                }
            }
        }

        private void OnNewTag() {
            mDialogService.ShowDialog( nameof( NewTagDialog ), new DialogParameters(), OnNewTagResult );
        }

        private void OnNewTagResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var newTag = new PresetTag( result.Parameters.GetValue<string>( NewTagDialogModel.cTagNameParameter ));

                mTagProvider.Insert( newTag ).IfLeft( ex => LogException( "OnNewTagResult", ex ));

                LoadTags();
                CurrentTag = Tags.FirstOrDefault( t => t.Tag.Identity.Equals( newTag.Identity ));
            }
        }

        private void OnEditTag( UiTag tag ) {
            if( tag != null ) {
                mCurrentTag = tag;

                mDialogService.ShowDialog( nameof( NewTagDialog), new DialogParameters {{ NewTagDialogModel.cTagNameParameter, tag.Name }}, OnEditTagResult );
            }
        }

        private void OnEditTagResult( IDialogResult result ) {
            if(( result.Result == ButtonResult.OK ) &&
               ( mCurrentTag != null )) {
                var newName = result.Parameters.GetValue<string>( NewTagDialogModel.cTagNameParameter );

                if(!String.IsNullOrWhiteSpace( newName )) {
                    var tag = mCurrentTag.Tag.WithName( newName );

                    mTagProvider.Update( tag ).IfLeft( ex => LogException( "OnEditTagResult", ex ));

                    LoadTags();
                }
            }
        }

        private void OnDeleteTag( UiTag tag ) {
            if( tag != null ) {
                mCurrentTag = tag;

                TagDelete( tag.Tag );
            }
        }

        private void OnDeleteTag() {
            TagDelete( mCurrentTag?.Tag );
        }

        private void TagDelete( PresetTag tag ) {
            if( tag != null ) {
                mDialogService.ShowDialog( nameof( ConfirmDeleteDialog ), 
                    new DialogParameters {{ ConfirmDeleteDialogModel.cEntityNameParameter, tag.Name }}, 
                    OnTagDeleteResult );
            }
        }

        private void OnTagDeleteResult( IDialogResult result ) {
            if(( result.Result == ButtonResult.OK ) &&
               ( mCurrentTag != null )) {
                mTagProvider.Delete( mCurrentTag.Tag )
                    .Match( 
                        unit => LoadTags(), 
                        ex => LogException( "OnTagDeleteResult", ex ));
            }
        }

        private bool CanDeleteTag() {
            return mCurrentTag != null;
        }

        private void LogException( string message, Exception ex ) {
            mLog.LogException( message, ex );
        }
    }
}
