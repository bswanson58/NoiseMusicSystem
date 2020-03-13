using System;
using System.Collections.ObjectModel;
using System.Linq;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Views;
using MoreLinq.Extensions;
using Prism;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class SetEditViewModel : PropertyChangeBase, IActiveAware {
        private readonly IPresetSetProvider     mSetProvider;
        private readonly ITagProvider           mTagProvider;
        private readonly IDialogService         mDialogService;
        private readonly IPlatformLog           mLog;
        private PresetSet                       mCurrentSet;
        private bool                            mUseFavoriteQualifier;
        private bool                            mUseNameQualifier;
        private string                          mNameQualifier;
        private bool                            mUseTagQualifier;
        private bool                            mIsActive;

        public  ObservableCollection<PresetSet> Sets {  get; }
        public  ObservableCollection<UiTag>     Tags { get; }

        public  DelegateCommand                 CreateSet { get; }
        public  DelegateCommand                 DeleteSet { get; }

        public  string                          Title => "Sets";
        public  event EventHandler              IsActiveChanged = delegate { };

        public SetEditViewModel( IPresetSetProvider setProvider, ITagProvider tagProvider, IDialogService dialogService, IPlatformLog log ) {
            mSetProvider = setProvider;
            mTagProvider = tagProvider;
            mDialogService = dialogService;
            mLog = log;

            Sets = new ObservableCollection<PresetSet>();
            Tags = new ObservableCollection<UiTag>();

            CreateSet = new DelegateCommand( OnCreateSet );
            DeleteSet = new DelegateCommand( OnDeleteSet, CanDeleteSet );

            LoadSets();
            LoadTags();
        }

        public bool IsActive {
            get => mIsActive;
            set {
                mIsActive = value;

                if( mIsActive ) {
                    LoadTags();
                    LoadSets();
                }
            }
        }

        public PresetSet CurrentSet {
            get => mCurrentSet;
            set {
                mCurrentSet = value;

                OnSetChanged();
                RaisePropertyChanged( () => CurrentSet );
            }
        }

        private void LoadSets() {
            var currentSet = mCurrentSet;

            Sets.Clear();

            mSetProvider.SelectSets( list => Sets.AddRange( from s in list orderby s.Name select s ));

            if( currentSet != null ) {
                CurrentSet = Sets.FirstOrDefault( s => s.Id.Equals( currentSet.Id ));
            }
        }

        private void LoadTags() {
            Tags.Clear();

            mTagProvider.SelectTags( list => Tags.AddRange( from t in list orderby t.Name select new UiTag( t, OnTagSelected )));
        }

        private void OnSetChanged() {
            DisplaySetQualifiers();

            DeleteSet.RaiseCanExecuteChanged();
        }

        private void OnCreateSet() {
            mDialogService.ShowDialog( nameof( NewSetDialog ), new DialogParameters(), OnCreateSetResult  );
        }

        private void OnCreateSetResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var setName = result.Parameters.GetValue<string>( NewSetDialogModel.cSetNameParameter );

                if(!String.IsNullOrWhiteSpace( setName )) {
                    mSetProvider.Insert( new PresetSet( setName ))
                        .Match( 
                            unit => LoadSets(),
                            ex => LogException( "OnCreateSet", ex ));
                }
            }
        }

        private void OnDeleteSet() {
            if( mCurrentSet != null ) {
                mDialogService.ShowDialog( nameof( ConfirmDeleteDialog ), new DialogParameters( $"{ConfirmDeleteDialogModel.cEntityNameParameter}={mCurrentSet.Name}" ), OnDeleteSetResult );
            }
        }

        private void OnDeleteSetResult( IDialogResult result ) {
            if(( result.Result == ButtonResult.OK ) &&
               ( mCurrentSet != null )) {
                mSetProvider.Delete( mCurrentSet )
                    .Match( 
                        unit => LoadSets(), 
                        ex => LogException( "DeleteSet", ex ));
            }
        }

        private bool CanDeleteSet() {
            return mCurrentSet != null;
        }

        public bool UseFavoriteQualifier {
            get => mUseFavoriteQualifier;
            set {
                mUseFavoriteQualifier = value;

                OnFavoriteQualifierChanged();
                RaisePropertyChanged( () => UseFavoriteQualifier );
            }
        }

        private void OnFavoriteQualifierChanged() {
            if( mCurrentSet != null ) {
                var preset = mUseFavoriteQualifier ? 
                    mCurrentSet.WithQualifier( new SetQualifier( QualifierField.IsFavorite, QualifierOperation.Equal, true.ToString())) :
                    mCurrentSet.WithoutQualifier( QualifierField.IsFavorite );

                mSetProvider.Update( preset )
                    .Match(
                        unit => LoadSets(),
                        ex => LogException( "OnFavoriteQualifierChanged", ex )
                        );
            }
        }

        public bool UseNameQualifier {
            get => mUseNameQualifier;
            set {
                mUseNameQualifier = value;

                OnNameQualifierChanged();
                RaisePropertyChanged( () => UseNameQualifier );
            }
        }

        public string NameQualifier {
            get => mNameQualifier;
            set {
                mNameQualifier = value;

                OnNameQualifierChanged();
                RaisePropertyChanged( () => UseNameQualifier );
            }
        }

        private void OnNameQualifierChanged() {
            if(( mCurrentSet != null ) &&
               (!String.IsNullOrWhiteSpace( mNameQualifier ))) {
                var preset = mUseNameQualifier ?
                    mCurrentSet.WithQualifier( new SetQualifier( QualifierField.Name, QualifierOperation.Contains, mNameQualifier )) :
                    mCurrentSet.WithoutQualifier( QualifierField.Name );

                mSetProvider.Update( preset )
                    .Match(
                        unit => LoadSets(),
                        ex => LogException( "OnNameQualifierChanged", ex )
                    );
            }
        }

        public bool UseTagQualifier {
            get => mUseTagQualifier;
            set {
                mUseTagQualifier = value;

                OnTagQualifierChanged();
                RaisePropertyChanged( () => UseTagQualifier );
            }
        }

        private void OnTagSelected( UiTag tag ) {
            if( tag.IsSelected ) {
                mUseTagQualifier = true;

                RaisePropertyChanged( () => UseTagQualifier );
            }

            OnTagQualifierChanged();
        }

        private void OnTagQualifierChanged() {
            var setTags = Tags.Where( t => t.IsSelected );

            if( mCurrentSet != null ) {
                var preset = mUseTagQualifier ?
                    mCurrentSet.WithQualifier( new SetQualifier( QualifierField.Tags, QualifierOperation.HasMemberIdentity, from t in setTags select t.Tag.Identity )) :
                    mCurrentSet.WithoutQualifier( QualifierField.Tags );

                mSetProvider.Update( preset )
                    .Match(
                        unit => LoadSets(),
                        ex => LogException( "OnTagQualifierChanged", ex )
                    );
            }
        }

        private void DisplaySetQualifiers() {
            mUseFavoriteQualifier = false;
            mUseNameQualifier = false;
            mNameQualifier = String.Empty;
            mUseTagQualifier = false;

            mCurrentSet?.Qualifiers.ForEach( q => {
                switch( q.Field ) {
                    case QualifierField.IsFavorite:
                        mUseFavoriteQualifier = true;
                        break;

                    case QualifierField.Name:
                        mUseNameQualifier = true;
                        mNameQualifier = q.Value;
                        break;

                    case QualifierField.Tags:
                        ReconcileTags( q );
                        mUseTagQualifier = true;
                        break;
                }
            });

            RaisePropertyChanged( () => UseFavoriteQualifier );
            RaisePropertyChanged( () => UseNameQualifier );
            RaisePropertyChanged( () => NameQualifier );
            RaisePropertyChanged( () => UseTagQualifier );
        }

        private void ReconcileTags( SetQualifier q ) {
            Tags.ForEach( t => t.SetSelectedState( false ));

            if( q?.Value != null ) {
                var qualifierTags = q.Value.Split( SetQualifier.cValueSeparator );

                foreach( var tag in qualifierTags ) {
                    var uiTag = Tags.FirstOrDefault( t => t.Tag.Identity.Equals( tag ));

                    uiTag?.SetSelectedState( true );
                }
            }
        }

        private void LogException( string message, Exception ex ) {
            mLog.LogException( message, ex );
        }
    }
}
