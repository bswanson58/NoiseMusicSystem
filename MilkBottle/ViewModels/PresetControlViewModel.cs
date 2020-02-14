using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class PresetControlViewModel : PropertyChangeBase, IDisposable, 
                                   IHandle<Events.PresetLibraryUpdated>, IHandle<Events.PresetLibrarySwitched> {
        private readonly IEventAggregator       mEventAggregator;
        private readonly IMilkController        mMilkController;
        private readonly IPresetController      mPresetController;
        private readonly IPresetLibrarian       mLibrarian;
        private IDisposable                     mPresetSubscription;
        private string                          mCurrentLibrary;

        public  DelegateCommand                 Start { get; }
        public  DelegateCommand                 Stop { get; }
        public  DelegateCommand                 NextPreset { get; }
        public  DelegateCommand                 PreviousPreset { get; }

        public  string                          PresetName { get; set; }
        public  ObservableCollection<string>    Libraries { get; }

        public PresetControlViewModel( IMilkController milkController, IPresetController presetController, IPresetLibrarian librarian, IEventAggregator eventAggregator ) {
            mMilkController = milkController;
            mPresetController = presetController;
            mLibrarian = librarian;
            mEventAggregator = eventAggregator;

            Libraries = new ObservableCollection<string>();
            mCurrentLibrary = String.Empty;

            Start = new DelegateCommand( OnStart );
            Stop = new DelegateCommand( OnStop );
            NextPreset = new DelegateCommand( OnNextPreset );
            PreviousPreset = new DelegateCommand( OnPreviousPreset );

            mPresetSubscription = mPresetController.CurrentPreset.Subscribe( OnPresetChanged );

            mCurrentLibrary = mPresetController.CurrentPresetLibrary;
            UpdateLibraries();

            mEventAggregator.Subscribe( this );
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

            if(( String.IsNullOrWhiteSpace( mCurrentLibrary )) ||
               (!Libraries.Contains( mCurrentLibrary ))) {
                mCurrentLibrary = Libraries.FirstOrDefault();

                RaisePropertyChanged( () => CurrentLibrary );
            }
        }

        private void OnPresetChanged( MilkDropPreset preset ) {
            PresetName = preset.PresetName;

            RaisePropertyChanged( () => PresetName );
        }

        public bool IsLocked {
            get => !mPresetController.RandomPresetCycling;
            set => mPresetController.RandomPresetCycling = !value;
        }

        public bool IsBlended {
            get => mPresetController.BlendPresetTransition;
            set => mPresetController.BlendPresetTransition = value;
        }

        public int PresetDuration {
            get => mPresetController.PresetDuration;
            set => mPresetController.PresetDuration = value;
        }

        private void OnStart() {
            mMilkController.StartVisualization();
            mPresetController.StartPresetCycling();
        }

        private void OnStop() {
            mMilkController.StopVisualization();
            mPresetController.StopPresetCycling();
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
