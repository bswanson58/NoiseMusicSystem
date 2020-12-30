using System;
using System.IO;
using System.Linq;
using DynamicData.Binding;
using Noise.RemoteClient.Interfaces;
using Prism.Commands;
using Prism.Mvvm;

namespace Noise.RemoteClient.ViewModels {
    class LogFileInfo {
        public  string  FileName { get; }
        public  string  FilePath { get; }

        public LogFileInfo( string path ) {
            FilePath = path;
            FileName = Path.GetFileName( path );
        }
    }

    class LogDisplayViewModel : BindableBase {
        private readonly IClientEnvironment     mEnvironment;
        private LogFileInfo                     mCurrentLogFile;

        public  string                                      LogText { get; private set; }
        public  ObservableCollectionExtended<LogFileInfo>   LogFiles { get; }

        public  DelegateCommand                             DeleteLog { get; }

        public LogDisplayViewModel( IClientEnvironment environment ) {
            mEnvironment = environment;

            LogText = String.Empty;
            LogFiles = new ObservableCollectionExtended<LogFileInfo>();

            DeleteLog = new DelegateCommand( OnDeleteLog, CanDeleteLogFile );

            LoadLogFiles();
        }

        public LogFileInfo CurrentLog {
            get => mCurrentLogFile;
            set => SetProperty( ref mCurrentLogFile, value, OnLogFileSelected );
        }

        private void OnLogFileSelected() {
            if( mCurrentLogFile != null ) {
                LoadLog( Path.Combine( mEnvironment.LogDirectory, mCurrentLogFile.FilePath ));
            }

            DeleteLog.RaiseCanExecuteChanged();
        }

        private void OnDeleteLog() {
            if( mCurrentLogFile != null ) {
                try {
                    var fileName = mCurrentLogFile.FileName;

                    File.Delete( mCurrentLogFile.FilePath );

                    LoadLogFiles();

                    LogText = $"  Log file '{fileName}' was deleted.";
                }
                catch( Exception ex ) {
                    LogText = $"Log could not be deleted: {ex.Message}";
                }
            }

            RaisePropertyChanged( nameof( LogText ));
        }

        private bool CanDeleteLogFile() {
            return mCurrentLogFile != null;
        }

        private void LoadLog( string path ) {
            LogText = String.Empty;

            try {
                if( File.Exists( path )) {
                    LogText = File.ReadAllText( path );
                }
            }
            catch( Exception ) {
                LogText = "Log file could not be read.";
            }

            RaisePropertyChanged( nameof( LogText ));
        }

        private void LoadLogFiles() {
            var logDirectory = mEnvironment.LogDirectory;

            LogFiles.Clear();

            if( Directory.Exists( logDirectory )) {
                LogFiles.AddRange( from f in Directory.GetFiles( logDirectory ) select new LogFileInfo( f ));
            }
        }
    }
}
