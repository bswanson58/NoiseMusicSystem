using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;
using MilkBottle.Support;
using MilkBottle.Types;
using MilkBottle.Views;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Platform;

namespace MilkBottle.ViewModels {
    class PresetControlViewModel : PropertyChangeBase, IDisposable,
                                   IHandle<Events.InitializationComplete>, IHandle<Events.PresetLibraryUpdated>, IHandle<Events.PresetLibrarySwitched>, IHandle<Events.ModeChanged> {
        private readonly IEventAggregator       mEventAggregator;
        private readonly IDialogService         mDialogService;
        private readonly IStateManager          mStateManager;
        private readonly IPresetController      mPresetController;
        private readonly IPresetProvider        mPresetProvider;
        private readonly IPresetListProvider    mListProvider;
        private readonly IIpcManager            mIpcManager;
        private readonly List<PresetList>       mLibraries;
        private readonly IPreferences           mPreferences;
        private readonly IPlatformLog           mLog;
        private readonly LimitedStack<string>   mHistory;
        private ICollectionView                 mLibrariesView;
        private IDisposable                     mPresetSubscription;
        private IDisposable                     mPlaybackSubscription;
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
        public  string                          CurrentLibraryTooltip => $"{mPresetController.CurrentPresetCount} presets in library";
        public  string                          PresetHistory => "History:" + Environment.NewLine + " " + String.Join( Environment.NewLine + " ", mHistory.ToList().Skip( 1 ));
        public  bool                            HasTags => mCurrentPreset?.Tags.Any() ?? false;

        public  string                          PlaybackTitle { get; private set; }

        public PresetControlViewModel( IStateManager stateManager, IPresetController presetController, IPresetListProvider listProvider, IPresetProvider presetProvider,
                                       IIpcManager ipcManager, IPreferences preferences, IDialogService dialogService, IEventAggregator eventAggregator, IPlatformLog log ) {
            mStateManager = stateManager;
            mPresetController = presetController;
            mPresetProvider = presetProvider;
            mListProvider = listProvider;
            mIpcManager = ipcManager;
            mEventAggregator = eventAggregator;
            mDialogService = dialogService;
            mPreferences = preferences;
            mLog = log;

            mLibraries = new List<PresetList>();
            mHistory = new LimitedStack<string>( 4 );

            Start = new DelegateCommand( OnStart, CanStart );
            Stop = new DelegateCommand( OnStop, CanStop );
            NextPreset = new DelegateCommand( OnNextPreset );
            PreviousPreset = new DelegateCommand( OnPreviousPreset );
            SelectPreset = new DelegateCommand( OnSelectPreset );
            EditTags = new DelegateCommand( OnTagEdit );

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

            mPresetSubscription = mPresetController.CurrentPreset.Subscribe( OnPresetChanged );
            mPlaybackSubscription = mIpcManager.OnPlaybackEvent.Subscribe( OnPlaybackEvent );

            RaisePropertyChanged( () => IsBlended );
            RaisePropertyChanged( () => IsLocked );
            RaisePropertyChanged( () => PresetDuration );
            RaisePropertyChanged( () => CurrentLibrary );

            Stop.RaiseCanExecuteChanged();
            Start.RaiseCanExecuteChanged();

            mStateManager.EnterState( eStateTriggers.Run );
        }

        private void OnPlaybackEvent( PlaybackEvent args ) {
            PlaybackTitle = args.IsValidEvent ? $"{args.ArtistName}/{args.TrackName}" : String.Empty;

            RaisePropertyChanged( () => PlaybackTitle );
        }

        public void Handle( Events.ModeChanged args ) {
            if( args.ToView != ShellView.Manual ) {
                mEventAggregator.Unsubscribe( this );

                mPresetSubscription?.Dispose();
                mPresetSubscription = null;

                mPlaybackSubscription?.Dispose();
                mPlaybackSubscription = null;
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
                    mLibrariesView.SortDescriptions.Add( new SortDescription( "Name", ListSortDirection.Ascending ));
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

        private async void OnIsFavoriteChanged( bool toValue ) {
            var preset = mCurrentPreset?.WithFavorite( toValue );

            if( preset != null ) {
                if( preset.Id.Equals( mCurrentPreset?.Id )) {
                    mCurrentPreset = preset;

                    RaisePropertyChanged( () => IsFavorite );
                }

                ( await mPresetProvider.UpdateAll( preset )).IfLeft( ex => LogException( "OnIsFavoriteChanged.Update", ex ));
            }
        }

        public bool DoNotPlay {
            get => mCurrentPreset != null && mCurrentPreset.Rating == PresetRating.DoNotPlayValue;
            set => OnDoNotPlayChanged( value );
        }
         
        private async void OnDoNotPlayChanged( bool toValue ) {
            var preset = mCurrentPreset?.WithRating( toValue ? PresetRating.DoNotPlayValue : PresetRating.UnRatedValue );

            if( preset != null ) {
                if( preset.Id.Equals( mCurrentPreset?.Id )) {
                    mCurrentPreset = preset;

                    RaisePropertyChanged( () => DoNotPlay );
                }

                ( await mPresetProvider.UpdateAll( preset )).IfLeft( ex => LogException( "OnDoNotPlayChanged.Update", ex ));
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
            set {
                var  preferences = mPreferences.Load<MilkPreferences>();

                mPresetController.BlendPresetTransition = value;
                preferences.BlendPresetTransition = value;

                mPreferences.Save( preferences );
            }
        }

        public int PresetDuration {
            get => mPresetController.PresetDuration;
            set {
                mPresetController.PresetDuration = value;

                RaisePropertyChanged( () => PresetDuration );
            }
        }

        private void OnTagEdit() {
            if( mCurrentPreset != null ) {
                var parameters = new DialogParameters { { TagEditDialogModel.cPresetParameter, mCurrentPreset } };

                mDialogService.ShowDialog( nameof( TagEditDialog ), parameters, OnTagsEdited );
            }
        }

        private async void OnTagsEdited( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var preset = result.Parameters.GetValue<Preset>( TagEditDialogModel.cPresetParameter );

                if( preset != null ) {
                    if( preset.Id.Equals( mCurrentPreset?.Id )) {
                        mCurrentPreset = preset;

                        RaisePropertyChanged( () => IsFavorite );
                        RaisePropertyChanged( () => HasTags );
                        RaisePropertyChanged( () => TagsTooltip );
                    }

                    ( await mPresetProvider.UpdateAll( preset )).IfLeft( ex => LogException( "OnTagsEdited", ex ));
                }
            }
        }

        public string TagsTooltip => 
            mCurrentPreset != null ? 
                mCurrentPreset.Tags.Any() ? 
                    String.Join( Environment.NewLine, from t in mCurrentPreset.Tags orderby t.Name select t.Name ) : "Set Preset Tags" : "Set Preset Tags";

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

        private void LogException( string message, Exception ex ) {
            mLog.LogException( message, ex );
        }

        public void Dispose() {
            mEventAggregator.Unsubscribe( this );

            mPresetSubscription?.Dispose();
            mPresetSubscription = null;

            mPlaybackSubscription?.Dispose();
            mPlaybackSubscription = null;
        }
    }
}
