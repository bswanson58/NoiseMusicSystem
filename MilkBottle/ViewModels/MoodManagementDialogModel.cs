using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Views;
using MoreLinq;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace MilkBottle.ViewModels {
    class MoodManagementDialogModel : IDialogAware {
        public  const string                    cMoodListParameter = "moodList";

        private readonly IMoodProvider          mMoodProvider;
        private readonly IDialogService         mDialogService;
        private readonly IPlatformLog           mLog;
        private readonly List<Mood>             mSelectedMoods;
        private UiMood                          mSelectedMood;

        public  string                          Title { get; }
        public  event Action<IDialogResult>     RequestClose;

        public  ObservableCollection<UiMood>    MoodList { get; }

        public  DelegateCommand                 AddMood { get; }
        public  DelegateCommand                 Ok { get; }
        public  DelegateCommand                 Cancel { get; }

        public MoodManagementDialogModel( IMoodProvider moodProvider, IDialogService dialogService, IPlatformLog log ) {
            mMoodProvider = moodProvider;
            mDialogService = dialogService;
            mLog = log;

            MoodList = new ObservableCollection<UiMood>();
            mSelectedMoods = new List<Mood>();

            AddMood = new DelegateCommand( OnAddMood );
            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

            Title = "Mood Selection";
        }

        private void OnAddMood() {
            mDialogService.ShowDialog( nameof( NewMoodDialog ), new DialogParameters(), OnAddMoodResult );
        }

        private void OnAddMoodResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var moodName = result.Parameters.GetValue<string>( NewMoodDialogModel.cMoodNameParameter );

                if(!String.IsNullOrWhiteSpace( moodName )) {
                    var mood = new Mood( moodName );

                    mMoodProvider.Insert( mood ).IfLeft( ex => mLog.LogException( "OnAddMoodResult.Insert", ex ));

                    LoadMoods();
                }
            }
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnDialogOpened( IDialogParameters parameters ) {
            var selectedList = parameters.GetValue<IEnumerable<Mood>>( cMoodListParameter );

            mSelectedMoods.Clear();
            if( selectedList != null ) {
                mSelectedMoods.AddRange( selectedList );
            }

            LoadMoods();
        }

        public void OnOk() {
            var parameters = new DialogParameters {{ cMoodListParameter, mSelectedMoods } };

            RaiseRequestClose( new DialogResult( ButtonResult.OK, parameters ));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        public virtual void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }

        private void LoadMoods() {
            MoodList.Clear();

            mMoodProvider.SelectMoods( list => MoodList.AddRange( from m in list orderby m.Name select new UiMood( m, OnChecked, OnEdit, OnDelete )))
                .IfLeft( ex => mLog.LogException( "MoodManagement:SelectMoods", ex ));

            SetSelectedMoods();
        }

        private void SetSelectedMoods() {
            MoodList.ForEach( m => m.SetChecked( mSelectedMoods.FirstOrDefault( s => s.Identity.Equals( m.Mood.Identity )) != null ));
        }

        private void OnChecked( UiMood mood ) {
            if( mood.IsChecked ) {
                mSelectedMoods.Add( mood.Mood );
            }
            else {
                var currentMood = mSelectedMoods.FirstOrDefault( m => m.Identity.Equals( mood.Mood.Identity ));

                if( currentMood != null ) {
                    mSelectedMoods.Remove( currentMood );
                }
            }
        }

        private void OnEdit( UiMood mood ) {
            mSelectedMood = mood;

            var parameters = new DialogParameters{{ NewMoodDialogModel.cMoodNameParameter, mood.Name }};

            mDialogService.ShowDialog( nameof( NewMoodDialog ), parameters, OnEditResult );
        }

        private void OnEditResult( IDialogResult result ) {
            if(( mSelectedMood != null) &&
               ( result.Result == ButtonResult.OK )) {
                var newName = result.Parameters.GetValue<string>( NewMoodDialogModel.cMoodNameParameter );

                if(!String.IsNullOrWhiteSpace( newName )) {
                    var newMood = mSelectedMood.Mood.WithName( newName );

                    mMoodProvider.Update( newMood ).IfLeft( ex => mLog.LogException( "OnEditMoodResult.Update", ex ));

                    LoadMoods();
                }
            }
        }

        private void OnDelete( UiMood mood ) {
            mSelectedMood = mood;

            var parameters = new DialogParameters{{ ConfirmDeleteDialogModel.cEntityNameParameter, mSelectedMood.Name } };

            mDialogService.ShowDialog( nameof( ConfirmDeleteDialog ), parameters, OnDeleteResult );
        }

        private void OnDeleteResult( IDialogResult result ) {
            if(( mSelectedMood != null ) &&
               ( result.Result == ButtonResult.OK )) {
                mMoodProvider.Delete( mSelectedMood.Mood ).IfLeft( ex => mLog.LogException( "OnDeleteMoodResult.Delete", ex ));

                LoadMoods();
            }
        }
    }
}
