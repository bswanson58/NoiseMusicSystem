using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using MilkBottle.Support;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class PresetControlViewModel : PropertyChangeBase, IDisposable, 
                                   IHandle<Events.InitializationComplete>, IHandle<Events.PresetLibraryUpdated>, IHandle<Events.PresetLibrarySwitched> {
        private readonly IEventAggregator       mEventAggregator;
        private readonly IDialogService         mDialogService;
        private readonly IStateManager          mStateManager;
        private readonly IPresetController      mPresetController;
        private readonly IPresetLibrarian       mLibrarian;
        private readonly List<LibrarySet>       mLibraries;
        private readonly LimitedStack<string>   mHistory;
        private ICollectionView                 mLibrariesView;
        private IDisposable                     mPresetSubscription;
        private LibrarySet                      mCurrentLibrary;

        public  DelegateCommand                 Start { get; }
        public  DelegateCommand                 Stop { get; }
        public  DelegateCommand                 NextPreset { get; }
        public  DelegateCommand                 PreviousPreset { get; }
        public  DelegateCommand                 SelectPreset { get; }

        public  int                             PresetDurationMinimum => Types.PresetDuration.MinimumValue;
        public  int                             PresetDurationMaximum => Types.PresetDuration.MaximumValue;

        public  string                          PresetName { get; set; }
        public  string                          CurrentLibraryTooltip => mCurrentLibrary != null ? $"{mCurrentLibrary.PresetCount} presets in library" : String.Empty;
        public  string                          PresetHistory => "History:" + Environment.NewLine + " " + String.Join( Environment.NewLine + " ", mHistory.ToList().Skip( 1 ));

        public PresetControlViewModel( IStateManager stateManager, IPresetController presetController, IPresetLibrarian librarian,
                                       IDialogService dialogService, IEventAggregator eventAggregator ) {
            mStateManager = stateManager;
            mPresetController = presetController;
            mLibrarian = librarian;
            mEventAggregator = eventAggregator;
            mDialogService = dialogService;

            mLibraries = new List<LibrarySet>();
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

            if( mLibrarian.IsInitialized ) {
                UpdateLibraries();
            }

            mEventAggregator.Subscribe( this );
        }

        public void Handle( Events.InitializationComplete args ) {
            Initialize();
        }

        private void Initialize() {
            RaisePropertyChanged( () => IsBlended );
            RaisePropertyChanged( () => IsLocked );
            RaisePropertyChanged( () => PresetDuration );
            RaisePropertyChanged( () => CurrentLibrary );

            Stop.RaiseCanExecuteChanged();
            Start.RaiseCanExecuteChanged();

            mPresetController.ConfigurePresetTimer( PresetTimer.FixedDuration );
            mPresetController.ConfigurePresetSequencer( PresetSequence.Random );
        }

        public void Handle( Events.PresetLibraryUpdated args ) {
            UpdateLibraries();
        }

        public void Handle( Events.PresetLibrarySwitched  args ) {
            mCurrentLibrary = mLibraries.FirstOrDefault( l => l.LibraryName.Equals( args.LibraryName ));

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

        public LibrarySet CurrentLibrary {
            get => mCurrentLibrary;
            set {
                mCurrentLibrary = value;

                if( mCurrentLibrary != null ) {
                    mPresetController.LoadLibrary( mCurrentLibrary.LibraryName );
                }

                RaisePropertyChanged( () => CurrentLibrary );
                RaisePropertyChanged( () => CurrentLibraryTooltip );
            }
        }

        private void UpdateLibraries() {
            mLibraries.Clear();
            mLibraries.AddRange( mLibrarian.PresetLibraries );

            mCurrentLibrary = mLibraries.FirstOrDefault( l => l.LibraryName.Equals( mPresetController.CurrentPresetLibrary )) ??
                              mLibraries.FirstOrDefault();

            RaisePropertyChanged( () => CurrentLibrary );
        }

        private void OnPresetChanged( MilkDropPreset preset ) {
            if( preset != null ) {
                PresetName = Path.GetFileNameWithoutExtension( preset.PresetName );

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
            var dialogParameters = new DialogParameters($"{SelectPresetDialogModel.cLibraryParameter}={CurrentLibrary.LibraryName}");

            mDialogService.ShowDialog( "SelectPresetDialog", dialogParameters, OnPresetSelected );
        }

        private void OnPresetSelected( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var preset = result.Parameters.GetValue<MilkDropPreset>( SelectPresetDialogModel.cPresetParameter );

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
