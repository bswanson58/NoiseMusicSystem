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
        private readonly IPlatformLog           mLog;
        private UiTag                           mCurrentTag;
        private bool                            mIsActive;

        public  ObservableCollection<UiTag>     Tags { get; }
        public  ObservableCollection<Preset>    TaggedPresets {  get; }

        public  DelegateCommand                 NewTag { get; }
        public  DelegateCommand                 DeleteTag {  get; }

        public  string                          Title => "Tags";
        public  event EventHandler              IsActiveChanged = delegate { };

        public TagEditViewModel( ITagProvider tagProvider, IDialogService dialogService, IPlatformLog log ) {
            mTagProvider = tagProvider;
            mDialogService = dialogService;
            mLog = log;

            Tags = new ObservableCollection<UiTag>();
            TaggedPresets = new ObservableCollection<Preset>();

            NewTag = new DelegateCommand( OnNewTag );
            DeleteTag = new DelegateCommand( OnDeleteTag, CanDeleteTag );

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

        private void LoadTags() {
            Tags.Clear();

            mTagProvider.SelectTags( list => Tags.AddRange( from t in list orderby t.Name select new UiTag(  t, null, OnEditTag, OnDeleteTag )));
        }

        private void OnTagChanged() {
            DeleteTag.RaiseCanExecuteChanged();
        }

        private void OnNewTag() {
            mDialogService.ShowDialog( "NewTagDialog", null, OnNewTagResult );
        }

        private void OnNewTagResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var newTag = new PresetTag( result.Parameters.GetValue<string>( NewTagDialogModel.cTagNameParameter ));

                mTagProvider.Insert( newTag ).IfLeft( ex => LogException( "OnNewTagResult", ex ));

                LoadTags();
                CurrentTag = Tags.FirstOrDefault( t => t.Tag.Identity.Equals( newTag.Identity ));
            }
        }

        private void OnEditTag( UiTag tag ) { }

        private void OnDeleteTag( UiTag tag ) {
            TagDelete( tag.Tag );
        }

        private void OnDeleteTag() {
            TagDelete( mCurrentTag?.Tag );
        }

        private void TagDelete( PresetTag tag ) {
            if( tag != null ) {
                mDialogService.ShowDialog( nameof( ConfirmDeleteDialog ), 
                    new DialogParameters( $"{ConfirmDeleteDialogModel.cEntityNameParameter}={tag.Name}" ), 
                    OnTagDeleteResult );
            }
        }

        private void OnTagDeleteResult( IDialogResult result ) {
            if(( result.Result == ButtonResult.OK ) &&
               ( mCurrentTag != null )) {
                mTagProvider.Delete( mCurrentTag.Tag )
                    .Match( 
                        unit => LoadTags(), 
                        ex => LogException( "DeleteTag", ex ));
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
