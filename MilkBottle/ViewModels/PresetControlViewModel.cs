using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class PresetControlViewModel : PropertyChangeBase, IDisposable, 
                                   IHandle<Events.PresetControllerInitialized>, IHandle<Events.PresetLibraryUpdated>, IHandle<Events.PresetLibrarySwitched> {
        private readonly IEventAggregator       mEventAggregator;
        private readonly IStateManager          mStateManager;
        private readonly IPresetController      mPresetController;
        private readonly IPresetLibrarian       mLibrarian;
        private IDisposable                     mPresetSubscription;
        private string                          mCurrentLibrary;

        public  DelegateCommand                 Start { get; }
        public  DelegateCommand                 Stop { get; }
        public  DelegateCommand                 NextPreset { get; }
        public  DelegateCommand                 PreviousPreset { get; }

        public  string                          PresetName { get; set; }
        public  string                          PresetDurationToolTip { get; private set; }
        public  ObservableCollection<string>    Libraries { get; }

        public PresetControlViewModel( IStateManager stateManager, IPresetController presetController, IPresetLibrarian librarian, IEventAggregator eventAggregator ) {
            mStateManager = stateManager;
            mPresetController = presetController;
            mLibrarian = librarian;
            mEventAggregator = eventAggregator;

            Libraries = new ObservableCollection<string>();
            mCurrentLibrary = String.Empty;

            Start = new DelegateCommand( OnStart );
            Stop = new DelegateCommand( OnStop );
            NextPreset = new DelegateCommand( OnNextPreset );
            PreviousPreset = new DelegateCommand( OnPreviousPreset );

            if( mPresetController.IsInitialized ) {
                Initialize();
            }

            if( mLibrarian.IsInitialized ) {
                UpdateLibraries();
            }

            mPresetSubscription = mPresetController.CurrentPreset.Subscribe( OnPresetChanged );
            mEventAggregator.Subscribe( this );
        }

        private void Initialize() {
            SetDurationTooltip();

            mCurrentLibrary = mPresetController.CurrentPresetLibrary;

            RaisePropertyChanged( () => IsBlended );
            RaisePropertyChanged( () => IsLocked );
            RaisePropertyChanged( () => PresetDuration );
            RaisePropertyChanged( () => CurrentLibrary );
        }

        public void Handle( Events.PresetControllerInitialized args ) {
            Initialize();
        }

        public void Handle( Events.PresetLibraryUpdated args ) {
            UpdateLibraries();
        }

        public void Handle( Events.PresetLibrarySwitched  args ) {
            if( Libraries.Contains( args.LibraryName )) {
                mCurrentLibrary = args.LibraryName;

                RaisePropertyChanged( () => CurrentLibrary );
            }
        }

        public string CurrentLibrary {
            get => mCurrentLibrary;
            set {
                mCurrentLibrary = value;

                if(!String.IsNullOrWhiteSpace( mCurrentLibrary )) {
                    mPresetController.LoadLibrary( mCurrentLibrary );
                }

                RaisePropertyChanged( () => CurrentLibrary );
            }
        }

        private void UpdateLibraries() {
            Libraries.Clear();
            Libraries.AddRange( mLibrarian.AvailableLibraries );

            mCurrentLibrary = mPresetController.CurrentPresetLibrary;

            if(( String.IsNullOrWhiteSpace( mCurrentLibrary )) ||
               (!Libraries.Contains( mCurrentLibrary ))) {
                mCurrentLibrary = Libraries.FirstOrDefault();
            }

            RaisePropertyChanged( () => CurrentLibrary );
        }

        private void OnPresetChanged( MilkDropPreset preset ) {
            PresetName = Path.GetFileNameWithoutExtension( preset.PresetName );

            RaisePropertyChanged( () => PresetName );
        }

        public bool IsLocked {
            get => mStateManager.PresetControllerLocked;
            set => mStateManager.SetPresetLock( value );
        }

        public bool IsBlended {
            get => mPresetController.BlendPresetTransition;
            set => mPresetController.BlendPresetTransition = value;
        }

        public int PresetDuration {
            get => mPresetController.PresetDuration;
            set {
                mPresetController.PresetDuration = value;

                SetDurationTooltip();
                RaisePropertyChanged( () => PresetDuration );
            }
        }

        private void SetDurationTooltip() {
            PresetDurationToolTip = $"Duration is {mPresetController.PresetDuration} seconds";
            RaisePropertyChanged( () => PresetDurationToolTip );
        }

        private void OnStart() {
            mStateManager.EnterState( eStateTriggers.Run );
        }

        private void OnStop() {
            mStateManager.EnterState( eStateTriggers.Stop );
        }

        private void OnNextPreset() {
            mPresetController.SelectNextPreset();
        }

        private void OnPreviousPreset() {
            mPresetController.SelectPreviousPreset();
        }

        public void Dispose() {
            mEventAggregator.Unsubscribe( this );

            mPresetSubscription?.Dispose();
            mPresetSubscription = null;
        }
    }
}
