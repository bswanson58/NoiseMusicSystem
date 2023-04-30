using System;
using System.Collections.ObjectModel;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;
using TuneRenamer.Dto;
using TuneRenamer.Interfaces;

namespace TuneRenamer.ViewModels {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ReplacementWordsViewModel : PropertyChangeBase, IDialogAware {
        private readonly IPreferences           mPreferences;
        private readonly IPlatformLog           mLog;

        public  string                          Title { get; }
        public  event Action<IDialogResult>     RequestClose;

        public  ObservableCollection<WordReplacement>   WordReplacements { get; }

        public  DelegateCommand                 Ok { get; }
        public  DelegateCommand                 Cancel { get; }

        public ReplacementWordsViewModel( IPreferences preferences, IPlatformLog log ) {
            mPreferences = preferences;
            mLog = log;
            Title = "Replacement Word Pairs";

            WordReplacements = new ObservableCollection<WordReplacement>();

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            var replacements = mPreferences.Load<WordReplacements>();

            WordReplacements.Clear();
            WordReplacements.AddRange( replacements.ReplacementList );
        }

        private void OnOk() {
            try {
                var replacements = new WordReplacements();

                replacements.ReplacementList.AddRange( WordReplacements );
                mPreferences.Save( replacements );
            }
            catch( Exception ex ) {
                mLog.LogException( String.Empty, ex );
            }

            RequestClose?.Invoke( new DialogResult( ButtonResult.OK ));
        }

        private void OnCancel() {
            RequestClose?.Invoke( new DialogResult( ButtonResult.Cancel ));
        }

        public bool CanCloseDialog() =>
            true;

        public void OnDialogClosed() { }
    }
}
