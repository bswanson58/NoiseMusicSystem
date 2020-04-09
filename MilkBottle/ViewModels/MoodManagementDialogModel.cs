using System;
using System.Collections.ObjectModel;
using System.Linq;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace MilkBottle.ViewModels {
    class MoodManagementDialogModel : IDialogAware {
        public  const string                    cMoodListParameter = "moodList";

        private readonly IMoodProvider          mMoodProvider;
        private readonly IPlatformLog           mLog;

        public  string                          Title { get; }
        public  event Action<IDialogResult>     RequestClose;

        public  ObservableCollection<UiMood>    MoodList { get; }

        public  DelegateCommand                 AddMood { get; }
        public  DelegateCommand                 Ok { get; }
        public  DelegateCommand                 Cancel { get; }

        public MoodManagementDialogModel( IMoodProvider moodProvider, IPlatformLog log ) {
            mMoodProvider = moodProvider;
            mLog = log;

            MoodList = new ObservableCollection<UiMood>();

            AddMood = new DelegateCommand( OnAddMood );
            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

            Title = "Mood Selection";
        }

        private void OnAddMood() { }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnDialogOpened( IDialogParameters parameters ) {
            LoadMoods();
        }

        public void OnOk() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        public virtual void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
        private void LoadMoods() {
            MoodList.Clear();

            mMoodProvider.SelectMoods( list => MoodList.AddRange( from m in list orderby m.Name select new UiMood( m )))
                .IfLeft( ex => mLog.LogException( "MoodManagement:SelectMoods", ex ));
        }
    }
}
