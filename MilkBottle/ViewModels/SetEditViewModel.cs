using System;
using System.Collections.ObjectModel;
using System.Linq;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Views;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class SetEditViewModel : PropertyChangeBase {
        private readonly IPresetSetProvider     mSetProvider;
        private readonly IDialogService         mDialogService;
        private readonly IPlatformLog           mLog;
        private PresetSet                       mCurrentSet;

        public  ObservableCollection<PresetSet> Sets {  get; }

        public  DelegateCommand                 CreateSet { get; }

        public SetEditViewModel( IPresetSetProvider setProvider, IDialogService dialogService, IPlatformLog log ) {
            mSetProvider = setProvider;
            mDialogService = dialogService;
            mLog = log;

            Sets = new ObservableCollection<PresetSet>();

            CreateSet = new DelegateCommand( OnCreateSet );
        }

        public PresetSet CurrentSet {
            get => mCurrentSet;
            set {
                mCurrentSet = value;

                OnSetChanged();
                RaisePropertyChanged( () => CurrentSet );
            }
        }

        private void LoadSets() {
            Sets.Clear();

            mSetProvider.SelectSets( list => Sets.AddRange( from s in list orderby s.Name select s ));
        }

        private void OnSetChanged() { }

        private void OnCreateSet() {
            mDialogService.ShowDialog( nameof( NewSetDialog ), new DialogParameters(), OnCreateSetResult  );
        }

        private void OnCreateSetResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var setName = result.Parameters.GetValue<string>( NewSetDialogModel.cSetNameParameter );

                if(!String.IsNullOrWhiteSpace( setName )) {
                    mSetProvider.Insert( new PresetSet( setName ))
                        .Match( 
                            unit => LoadSets(),
                            ex => mLog.LogException( "OnCreateSet", ex ));
                }
            }
        }
    }
}
