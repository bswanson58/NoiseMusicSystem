using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class PresetControlViewModel : PropertyChangeBase, IDisposable, 
                                   IHandle<Events.PresetControllerInitialized>, IHandle<Events.PresetLibraryUpdated>, IHandle<Events.PresetLibrarySwitched> {
        private readonly IEventAggregator       mEventAggregator;
        private readonly IDialogService         mDialogService;
        private readonly IStateManager          mStateManager;
        private readonly IPresetController      mPresetController;
        private readonly IPresetLibrarian       mLibrarian;
        private IDisposable                     mPresetSubscription;
        private string                          mCurrentLibrary;

        public  DelegateCommand                 Start { get; }
        public  DelegateCommand                 Stop { get; }
        public  DelegateCommand                 NextPreset { get; }
        public  DelegateCommand                 PreviousPreset { get; }
        public  DelegateCommand                 SelectPreset { get; }

        public  string                          PresetName { get; set; }
        public  ObservableCollection<string>    Libraries { get; }

        public PresetControlViewModel( IStateManager stateManager, IPresetController presetController, IPresetLibrarian librarian,
                                       IDialogService dialogService, IEventAggregator eventAggregator ) {
            mStateManager = stateManager;
            mPresetController = presetController;
            mLibrarian = librarian;
            mEventAggregator = eventAggregator;
            mDialogService = dialogService;

            Libraries = new ObservableCollection<string>();
            mCurrentLibrary = String.Empty;

            Start = new DelegateCommand( OnStart );
            Stop = new DelegateCommand( OnStop );
            NextPreset = new DelegateCommand( OnNextPreset );
            PreviousPreset = new DelegateCommand( OnPreviousPreset );
            SelectPreset = new DelegateCommand( OnSelectPreset );

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

        private void OnSelectPreset() {
            var dialogParameters = new DialogParameters($"{SelectPresetDialogModel.cLibraryParameter}={CurrentLibrary}");

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
