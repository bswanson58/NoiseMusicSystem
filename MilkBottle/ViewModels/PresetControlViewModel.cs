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
        private readonly IPresetLibraryProvider mLibraryProvider;
        private readonly List<PresetLibrary>    mLibraries;
        private readonly IPreferences           mPreferences;
        private readonly LimitedStack<string>   mHistory;
        private ICollectionView                 mLibrariesView;
        private IDisposable                     mPresetSubscription;
        private PresetLibrary                   mCurrentLibrary;

        public  DelegateCommand                 Start { get; }
        public  DelegateCommand                 Stop { get; }
        public  DelegateCommand                 NextPreset { get; }
        public  DelegateCommand                 PreviousPreset { get; }
        public  DelegateCommand                 SelectPreset { get; }

        public  int                             PresetDurationMinimum => Types.PresetDuration.MinimumValue;
        public  int                             PresetDurationMaximum => Types.PresetDuration.MaximumValue;

        public  string                          PresetName { get; set; }
        public  string                          CurrentLibraryTooltip => null; // mCurrentLibrary != null ? $"{mCurrentLibrary.PresetCount} presets in library" : String.Empty;
        public  string                          PresetHistory => "History:" + Environment.NewLine + " " + String.Join( Environment.NewLine + " ", mHistory.ToList().Skip( 1 ));

        public PresetControlViewModel( IStateManager stateManager, IPresetController presetController, IPresetLibraryProvider libraryProvider,
                                       IPreferences preferences, IDialogService dialogService, IEventAggregator eventAggregator ) {
            mStateManager = stateManager;
            mPresetController = presetController;
            mLibraryProvider = libraryProvider;
            mEventAggregator = eventAggregator;
            mDialogService = dialogService;
            mPreferences = preferences;

            mLibraries = new List<PresetLibrary>();
            mHistory = new LimitedStack<string>( 4 );

            Start = new DelegateCommand( OnStart, CanStart );
            Stop = new DelegateCommand( OnStop, CanStop );
            NextPreset = new DelegateCommand( OnNextPreset );
            PreviousPreset = new DelegateCommand( OnPreviousPreset );
            SelectPreset = new DelegateCommand( OnSelectPreset );

            mPresetSubscription = mPresetController.CurrentPreset.Subscribe( OnPresetChanged );

            if( mPresetController.IsInitialized ) {
                Initialize();
            }
            
            UpdateLibraries();

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

        public PresetLibrary CurrentLibrary {
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

            mLibraryProvider.SelectLibraries( list => mLibraries.AddRange( from l in list orderby l.Name select l ));

            mCurrentLibrary = mLibraries.FirstOrDefault( l => l.Name.Equals( mPresetController.CurrentPresetLibrary )) ??
                              mLibraries.FirstOrDefault();

            RaisePropertyChanged( () => CurrentLibrary );
        }

        private void OnPresetChanged( Preset preset ) {
            if( preset != null ) {
                PresetName = Path.GetFileNameWithoutExtension( preset.Name );

                mHistory.Push( PresetName );

                RaisePropertyChanged( () => PresetName );
                RaisePropertyChanged( () => PresetHistory );
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
