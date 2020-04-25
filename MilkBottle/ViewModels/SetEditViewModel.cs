using System;
using System.Collections.ObjectModel;
using System.Linq;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Views;
using MoreLinq;
using Prism;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class SetEditViewModel : PropertyChangeBase, IActiveAware {
        private readonly IPresetSetProvider     mSetProvider;
        private readonly ITagProvider           mTagProvider;
        private readonly IPresetProvider        mPresetProvider;
        private readonly IPresetListProvider    mPresetListProvider;
        private readonly IPresetController      mPresetController;
        private readonly IStateManager          mStateManager;
        private readonly IDialogService         mDialogService;
        private readonly IPlatformLog           mLog;
        private UiSet                           mCurrentSet;
        private UiPreset                        mCurrentPreset;
        private bool                            mUseFavoriteQualifier;
        private bool                            mUseNameQualifier;
        private string                          mNameQualifier;
        private bool                            mUseTagQualifier;
        private bool                            mIsActive;

        public  ObservableCollection<UiSet>     Sets {  get; }
        public  ObservableCollection<UiTag>     Tags { get; }
        public  ObservableCollection<UiPreset>  Presets { get; }

        public  DelegateCommand                 CreateSet { get; }
        public  DelegateCommand                 DeleteSet { get; }
        public  DelegateCommand                 CreateTag { get; }

        public  string                          Title => "Sets";
        public  string                          PresetListTitle => Presets.Any() ? $"({Presets.Count}) Presets In Set " : " Presets In Set ";
        public  bool                            IsSetSelected => mCurrentSet != null;
        public  event EventHandler              IsActiveChanged = delegate { };

        public SetEditViewModel( IPresetSetProvider setProvider, ITagProvider tagProvider, IPresetProvider presetProvider, IPresetListProvider listProvider,
                                 IPresetController presetController, IStateManager stateManager, IDialogService dialogService, IPlatformLog log ) {
            mSetProvider = setProvider;
            mTagProvider = tagProvider;
            mPresetListProvider = listProvider;
            mPresetProvider = presetProvider;
            mPresetController = presetController;
            mStateManager = stateManager;
            mDialogService = dialogService;
            mLog = log;

            Sets = new ObservableCollection<UiSet>();
            Tags = new ObservableCollection<UiTag>();
            Presets = new ObservableCollection<UiPreset>();

            CreateSet = new DelegateCommand( OnCreateSet );
            DeleteSet = new DelegateCommand( OnDeleteSet, CanDeleteSet );
            CreateTag = new DelegateCommand( OnCreateTag );

            mPresetController.BlendPresetTransition = false;
            mPresetController.ConfigurePresetSequencer( PresetSequence.Sequential );
            mPresetController.ConfigurePresetTimer( PresetTimer.Infinite );
            mStateManager.EnterState( eStateTriggers.Stop );

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

        public UiSet CurrentSet {
            get => mCurrentSet;
            set {
                mCurrentSet = value;

                OnSetChanged();
                RaisePropertyChanged( () => CurrentSet );
                RaisePropertyChanged( () => IsSetSelected );
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

        private void LoadSets() {
            var currentSet = mCurrentSet;

            Sets.Clear();

            mSetProvider.SelectSets( list => Sets.AddRange( from s in list orderby s.Name select new UiSet( s, OnEditSet, OnDeleteSet )));

            CurrentSet = currentSet != null ? Sets.FirstOrDefault( s => s.Set.Id.Equals( currentSet.Set.Id )) : 
                                              Sets.FirstOrDefault();
        }

        private void LoadTags() {
            Tags.Clear();

            mTagProvider.SelectTags( list => Tags.AddRange( from t in list orderby t.Name select new UiTag( t, OnTagSelected, null, null )));
        }

        private void LoadPresets() {
            Presets.Clear();

            if( mCurrentSet != null ) {
                var presets = mPresetListProvider.GetPresets( mCurrentSet.Set );

                Presets.AddRange( from p in presets orderby p.Name select new UiPreset( p, OnPresetEdit, null ));
            }

            mPresetController.LoadPresets( from p in Presets select p.Preset );
            CurrentPreset = Presets.FirstOrDefault();

            RaisePropertyChanged( () => PresetListTitle );
        }

        private void OnPresetEdit( Preset preset ) {
            mDialogService.ShowDialog( nameof( TagEditDialog ), new DialogParameters { { TagEditDialogModel.cPresetParameter, mCurrentPreset.Preset } }, OnPresetEditResult );
        }

        private async void OnPresetEditResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var preset = result.Parameters.GetValue<Preset>( TagEditDialogModel.cPresetParameter );

                if( preset != null ) {
                    ( await mPresetProvider.UpdateAll( preset )).IfLeft( ex => LogException( "OnTagsEdited", ex ));

                    LoadPresets();
                }
            }
        }

        private void OnSetChanged() {
            DisplaySetQualifiers();
            LoadPresets();

            DeleteSet.RaiseCanExecuteChanged();
        }

        private void OnCreateTag() {
            mDialogService.ShowDialog( nameof( NewTagDialog ), new DialogParameters(), OnCreateTagResult );
        }

        private void OnCreateTagResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var tagName = result.Parameters.GetValue<string>( NewTagDialogModel.cTagNameParameter );

                if(!String.IsNullOrWhiteSpace( tagName )) {
                    mTagProvider.Insert( new PresetTag( tagName )).IfLeft( ex => LogException( "OnCreateTagResult", ex ));
                }
            }
        }

        private void OnCreateSet() {
            mDialogService.ShowDialog( nameof( NewSetDialog ), new DialogParameters(), OnCreateSetResult  );
        }

        private void OnCreateSetResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var setName = result.Parameters.GetValue<string>( NewSetDialogModel.cSetNameParameter );

                if(!String.IsNullOrWhiteSpace( setName )) {
                    var newSet = new PresetSet( setName );

                    mSetProvider.Insert( newSet ).IfLeft( ex => LogException( "OnCreateSetResult", ex ));

                    LoadSets();
                    CurrentSet = Sets.FirstOrDefault( s => s.Set.Id.Equals( newSet.Id ));
                }
            }
        }

        private void OnEditSet( UiSet set ) {
            mCurrentSet = set;

            if( set != null ) {
                mDialogService.ShowDialog( nameof( NewSetDialog ), new DialogParameters{{ NewSetDialogModel.cSetNameParameter, set.Name }}, OnEditSetResult  );
            }
        }

        private void OnEditSetResult( IDialogResult result ) {
            if(( result.Result == ButtonResult.OK ) &&
               ( mCurrentSet != null )) {
                var newName = result.Parameters.GetValue<string>( NewSetDialogModel.cSetNameParameter );

                if(!String.IsNullOrWhiteSpace( newName )) {
                    var set = mCurrentSet.Set.WithName( newName );

                    mSetProvider.Update( set ).IfLeft( ex => LogException( "OnEditSetResult", ex ));

                    LoadSets();
                }
            }
        }

        private void OnDeleteSet( UiSet set ) {
            mCurrentSet = set;

            OnDeleteSet( mCurrentSet?.Set );
        }

        private void OnDeleteSet() {
            OnDeleteSet( mCurrentSet?.Set );
        }

        private void OnDeleteSet( PresetSet set ) {
            if( set != null ) {
                mDialogService.ShowDialog( nameof( ConfirmDeleteDialog ), new DialogParameters{{ ConfirmDeleteDialogModel.cEntityNameParameter, set.Name} }, OnDeleteSetResult );
            }
        }

        private void OnDeleteSetResult( IDialogResult result ) {
            if(( result.Result == ButtonResult.OK ) &&
               ( mCurrentSet != null )) {
                mSetProvider.Delete( mCurrentSet.Set )
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
                    mCurrentSet.Set.WithQualifier( new SetQualifier( QualifierField.IsFavorite, QualifierOperation.Equal, true.ToString())) :
                    mCurrentSet.Set.WithoutQualifier( QualifierField.IsFavorite );

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
                    mCurrentSet.Set.WithQualifier( new SetQualifier( QualifierField.Name, QualifierOperation.Contains, mNameQualifier )) :
                    mCurrentSet.Set.WithoutQualifier( QualifierField.Name );

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
                    mCurrentSet.Set.WithQualifier( new SetQualifier( QualifierField.Tags, QualifierOperation.HasMemberIdentity, from t in setTags select t.Tag.Identity )) :
                    mCurrentSet.Set.WithoutQualifier( QualifierField.Tags );

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

            Tags.ForEach( t => t.SetSelectedState( false ));

            mCurrentSet?.Set.Qualifiers.ForEach( q => {
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
