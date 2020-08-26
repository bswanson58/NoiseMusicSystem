using System;
using System.Collections.ObjectModel;
using System.Linq;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;
using MilkBottle.Models;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class BoostMode {
        public  string  Name { get; }
        public  int     Mode { get; }

        public BoostMode( string name, int mode ) {
            Name = name;
            Mode = mode;
        }
    }

    class MoodSelectionDialogModel : PropertyChangeBase, IDialogAware {
        private readonly IMoodProvider          mMoodProvider;
        private readonly IPreferences           mPreferences;
        private readonly IPlatformLog           mLog;
        private Mood                            mCurrentMood;

        public  string                          Title { get; }
        public  event Action<IDialogResult>     RequestClose;

        public  ObservableCollection<Mood>      MoodList { get; }
        public  ObservableCollection<BoostMode> BoostModeList { get; }
        public  BoostMode                       SelectedMode { get; set; }

        public  DelegateCommand                 Ok {  get; }
        public  DelegateCommand                 Cancel { get; }

        public MoodSelectionDialogModel( IMoodProvider moodProvider, IPreferences preferences, IPlatformLog log ) {
            mMoodProvider = moodProvider;
            mPreferences = preferences;
            mLog = log;

            MoodList = new ObservableCollection<Mood>();
            BoostModeList = new ObservableCollection<BoostMode> {
                                    new BoostMode( "Prefer Music Over Mood", RatingsBoostMode.PreferMusicOverMood ),
                                    new BoostMode( "Prefer Mood Over Music", RatingsBoostMode.PreferMoodOverMusic ) };

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

            Title = "Mood Selection";
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            var preferences = mPreferences.Load<MilkPreferences>();

            LoadMoods();

            if(!String.IsNullOrWhiteSpace( preferences.CurrentMood )) {
                mCurrentMood = MoodList.FirstOrDefault( m => m.Identity.ToString().Equals( preferences.CurrentMood ));
            }

            if( mCurrentMood == null ) {
                mCurrentMood = MoodList.FirstOrDefault();
            }

            SelectedMode = BoostModeList.FirstOrDefault( m => m.Mode == preferences.SceneRatingsBoostMode ) ??
                           BoostModeList.FirstOrDefault();

            RaisePropertyChanged( () => SelectedMood );
            RaisePropertyChanged( () => SelectedMode );
        }

        public Mood SelectedMood {
            get => mCurrentMood;
            set {
                mCurrentMood = value;

                RaisePropertyChanged( () => SelectedMood );
            }
        }

        private void LoadMoods() {
            mMoodProvider.SelectMoods( list => MoodList.AddRange( from m in list orderby m.Name select m )).IfLeft( ex => mLog.LogException( "LoadMoods.SelectMoods", ex ));
        }

        private void OnOk() {
            if( mCurrentMood != null ) {
                var preferences = mPreferences.Load<MilkPreferences>();

                preferences.CurrentMood = mCurrentMood.Identity;
                if( SelectedMode != null ) {
                    preferences.SceneRatingsBoostMode = SelectedMode.Mode;
                }

                mPreferences.Save( preferences );

                RaiseRequestClose( new DialogResult( ButtonResult.OK ));
            }
            else {
                RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
            }
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }
    }
}
