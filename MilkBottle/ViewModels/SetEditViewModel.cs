using System;
using System.Collections.Generic;
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
        private bool                            mUseFavoriteQualifier;
        private bool                            mUseNameQualifier;
        private string                          mNameQualifier;

        public  ObservableCollection<PresetSet> Sets {  get; }

        public  DelegateCommand                 CreateSet { get; }

        public SetEditViewModel( IPresetSetProvider setProvider, IDialogService dialogService, IPlatformLog log ) {
            mSetProvider = setProvider;
            mDialogService = dialogService;
            mLog = log;

            Sets = new ObservableCollection<PresetSet>();
            CreateSet = new DelegateCommand( OnCreateSet );

            LoadSets();
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
            var currentSet = mCurrentSet;

            Sets.Clear();

            mSetProvider.SelectSets( list => Sets.AddRange( from s in list orderby s.Name select s ));

            if( currentSet != null ) {
                CurrentSet = Sets.FirstOrDefault( s => s.Id.Equals( currentSet.Id ));
            }
        }

        private void OnSetChanged() {
            DisplaySetQualifiers();
        }

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
                            ex => LogException( "OnCreateSet", ex ));
                }
            }
        }

        public bool UseFavoriteQualifier {
            get => mUseFavoriteQualifier;
            set {
                mUseFavoriteQualifier = value;

                OnFavoriteQualifierChanged();
                RaisePropertyChanged( () => UseFavoriteQualifier );
            }
        }

        public bool UseNameQualifier {
            get => mUseNameQualifier;
            set {
                mUseNameQualifier = value;

                OnNameQualifierChanged();
                RaisePropertyChanged( () => UseNameQualifier );
            }
        }

        public string NameQualifier {
            get => mNameQualifier;
            set {
                mNameQualifier = value;

                OnNameQualifierChanged();
                RaisePropertyChanged( () => UseNameQualifier );
            }
        }

        private void OnFavoriteQualifierChanged() {
            if( mCurrentSet != null ) {
                var preset = mUseFavoriteQualifier ? 
                    mCurrentSet.WithQualifier( new SetQualifier( QualifierField.IsFavorite, QualifierOperation.Equal, true.ToString())) :
                    mCurrentSet.WithoutQualifier( QualifierField.IsFavorite );

                mSetProvider.Update( preset )
                    .Match(
                        unit => LoadSets(),
                        ex => LogException( "OnFavoriteQualifierChanged", ex )
                        );

                PeekAtPresets();
            }
        }

        private void OnNameQualifierChanged() {
            if(( mCurrentSet != null ) &&
               (!String.IsNullOrWhiteSpace( mNameQualifier ))) {
                var preset = mUseNameQualifier ?
                    mCurrentSet.WithQualifier( new SetQualifier( QualifierField.Name, QualifierOperation.Contains, mNameQualifier )) :
                    mCurrentSet.WithoutQualifier( QualifierField.Name );

                mSetProvider.Update( preset )
                    .Match(
                        unit => LoadSets(),
                        ex => LogException( "OnNameQualifierChanged", ex )
                    );

                PeekAtPresets();
            }
        }

        private void DisplaySetQualifiers() {
            mUseFavoriteQualifier = false;
            mUseNameQualifier = false;
            mNameQualifier = String.Empty;

            mCurrentSet?.Qualifiers.ForEach( q => {
                switch( q.Field ) {
                    case QualifierField.IsFavorite:
                        mUseFavoriteQualifier = true;
                        break;

                    case QualifierField.Name:
                        mUseNameQualifier = true;
                        mNameQualifier = q.Value;
                        break;
                }
            });

            RaisePropertyChanged( () => UseFavoriteQualifier );
            RaisePropertyChanged( () => UseNameQualifier );
            RaisePropertyChanged( () => NameQualifier );
        }

        private void LogException( string message, Exception ex ) {
            mLog.LogException( message, ex );
        }

        private void PeekAtPresets() {
            if( mCurrentSet != null ) {
                var presetList = new List<Preset>();

                mSetProvider.GetPresetList( mCurrentSet, list => presetList.AddRange( list ))
                    .Match( 
                        init => { },
                        ex => mLog.LogException( "PeekAtPresets", ex ));

                var count = presetList.Count;
            }
        }
    }
}
