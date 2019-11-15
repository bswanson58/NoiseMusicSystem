﻿using System;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.Dto {
    class DisplayedProcessItem : AutomaticCommandBase {
        private readonly Action<DisplayedProcessItem> mContinueOnError;
        private readonly Action<DisplayedProcessItem> mOpenAction;

        private string          mCurrentHandler;
        private ProcessState    mCurrentState;

        public  string          Key {  get; }
        public  string          Name { get; }
        public  string          FileName { get; }
        public  string          ProcessOutput { get; private set; }
        public  bool            OutputFilePresent { get; private set; }

        public  bool            IsRunning => CurrentState == ProcessState.Running;
        public  bool            IsPending => CurrentState == ProcessState.Pending;
        public  bool            HasCompleted => CurrentState == ProcessState.Completed;
        public  bool            HasError => CurrentState == ProcessState.Error;

        public DisplayedProcessItem( ProcessItem item, Action<DisplayedProcessItem> onContinuation, Action<DisplayedProcessItem> openAction ) {
            Key = item.Key;
            Name = item.Name;
            FileName = item.FileName;
            mContinueOnError = onContinuation;
            mOpenAction = openAction;

            CurrentHandler = "File Copied";
            CurrentState = ProcessState.Pending;
        }

        public string CurrentHandler {
            get => mCurrentHandler;
            set {
                mCurrentHandler = value;

                RaisePropertyChanged( () => CurrentHandler );
            }
        }

        public ProcessState CurrentState {
            get => mCurrentState;
            set {
                mCurrentState = value;

                RaisePropertyChanged( () => CurrentState );
                RaisePropertyChanged( () => IsPending );
                RaisePropertyChanged( () => IsRunning );
                RaisePropertyChanged( () => HasCompleted );
                RaisePropertyChanged( () => HasError );
                RaisePropertyChanged( () => OutputFilePresent );
            }
        }

        public void SetProcessOutput( string output, bool outputPresent ) {
            ProcessOutput = output;
            OutputFilePresent = outputPresent;

            RaisePropertyChanged( () => ProcessOutput );
            RaisePropertyChanged( () => OutputFilePresent );
        }

        public void Execute_OnContinue() {
            mContinueOnError?.Invoke( this );
        }

        public void Execute_OpenFolder() {
            mOpenAction?.Invoke( this );
        }
    }
}
