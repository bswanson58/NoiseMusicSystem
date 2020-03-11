using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Support;
using MilkBottle.Types;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class PresetControlViewModel : PropertyChangeBase, IDisposable, 
                                   IHandle<Events.InitializationComplete>, IHandle<Events.PresetLibraryUpdated>, IHandle<Events.PresetLibrarySwitched>, IHandle<Events.ModeChanged> {
        private readonly IEventAggregator       mEventAggregator;
        private readonly IDialogService         mDialogService;
        private readonly IStateManager          mStateManager;
        private readonly IPresetController      mPresetController;
        private readonly IPresetProvider        mPresetProvider;
        private readonly IPresetListProvider    mListProvider;
        private readonly List<PresetList>       mLibraries;
        private readonly IPreferences           mPreferences;
        private readonly LimitedStack<string>   mHistory;
        private ICollectionView                 mLibrariesView;
        private IDisposable                     mPresetSubscription;
        private PresetList                      mCurrentLibrary;
        private Preset                          mCurrentPreset;

        public  DelegateCommand                 Start { get; }
        public  DelegateCommand                 Stop { get; }
        public  DelegateCommand                 NextPreset { get; }
        public  DelegateCommand                 PreviousPreset { get; }
        public  DelegateCommand                 SelectPreset { get; }
        public  DelegateCommand                 EditTags { get; }

        public  int                             PresetDurationMinimum => Types.PresetDuration.MinimumValue;
        public  int                             PresetDurationMaximum => Types.PresetDuration.MaximumValue;

        public  string                          PresetName { get; set; }
        public  string                          CurrentLibraryTooltip => null; // mCurrentLibrary != null ? $"{mCurrentLibrary.PresetCount} presets in library" : String.Empty;
        public  string                          PresetHistory => "History:" + Environment.NewLine + " " + String.Join( Environment.NewLine + " ", mHistory.ToList().Skip( 1 ));
        public  bool                            HasTags => mCurrentPreset?.Tags.Any() ?? false;

        public PresetControlViewModel( IStateManager stateManager, IPresetController presetController, IPresetListProvider listProvider, IPresetProvider presetProvider,
                                       IPreferences preferences, IDialogService dialogService, IEventAggregator eventAggregator ) {
            mStateManager = stateManager;
            mPresetController = presetController;
            mPresetProvider = presetProvider;
            mListProvider = listProvider;
            mEventAggregator = eventAggregator;
            mDialogService = dialogService;
            mPreferences = preferences;

            mLibraries = new List<PresetList>();
            mHistory = new LimitedStack<string>( 4 );

            Start = new DelegateCommand( OnStart, CanStart );
            Stop = new DelegateCommand( OnStop, CanStop );
            NextPreset = new DelegateCommand( OnNextPreset );
            PreviousPreset = new DelegateCommand( OnPreviousPreset );
            SelectPreset = new DelegateCommand( OnSelectPreset );
            EditTags = new DelegateCommand( OnTagEdit );

            mPresetSubscription = mPresetController.CurrentPreset.Subscribe( OnPresetChanged );

            UpdateLibraries();

            if( mPresetController.IsInitialized ) {
                Initialize();
            }

            mEventAggregator.Subscribe( this );
        }

        public void Handle( Events.InitializationComplete args ) {
            Initialize();
        }

        private void Initialize() {
            var preferences = mPreferences.Load<MilkPreferences>();

            mPresetController.BlendPresetTransition = preferences.BlendPresetTransition;
            mPresetController.ConfigurePresetTimer( PresetTimer.FixedDuration );
            mPresetController.ConfigurePresetSequencer( PresetSequence.Random );

            CurrentLibrary = mLibraries.FirstOrDefault( l => l.Name.Equals( preferences.CurrentPresetLibrary ));

            RaisePropertyChanged( () => IsBlended );
            RaisePropertyChanged( () => IsLocked );
            RaisePropertyChanged( () => PresetDuration );
            RaisePropertyChanged( () => CurrentLibrary );

            Stop.RaiseCanExecuteChanged();
            Start.RaiseCanExecuteChanged();

            mStateManager.EnterState( eStateTriggers.Run );
        }

        public void Handle( Events.ModeChanged args ) {
            if( args.ToView != ShellView.Manual ) {
                mEventAggregator.Unsubscribe( this );

                mPresetSubscription?.Dispose();
                mPresetSubscription = null;
            }
        }

        public void Handle( Events.PresetLibraryUpdated args ) {
            UpdateLibraries();
        }

        public void Handle( Events.PresetLibrarySwitched  args ) {
            mCurrentLibrary = mLibraries.FirstOrDefault( l => l.Name.Equals( args.LibraryName ));

            RaisePropertyChanged( () => CurrentLibrary );
        }

        public ICollectionView Libraries {
            get {
                if( mLibrariesView == null ) {
                    mLibrariesView = CollectionViewSource.GetDefaultView( mLibraries );
                    mLibrariesView.SortDescriptions.Add( new SortDescription( "LibraryName", ListSortDirection.Ascending ));
                }

                return mLibrariesView;
            }
        }

        public PresetList CurrentLibrary {
            get => mCurrentLibrary;
            set {
                mCurrentLibrary = value;

                if( mCurrentLibrary != null ) {
                    mPresetController.LoadLibrary( mCurrentLibrary );
                }

                RaisePropertyChanged( () => CurrentLibrary );
                RaisePropertyChanged( () => CurrentLibraryTooltip );
            }
        }

        private void UpdateLibraries() {
            mLibraries.Clear();

            mLibraries.AddRange( mListProvider.GetLists());

            mCurrentLibrary = mLibraries.FirstOrDefault( l => l.Name.Equals( mPresetController.CurrentPresetLibrary )) ??
                              mLibraries.FirstOrDefault();

            RaisePropertyChanged( () => CurrentLibrary );
        }

        private void OnPresetChanged( Preset preset ) {
            mCurrentPreset = preset;

            if( preset != null ) {
                PresetName = Path.GetFileNameWithoutExtension( preset.Name );

                mHistory.Push( PresetName );

                RaisePropertyChanged( () => PresetName );
                RaisePropertyChanged( () => PresetHistory );
            }

            RaisePropertyChanged( () => IsFavorite );
            RaisePropertyChanged( () => DoNotPlay );
            RaisePropertyChanged( () => HasTags );
            RaisePropertyChanged( () => TagsTooltip );
        }

        public bool IsFavorite {
            get => mCurrentPreset?.IsFavorite ?? false;
            set => OnIsFavoriteChanged( value );
        }

        private void OnIsFavoriteChanged( bool toValue ) {
            var preset = mCurrentPreset?.WithFavorite( toValue );

            if( preset != null ) {
                mPresetProvider.Update( preset );
            }
        }

        public bool DoNotPlay {
            get => mCurrentPreset != null && mCurrentPreset.Rating == PresetRating.DoNotPlayValue;
            set => OnDoNotPlayChanged( value );
        }
         
        private void OnDoNotPlayChanged( bool toValue ) {
            var preset = mCurrentPreset?.WithRating( toValue ? PresetRating.DoNotPlayValue : PresetRating.MinimumValue );

            if( preset != null ) {
                mPresetProvider.Update( preset );
            }
        }

        public bool IsLocked {
            get => mStateManager.PresetControllerLocked;
            set {
                mStateManager.SetPresetLock( value );

                RaisePropertyChanged( () => IsLocked );
            }
        }

        public bool IsBlended {
            get => mPresetController.BlendPresetTransition;
            set => mPresetController.BlendPresetTransition = value;
        }

        public int PresetDuration {
            get => mPresetController.PresetDuration;
            set {
                mPresetController.PresetDuration = value;

                RaisePropertyChanged( () => PresetDuration );
            }
        }

        private void OnTagEdit() {
            var parameters = new DialogParameters { { TagEditDialogModel.cPresetParameter, mCurrentPreset } };

            mDialogService.ShowDialog( "TagEditDialog", parameters, OnTagsEdited );
        }

        private void OnTagsEdited( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var preset = result.Parameters.GetValue<Preset>( TagEditDialogModel.cPresetParameter );

                if( preset != null ) {
                    mPresetProvider.Update( preset );
                }
            }
        }

        public string TagsTooltip => 
            mCurrentPreset != null ? 
                mCurrentPreset.Tags.Any() ? 
                    String.Join( Environment.NewLine, from t in mCurrentPreset.Tags select t.Name ) : "Set Preset Tags" : "Set Preset Tags";

        private void OnStart() {
            mStateManager.EnterState( eStateTriggers.Run );

            Stop.RaiseCanExecuteChanged();
            Start.RaiseCanExecuteChanged();
        }

        private bool CanStart() {
            return !mStateManager.IsRunning;
        }

        private void OnStop() {
            mStateManager.EnterState( eStateTriggers.Stop );

            Stop.RaiseCanExecuteChanged();
            Start.RaiseCanExecuteChanged();
        }

        private bool CanStop() {
            return mStateManager.IsRunning;
        }

        private void OnNextPreset() {
            mPresetController.SelectNextPreset();
        }

        private void OnPreviousPreset() {
            mPresetController.SelectPreviousPreset();
        }

        private void OnSelectPreset() {
            var dialogParameters = new DialogParameters($"{SelectPresetDialogModel.cLibraryParameter}={CurrentLibrary.Name}");

            mDialogService.ShowDialog( "SelectPresetDialog", dialogParameters, OnPresetSelected );
        }

        private void OnPresetSelected( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var preset = result.Parameters.GetValue<Preset>( SelectPresetDialogModel.cPresetParameter );

                if( preset != null ) {
                    mPresetController.PlayPreset( preset );

                    IsLocked = true;
                }
            }
        }

        public void Dispose() {
            mEventAggregator.Unsubscribe( this );

            mPresetSubscription?.Dispose();
            mPresetSubscription = null;
        }
    }
}
